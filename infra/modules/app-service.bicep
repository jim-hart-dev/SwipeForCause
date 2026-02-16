// =============================================================================
// Module: App Service — .NET 8 API (Free tier for dev/prototype)
// =============================================================================

@description('Deployment environment.')
param environment string

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('Storage account name for connection string.')
param storageAccountName string

@description('Storage account key.')
@secure()
param storageAccountKey string

@description('PostgreSQL FQDN.')
param postgresHost string

@description('PostgreSQL admin username.')
param postgresAdminUser string

@description('PostgreSQL admin password.')
@secure()
param postgresAdminPassword string

@description('PostgreSQL database name.')
param postgresDbName string

@description('Clerk secret key.')
@secure()
param clerkSecretKey string

@description('SendGrid API key.')
@secure()
param sendGridApiKey string

// ---------------------------------------------------------------------------
// Variables
// ---------------------------------------------------------------------------

var appServicePlanName = 'plan-sfc-${environment}'
var apiAppName = 'app-sfc-api-${environment}'

// ---------------------------------------------------------------------------
// App Service Plan (F1 Free — upgrade to B1 when needed)
// ---------------------------------------------------------------------------

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// ---------------------------------------------------------------------------
// App Service — .NET 8 API
// ---------------------------------------------------------------------------

resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: apiAppName
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: environment == 'prod' ? 'Production' : 'Development' }
        { name: 'Clerk__SecretKey', value: clerkSecretKey }
        { name: 'SendGrid__ApiKey', value: sendGridApiKey }
        { name: 'Storage__ConnectionString', value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storageAccountKey};EndpointSuffix=core.windows.net' }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: 'Host=${postgresHost};Port=5432;Database=${postgresDbName};Username=${postgresAdminUser};Password=${postgresAdminPassword};SSL Mode=Require;Trust Server Certificate=true'
          type: 'Custom'
        }
      ]
    }
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------

output apiAppName string = apiApp.name
output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output apiPrincipalId string = apiApp.identity.principalId
