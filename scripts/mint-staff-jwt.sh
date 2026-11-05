#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
private_key="${JWT_PRIVATE_KEY_PATH:-$root/.local/jwt/private.pem}"
issuer="${JWT_ISSUER:-estoque-local}"
audience="${JWT_AUDIENCE:-estoque-local}"

if [[ ! -f "$private_key" ]]; then
  echo "Chave privada não encontrada em $private_key" >&2
  echo "Rode: ./scripts/setup-local-jwt.sh" >&2
  exit 1
fi

header='{"alg":"RS256","typ":"JWT"}'
now="$(date +%s)"
exp="$((now + 3600))"
payload="$(printf '{"iss":"%s","aud":"%s","iat":%s,"exp":%s,"role":"Admin","sub":"local-staff"}' \
  "$issuer" "$audience" "$now" "$exp")"

b64url() {
  openssl base64 -e -A | tr '+/' '-_' | tr -d '='
}

h="$(printf '%s' "$header" | b64url)"
p="$(printf '%s' "$payload" | b64url)"
sig="$(printf '%s' "$h.$p" | openssl dgst -sha256 -sign "$private_key" -binary | b64url)"
printf '%s.%s.%s\n' "$h" "$p" "$sig"
