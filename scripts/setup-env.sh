#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
EXAMPLE_FILE="$ROOT_DIR/.env.example"
ENV_FILE="$ROOT_DIR/.env"

FORCE=false
if [[ "${1:-}" == "--force" ]]; then
  FORCE=true
fi

random_secret() {
  local length="$1"
  if command -v openssl >/dev/null 2>&1; then
    openssl rand -base64 64 | tr -dc 'A-Za-z0-9' | head -c "$length"
  else
    tr -dc 'A-Za-z0-9' </dev/urandom | head -c "$length"
  fi
}

generate_passwords() {
  POSTGRES_PASSWORD="$(random_secret 24)"
  JWT_SECRET="$(random_secret 48)"
  SUPERADMIN_PASSWORD="$(random_secret 10)A1$(random_secret 4)"
}

is_placeholder() {
  local value="$1"
  local min_length="$2"
  [[ -z "$value" ]] && return 0
  [[ "$value" == change_me* ]] && return 0
  [[ "${#value}" -lt "$min_length" ]] && return 0
  return 1
}

read_env_value() {
  local key="$1"
  if [[ ! -f "$ENV_FILE" ]]; then
    echo ""
    return
  fi
  grep -E "^${key}=" "$ENV_FILE" | head -n1 | cut -d= -f2- || true
}

apply_secrets_to_env() {
  if [[ "$(uname)" == "Darwin" ]]; then
    sed -i '' "s|^POSTGRES_PASSWORD=.*|POSTGRES_PASSWORD=${POSTGRES_PASSWORD}|" "$ENV_FILE"
    sed -i '' "s|^Jwt__Secret=.*|Jwt__Secret=${JWT_SECRET}|" "$ENV_FILE"
    sed -i '' "s|^SuperAdmin__Password=.*|SuperAdmin__Password=${SUPERADMIN_PASSWORD}|" "$ENV_FILE"
  else
    sed -i "s|^POSTGRES_PASSWORD=.*|POSTGRES_PASSWORD=${POSTGRES_PASSWORD}|" "$ENV_FILE"
    sed -i "s|^Jwt__Secret=.*|Jwt__Secret=${JWT_SECRET}|" "$ENV_FILE"
    sed -i "s|^SuperAdmin__Password=.*|SuperAdmin__Password=${SUPERADMIN_PASSWORD}|" "$ENV_FILE"
  fi
}

print_summary() {
  local action="$1"
  echo "${action} $ENV_FILE with generated secrets."
  echo ""
  echo "All passwords are stored in .env (never commit this file):"
  echo "  POSTGRES_PASSWORD"
  echo "  Jwt__Secret"
  echo "  SuperAdmin__Password"
  echo ""
  echo "Super admin login:"
  grep '^SuperAdmin__Email=' "$ENV_FILE" | cut -d= -f2-
  echo "  password: (see SuperAdmin__Password in .env)"
}

needs_secret_update() {
  is_placeholder "$(read_env_value POSTGRES_PASSWORD)" 8 \
    || is_placeholder "$(read_env_value Jwt__Secret)" 32 \
    || is_placeholder "$(read_env_value SuperAdmin__Password)" 12
}

if [[ ! -f "$EXAMPLE_FILE" ]]; then
  echo "Missing $EXAMPLE_FILE"
  exit 1
fi

if [[ -f "$ENV_FILE" ]]; then
  if [[ "$FORCE" == true ]]; then
    generate_passwords
    apply_secrets_to_env
    print_summary "Updated"
    exit 0
  fi

  if needs_secret_update; then
    echo "Found placeholder or invalid secrets in $ENV_FILE — regenerating passwords..."
    generate_passwords
    apply_secrets_to_env
    print_summary "Updated"
    exit 0
  fi

  echo ".env already exists at $ENV_FILE with custom secrets — skipping."
  echo "Run with --force to regenerate all passwords."
  exit 0
fi

generate_passwords
cp "$EXAMPLE_FILE" "$ENV_FILE"
apply_secrets_to_env
print_summary "Created"
