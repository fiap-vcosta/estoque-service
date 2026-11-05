# Smoke GCP — estoque-service (§9)

Pré-requisitos: PRs mergeados; `infra-events` com tópicos; `demo-up` no `e2e` verde (inclui TF + deploy do estoque).

## 1. Health + CRUD (Gateway)

1. Requestly environment **GCP-Gateway** (`https://vcosta-fiap.online`).
2. Login staff no `autenticacao-service` → token.
3. `GET /estoque/health`
4. `POST /estoque/itens` (saldo alto, ex. 10) e anote `id`.
5. `GET /estoque/itens/{id}`

System (não passa no API Gateway público):

```bash
kubectl -n tech-challenge port-forward svc/estoque-service 18082:80
curl -sH "X-Service-Key: $SERVICE_AUTH_KEY" \
  http://127.0.0.1:18082/system/estoque/itens/1
```

## 2. Publicar `OrdemServicoAprovada` (reserva)

```bash
PROJECT=vcosta-fiap-tech-challenge
EVENT_ID=$(uuidgen)
PAYLOAD=$(cat <<JSON
{
  "eventId": "$EVENT_ID",
  "type": "OrdemServicoAprovada",
  "version": 1,
  "occurredAt": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
  "data": {
    "ordemServicoId": 9001,
    "aprovadaEm": "2026-11-05T18:00:00Z",
    "itens": [{ "itemEstoqueId": 1, "quantidade": 2 }]
  }
}
JSON
)

gcloud pubsub topics publish ordem-servico-aprovada \
  --project="$PROJECT" \
  --attribute=eventType=OrdemServicoAprovada \
  --message="$PAYLOAD"
```

Esperado: mensagem em `estoque-reservado` (ou `estoque-em-falta` se saldo insuficiente); `SaldoReservado` sobe no CRUD.

Reentrega: publique de novo com o **mesmo** `eventId` → sem mudança.

## 3. Entrada reavalia pendência

Se o passo 2 gerou falta: `POST /estoque/itens/{id}/registrar-entrada` com quantidade suficiente → `estoque-reservado`.

## 4. Finalização / descarte

```bash
# Consumo
gcloud pubsub topics publish ordem-servico-finalizada --project="$PROJECT" \
  --attribute=eventType=OrdemServicoFinalizada \
  --message="{\"eventId\":\"$(uuidgen)\",\"type\":\"OrdemServicoFinalizada\",\"version\":1,\"occurredAt\":\"$(date -u +%Y-%m-%dT%H:%M:%SZ)\",\"data\":{\"ordemServicoId\":9001}}"

# Ou descarte (cenário separado, outra OS)
gcloud pubsub topics publish ordem-servico-descartada --project="$PROJECT" \
  --attribute=eventType=OrdemServicoDescartada \
  --message="{\"eventId\":\"$(uuidgen)\",\"type\":\"OrdemServicoDescartada\",\"version\":1,\"occurredAt\":\"$(date -u +%Y-%m-%dT%H:%M:%SZ)\",\"data\":{\"ordemServicoId\":9002}}"
```

## 5. Descida

`demo-down` no `e2e` → conferir DB/user/secrets/subscriptions do estoque removidos.
