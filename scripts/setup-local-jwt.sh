#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
dir="$root/.local/jwt"
mkdir -p "$dir"

if [[ ! -f "$dir/private.pem" ]]; then
  openssl genrsa -out "$dir/private.pem" 2048
  openssl rsa -in "$dir/private.pem" -pubout -out "$dir/public.pem"
  chmod 600 "$dir/private.pem"
  echo "Par RSA de teste criado em $dir (não versionado)."
else
  echo "Par RSA já existe em $dir — mantido."
fi

pub_escaped="$(python3 - <<PY
from pathlib import Path
print(Path("$dir/public.pem").read_text().strip().replace("\n", r"\n"))
PY
)"

env_file="$root/.env"
if [[ ! -f "$env_file" ]]; then
  cat >"$env_file" <<EOF
ConnectionStrings__DefaultConnection=Host=localhost;Database=estoque;Username=postgres;Password=postgres;Maximum Pool Size=15
POSTGRES_DB=estoque
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
ASPNETCORE_ENVIRONMENT=Development
SERVICE_AUTH_KEY=local-service-auth-key-change-me
JWT_ISSUER=estoque-local
JWT_AUDIENCE=estoque-local
JWT_PUBLIC_KEY_PEM=${pub_escaped}
NUGET_AUTH_TOKEN=
DD_API_KEY=
DD_SITE=datadoghq.com
EOF
  echo "Criado $env_file — preencha NUGET_AUTH_TOKEN (PAT read:packages) para docker build."
else
  echo "$env_file já existe — atualize JWT_PUBLIC_KEY_PEM manualmente se regenerou as chaves."
fi
