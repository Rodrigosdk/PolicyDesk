targetScope = 'resourceGroup'

@description('Nome do Web App; vira o subdomínio <nome>.azurewebsites.net e precisa ser único na Azure.')
@minLength(2)
@maxLength(58)
param appName string

@description('Região dos recursos.')
param location string = resourceGroup().location

@description('Usuário administrador do PostgreSQL.')
param databaseAdminLogin string = 'policydeskadmin'

@description('Senha do administrador do PostgreSQL.')
@secure()
param databaseAdminPassword string

@description('Nome do banco usado pela API.')
param databaseName string = 'policydesk'

@description('Imagem da API. O pipeline de CD troca a tag pela do commit publicado.')
param containerImage string = 'ghcr.io/rodrigosdk/policydesk-api:latest'

@description('Endpoint OTLP opcional para exportar traces, métricas e logs.')
param otlpEndpoint string = ''

var otlpSettings = empty(otlpEndpoint) ? [] : [
  {
    name: 'OTEL_EXPORTER_OTLP_ENDPOINT'
    value: otlpEndpoint
  }
]

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: 'psql-${appName}'
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '17'
    administratorLogin: databaseAdminLogin
    administratorLoginPassword: databaseAdminPassword
    storage: {
      storageSizeGB: 32
      autoGrow: 'Disabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
    authConfig: {
      passwordAuth: 'Enabled'
      activeDirectoryAuth: 'Disabled'
    }
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: postgres
  name: databaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: postgres
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

var databaseConnectionString = 'Host=${postgres.properties.fullyQualifiedDomainName};Port=5432;Database=${database.name};Username=${databaseAdminLogin};Password=${databaseAdminPassword};SSL Mode=Require'

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: 'plan-${appName}'
  location: location
  kind: 'linux'
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  properties: {
    reserved: true
  }
}

resource app 'Microsoft.Web/sites@2024-04-01' = {
  name: appName
  location: location
  kind: 'app,linux,container'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: 'DOCKER|${containerImage}'
      alwaysOn: false
      http20Enabled: true
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: concat([
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'Swagger__Enabled'
          value: 'true'
        }
        {
          name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
          value: 'true'
        }
        {
          name: 'WEBSITES_PORT'
          value: '8080'
        }
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://ghcr.io'
        }
      ], otlpSettings)
      connectionStrings: [
        {
          name: 'DbConnection'
          connectionString: databaseConnectionString
          type: 'Custom'
        }
      ]
    }
  }
}

resource ftpBasicAuth 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
  parent: app
  name: 'ftp'
  properties: {
    allow: false
  }
}

output webAppName string = app.name
output databaseHost string = postgres.properties.fullyQualifiedDomainName
output webAppUrl string = 'https://${app.properties.defaultHostName}'
