// =============================================================================
// SwipeForCause MVP — Azure Infrastructure (Orchestration)
// =============================================================================
// Provisions Azure resources via modular Bicep templates:
//   - App Service (.NET 8 API, Free tier)
//   - Static Web App (React frontend)
//   - PostgreSQL Flexible Server (v16, B1ms)
//   - Blob Storage with media containers
// =============================================================================

targetScope = 'resourceGroup'

// ---------------------------------------------------------------------------
// Parameters
// ---------------------------------------------------------------------------

@allowed(['dev', 'staging', 'prod'])
@description('Deployment environment.')
param environment string = 'prod'

@description('Azure region for all resources.')
param location string = 'eastus'

@secure()
@description('Administrator password for the PostgreSQL server.')
param postgresAdminPassword string

@secure()
@description('Clerk secret key for authentication.')
param clerkSecretKey string = ''

@secure()
@description('SendGrid API key for transactional email.')
param sendGridApiKey string = ''

// ---------------------------------------------------------------------------
// Variables
// ---------------------------------------------------------------------------

var tags = {
  project: 'swipeforcause'
  environment: environment
}
var postgresAdminUser = 'sfcadmin'
var postgresDbName = 'swipeforcause'
var prefix = 'sfc'
var storageAccountName = '${prefix}storage${environment}'

// ---------------------------------------------------------------------------
// Module: PostgreSQL
// ---------------------------------------------------------------------------

module postgresql 'modules/postgresql.bicep' = {
  name: 'postgresql-${environment}'
  params: {
    environment: environment
    location: location
    tags: tags
    adminUser: postgresAdminUser
    adminPassword: postgresAdminPassword
    databaseName: postgresDbName
  }
}

// ---------------------------------------------------------------------------
// Module: Storage
// ---------------------------------------------------------------------------

module storage 'modules/storage.bicep' = {
  name: 'storage-${environment}'
  params: {
    environment: environment
    location: location
    tags: tags
  }
}

// ---------------------------------------------------------------------------
// Module: App Service
// ---------------------------------------------------------------------------

module appService 'modules/app-service.bicep' = {
  name: 'app-service-${environment}'
  params: {
    environment: environment
    location: location
    tags: tags
    storageAccountName: storage.outputs.storageAccountName
    storageAccountKey: storage.outputs.storageAccountKey
    postgresHost: postgresql.outputs.host
    postgresAdminUser: postgresAdminUser
    postgresAdminPassword: postgresAdminPassword
    postgresDbName: postgresDbName
    clerkSecretKey: clerkSecretKey
    sendGridApiKey: sendGridApiKey
  }
}

// ---------------------------------------------------------------------------
// Module: Static Web App
// ---------------------------------------------------------------------------

module staticWebApp 'modules/static-web-app.bicep' = {
  name: 'static-web-app-${environment}'
  params: {
    environment: environment
    location: location
    tags: tags
  }
}

// ---------------------------------------------------------------------------
// Role Assignment — Storage Blob Data Contributor for API
// ---------------------------------------------------------------------------

var storageBlobDataContributorRole = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

resource storageAccountRef 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource apiStorageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccountName, 'api', storageBlobDataContributorRole)
  scope: storageAccountRef
  properties: {
    principalId: appService.outputs.apiPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRole)
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------

output apiUrl string = appService.outputs.apiUrl
output staticWebAppUrl string = staticWebApp.outputs.staticWebAppUrl
output storageAccountName string = storage.outputs.storageAccountName
output postgresHost string = postgresql.outputs.host
