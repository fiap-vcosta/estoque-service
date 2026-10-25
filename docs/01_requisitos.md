# Requisitos — estoque-service

Sessão de Outubro de 2026. Este serviço guarda item de estoque, entrada, reserva, consumo, liberação e a fila de pendências. Persistência em PostgreSQL. Não é dono de ordem de serviço, orçamento, catálogo administrativo nem de emissão de JWT.

Payloads, tópicos e chaves de idempotência: [eventos](https://github.com/fiap-vcosta/e2e/blob/main/docs/eventos.md). Paths: [HTTP](https://github.com/fiap-vcosta/e2e/blob/main/docs/contratos-http.md).

## 1. Paths

Rotas públicas exigem JWT staff. O recurso chama-se `itens`.

| Método | Path |
|--------|------|
| `POST` | `/estoque/itens` |
| `GET` | `/estoque/itens` |
| `GET` | `/estoque/itens/{id}` |
| `PUT` | `/estoque/itens/{id}` |
| `DELETE` | `/estoque/itens/{id}` |
| `POST` | `/estoque/itens/{id}/registrar-entrada` |

`GET /system/estoque/itens/{id}` existe só na rede interna, com header `X-Service-Key`. Chave ausente ou diferente responde 401. A resposta leva `id`, `codigo`, `nome` e `unidadeMedida` (`Unidade`, `Jogo`, `Par`, `Litro`, `Kg`, `mL`, `Frasco`). Não leva saldo, saldo reservado nem preço. Ausente responde 404.

## 2. Reserva

* **RF01:** Ao consumir `OrdemServicoAprovada`, o serviço tenta reservar os itens dos serviços aprovados. A reserva trava a linha do item (`FOR UPDATE`). O saldo disponível é `Saldo - SaldoReservado`.
* **RF01.1:** Linhas com o mesmo `itemEstoqueId` na mesma ordem compartilham um saldo. A quantidade vai sendo descontada linha a linha. Duas linhas não podem as duas enxergar o saldo cheio.
* **RF01.2:** A reserva é toda ou nada. Se todas as linhas couberem, o serviço soma a quantidade em `SaldoReservado` e publica `EstoqueReservado`, com cada item em `EstoqueTravado`.
* **RF01.3:** Se alguma linha não couber, nada é travado. O serviço publica `EstoqueEmFalta` uma vez para a chave `ordemServicoId` + `aprovadaEm`, com cada linha em `EstoqueDisponivel` ou `EstoqueEmFalta`, e guarda a pendência.

## 3. Fila de pendências

* **RF02:** A pendência fica neste serviço. A prioridade é `aprovadaEm` crescente (a aprovação mais antiga primeiro).
* **RF02.1:** `POST /estoque/itens/{id}/registrar-entrada` reavalia as ordens pendentes que têm aquele item em falta, nessa ordem. Cada reavaliação refaz a checagem da ordem inteira, com a regra do RF01.1.
* **RF02.2:** A ordem que passar a caber é reservada e gera `EstoqueReservado`. A que continuar sem saldo não gera outro `EstoqueEmFalta`.

## 4. Consumo e liberação

* **RF03:** `OrdemServicoFinalizada` baixa `Saldo` e `SaldoReservado` das quantidades reservadas para aquela ordem, uma vez. Confirmação parcial de execução, que ainda não publica esse evento, não baixa o saldo.
* **RF04:** `OrdemServicoDescartada` libera a reserva se ela existir (`SaldoReservado` diminui, `Saldo` permanece) e tira a ordem da fila. Sem reserva e sem pendência, a mensagem é reconhecida e não muda saldo.
* **RF04.1:** Se o descarte dessa ordem já foi aplicado, um `OrdemServicoAprovada` posterior não reserva.

## 5. Idempotência

* **RNF01:** Reentrega do mesmo `eventId` não muda estado. A mesma chave de negócio, ainda que com outro `eventId`, também não é aplicada de novo.
