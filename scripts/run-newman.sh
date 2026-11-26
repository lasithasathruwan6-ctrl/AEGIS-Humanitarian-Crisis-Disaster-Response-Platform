#!/usr/bin/env bash
# scripts/run-newman.sh
#
# Convenience wrapper to run the full Postman/Newman suite.
#
# Usage:
#   bash scripts/run-newman.sh                           # localhost:8080
#   BASE_URL=https://staging.example.com bash scripts/run-newman.sh
#   FOLDER="Auth" bash scripts/run-newman.sh             # single folder

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
COLLECTION="$REPO_ROOT/docs/postman/crisis-aid-platform.postman_collection.json"
ENVIRONMENT="$REPO_ROOT/docs/postman/crisis-aid-platform.postman_environment.json"
BASE_URL="${BASE_URL:-http://localhost:8080}"
FOLDER="${FOLDER:-}"

if ! command -v newman &>/dev/null; then
  echo "Newman not found. Install it with: npm install -g newman"
  exit 1
fi

ARGS=(
  run "$COLLECTION"
  -e "$ENVIRONMENT"
  --env-var "baseUrl=$BASE_URL"
  --color on
)

if [[ -n "$FOLDER" ]]; then
  ARGS+=(--folder "$FOLDER")
fi

# Optional HTML report when htmlextra reporter is available.
if newman --reporters | grep -q htmlextra 2>/dev/null; then
  ARGS+=(
    --reporters cli,htmlextra
    --reporter-htmlextra-export newman-report.html
  )
  echo "HTML report will be written to newman-report.html"
fi

echo "Running Newman against $BASE_URL ..."
newman "${ARGS[@]}"
