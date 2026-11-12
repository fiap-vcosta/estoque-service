# estoque-service

[![Quality Gate](https://sonarcloud.io/api/project_badges/quality_gate?project=fiap-vcosta_estoque-service)](https://sonarcloud.io/summary/new_code?id=fiap-vcosta_estoque-service)
[![Reliability](https://sonarcloud.io/api/project_badges/measure?project=fiap-vcosta_estoque-service&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=fiap-vcosta_estoque-service)
[![Security](https://sonarcloud.io/api/project_badges/measure?project=fiap-vcosta_estoque-service&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=fiap-vcosta_estoque-service)
[![Maintainability](https://sonarcloud.io/api/project_badges/measure?project=fiap-vcosta_estoque-service&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=fiap-vcosta_estoque-service)
[![Duplications](https://sonarcloud.io/api/project_badges/measure?project=fiap-vcosta_estoque-service&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=fiap-vcosta_estoque-service)

Serviço de estoque da oficina: item, entrada, reserva com `FOR UPDATE`, consumo, liberação e fila de pendências pela ordem de aprovação. Publica `EstoqueReservado` / `EstoqueEmFalta` e consome aprovação, finalização e descarte da ordem de serviço.

Stack: .NET 8, PostgreSQL, Google Pub/Sub (building-blocks). Org [fiap-vcosta](https://github.com/fiap-vcosta).

Requisitos: [docs/01_requisitos.md](docs/01_requisitos.md). Contratos: [eventos](https://github.com/fiap-vcosta/e2e/blob/main/docs/eventos.md), [HTTP](https://github.com/fiap-vcosta/e2e/blob/main/docs/contratos-http.md).

## Local

```bash
cp .env.example .env
# PAT GitHub com read:packages → NUGET_AUTH_TOKEN
./scripts/setup-local-jwt.sh
./scripts/mint-staff-jwt.sh   # cola o JWT no Requestly (variável token)

docker compose up db pubsub -d
dotnet restore TechChallenge.sln
dotnet run --project src/Api/Api.csproj
# ou: docker compose --profile app up --build
```

- API: `http://localhost:8082` (Swagger `/swagger`)
- Health: `GET /estoque/health`
- Requestly: [docs/requestly/estoque.requestly.json](docs/requestly/estoque.requestly.json)

Workers Pub/Sub no Development usam transport in-memory (`PubSub__UseInMemory=true`) com `EnableWorkers=false` por padrão. Para exercitar reserva/pendência nos testes, use a suite de integração ou chame `ReservaEstoqueService` via testes. No Compose, o emulador sobe em `:8085` para validação futura com `PUBSUB_EMULATOR_HOST`.

## Testes

```bash
dotnet test tests/UnitTests/UnitTests.csproj -c Release \
  "/p:CollectCoverage=true" \
  "/p:CoverletOutputFormat=cobertura" \
  "/p:Include=[Domain]*%2c[Application]*%2c[Infrastructure]*%2c[Api]*" \
  "/p:Exclude=[*]*Migrations.*" \
  "/p:Threshold=80" \
  "/p:ThresholdType=line%2Cbranch" \
  "/p:ThresholdStat=minimum"
dotnet test tests/IntegrationTests/IntegrationTests.csproj -c Release
```

## Service auth (malha)

`ServiceAuth__Key` / `X-Service-Key` vem do org secret `SERVICE_AUTH_KEY` (mesmo valor em todos os serviços).

## Terraform (GCP)

Root module: [`terraform/`](terraform/). Cria a própria instância Cloud SQL (`estoque-service-pg`, `db-f1-micro`); VPC e SA de runtime via [`infra-bootstrap`](https://github.com/fiap-vcosta/infra-bootstrap). Decisão de sistema: [ADR no e2e](https://github.com/fiap-vcosta/e2e/blob/main/docs/adrs/001-instancia-cloud-sql-por-servico.md).

## Deploy (GCP)

`tf-apply` / `deploy` manuais (`workflow_dispatch`). Smoke: `/estoque/health` + CRUD staff via Gateway; workers com publish manual nos tópicos após o `demo-up`.
