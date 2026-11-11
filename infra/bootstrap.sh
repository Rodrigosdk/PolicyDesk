#!/usr/bin/env bash
set -euo pipefail

: "${APP_NAME:?Defina APP_NAME (nome único do Web App, ex.: policydesk-api-rodrigo)}"
: "${DATABASE_CONNECTION_STRING:?Defina DATABASE_CONNECTION_STRING}"

RESOURCE_GROUP="${RESOURCE_GROUP:-rg-policydesk}"
LOCATION="${LOCATION:-brazilsouth}"
GITHUB_REPO="${GITHUB_REPO:-Rodrigosdk/PolicyDesk}"
GITHUB_ENVIRONMENT="${GITHUB_ENVIRONMENT:-production}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "==> Grupo de recursos $RESOURCE_GROUP ($LOCATION)"
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

echo "==> Plano F1 + Web App $APP_NAME"
az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file "$SCRIPT_DIR/main.bicep" \
  --parameters appName="$APP_NAME" databaseConnectionString="$DATABASE_CONNECTION_STRING" \
  --output none

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

cat <<OUT

Pronto. Configure no GitHub (Settings > Environments > $GITHUB_ENVIRONMENT):

  Secrets:
    AZURE_CLIENT_ID             = $CLIENT_ID
    AZURE_TENANT_ID             = $TENANT_ID
    AZURE_SUBSCRIPTION_ID       = $SUBSCRIPTION_ID
    DATABASE_CONNECTION_STRING  = (a mesma connection string usada acima)

  Variables:
    AZURE_WEBAPP_NAME           = $APP_NAME

Ou, com o GitHub CLI:

  gh api -X PUT repos/$GITHUB_REPO/environments/$GITHUB_ENVIRONMENT
  gh secret set AZURE_CLIENT_ID --env $GITHUB_ENVIRONMENT --body "$CLIENT_ID"
  gh secret set AZURE_TENANT_ID --env $GITHUB_ENVIRONMENT --body "$TENANT_ID"
  gh secret set AZURE_SUBSCRIPTION_ID --env $GITHUB_ENVIRONMENT --body "$SUBSCRIPTION_ID"
  gh secret set DATABASE_CONNECTION_STRING --env $GITHUB_ENVIRONMENT
  gh variable set AZURE_WEBAPP_NAME --env $GITHUB_ENVIRONMENT --body "$APP_NAME"

URL: https://$APP_NAME.azurewebsites.net
OUT
