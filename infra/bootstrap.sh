#!/usr/bin/env bash
set -euo pipefail

APP_NAME="${APP_NAME:-policydesk-api-rodrigo}"

RESOURCE_GROUP="${RESOURCE_GROUP:-rg-policydesk}"
LOCATION="${LOCATION:-westus2}"
GITHUB_REPO="${GITHUB_REPO:-Rodrigosdk/PolicyDesk}"
GITHUB_ENVIRONMENT="${GITHUB_ENVIRONMENT:-production}"
for cli in az gh; do
  command -v "$cli" >/dev/null || { echo "Instale o $cli antes de continuar." >&2; exit 1; }
done
az account show --output none 2>/dev/null || { echo "Rode 'az login' antes de continuar." >&2; exit 1; }
if [[ "$(az account show --query name --output tsv)" == "N/A(tenant level account)" ]]; then
  echo "A conta logada não tem assinatura da Azure. Crie uma em https://azure.microsoft.com/free e rode 'az login' de novo." >&2
  exit 1
fi
gh auth status >/dev/null 2>&1 || { echo "Rode 'gh auth login' antes de continuar." >&2; exit 1; }

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "==> Grupo de recursos $RESOURCE_GROUP ($LOCATION)"
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

DB_ADMIN_LOGIN="policydeskadmin"
DB_NAME="policydesk"
DB_ADMIN_PASSWORD="${DB_ADMIN_PASSWORD:-$(openssl rand -base64 32 | tr -dc 'A-Za-z0-9' | head -c 24)Aa1}"

echo "==> PostgreSQL Flexible Server (B1ms), plano F1 e Web App $APP_NAME (pode levar ~10 min)"
DB_HOST="$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file "$SCRIPT_DIR/main.bicep" \
  --parameters appName="$APP_NAME" databaseAdminLogin="$DB_ADMIN_LOGIN" databaseAdminPassword="$DB_ADMIN_PASSWORD" databaseName="$DB_NAME" \
  --query properties.outputs.databaseHost.value \
  --output tsv)"
DATABASE_CONNECTION_STRING="Host=$DB_HOST;Port=5432;Database=$DB_NAME;Username=$DB_ADMIN_LOGIN;Password=$DB_ADMIN_PASSWORD;SSL Mode=Require"

WEBAPP_ID="$(az webapp show --resource-group "$RESOURCE_GROUP" --name "$APP_NAME" --query id --output tsv)"
SUBSCRIPTION_ID="$(az account show --query id --output tsv)"
TENANT_ID="$(az account show --query tenantId --output tsv)"

echo "==> Identidade do GitHub Actions (OIDC, sem segredo de longa duração)"
IDENTITY_NAME="github-$APP_NAME"
CLIENT_ID="$(az ad app list --display-name "$IDENTITY_NAME" --query '[0].appId' --output tsv)"
if [[ -z "$CLIENT_ID" ]]; then
  CLIENT_ID="$(az ad app create --display-name "$IDENTITY_NAME" --query appId --output tsv)"
fi
az ad sp show --id "$CLIENT_ID" --output none 2>/dev/null || az ad sp create --id "$CLIENT_ID" --output none

CREDENTIAL_NAME="github-$GITHUB_ENVIRONMENT"
if [[ -z "$(az ad app federated-credential list --id "$CLIENT_ID" --query "[?name=='$CREDENTIAL_NAME'].name" --output tsv)" ]]; then
  az ad app federated-credential create --id "$CLIENT_ID" --parameters "{
    \"name\": \"$CREDENTIAL_NAME\",
    \"issuer\": \"https://token.actions.githubusercontent.com\",
    \"subject\": \"repo:$GITHUB_REPO:environment:$GITHUB_ENVIRONMENT\",
    \"audiences\": [\"api://AzureADTokenExchange\"]
  }" --output none
fi

echo "==> Permissão mínima: Website Contributor apenas no Web App"
az role assignment create \
  --assignee "$CLIENT_ID" \
  --role "Website Contributor" \
  --scope "$WEBAPP_ID" \
  --output none 2>/dev/null || true

echo "==> Environment $GITHUB_ENVIRONMENT no GitHub (deploy só a partir da main)"
gh api -X PUT "repos/$GITHUB_REPO/environments/$GITHUB_ENVIRONMENT" --silent --input - <<< \
  '{"deployment_branch_policy":{"protected_branches":false,"custom_branch_policies":true}}'
if [[ -z "$(gh api "repos/$GITHUB_REPO/environments/$GITHUB_ENVIRONMENT/deployment-branch-policies" --jq '.branch_policies[] | select(.name=="main") | .name')" ]]; then
  gh api -X POST "repos/$GITHUB_REPO/environments/$GITHUB_ENVIRONMENT/deployment-branch-policies" \
    -f name=main -f type=branch --silent
fi

echo "==> Secrets e variável do environment"
gh secret set AZURE_CLIENT_ID --repo "$GITHUB_REPO" --env "$GITHUB_ENVIRONMENT" --body "$CLIENT_ID"
gh secret set AZURE_TENANT_ID --repo "$GITHUB_REPO" --env "$GITHUB_ENVIRONMENT" --body "$TENANT_ID"
gh secret set AZURE_SUBSCRIPTION_ID --repo "$GITHUB_REPO" --env "$GITHUB_ENVIRONMENT" --body "$SUBSCRIPTION_ID"
gh secret set DATABASE_CONNECTION_STRING --repo "$GITHUB_REPO" --env "$GITHUB_ENVIRONMENT" --body "$DATABASE_CONNECTION_STRING"
gh variable set AZURE_WEBAPP_NAME --repo "$GITHUB_REPO" --env "$GITHUB_ENVIRONMENT" --body "$APP_NAME"

echo
echo "Pronto. Faça merge em main para o primeiro deploy: https://$APP_NAME.azurewebsites.net"
