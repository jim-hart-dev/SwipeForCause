// =============================================================================
// SwipeForCause MVP — Azure Infrastructure (Orchestration)
// =============================================================================
// Provisions all Azure resources via modular Bicep templates:
//   - App Service (.NET 8 API) with staging slot
//   - Static Web App (React frontend)
//   - PostgreSQL Flexible Server (v16, B1ms)
//   - Blob Storage with media containers
//   - CDN (Standard Microsoft, HTTPS only)
//   - Azure Functions (Consumption, media processing)
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
// Module: CDN
// ---------------------------------------------------------------------------

module cdn 'modules/cdn.bicep' = {
  name: 'cdn-${environment}'
  params: {
    environment: environment
    tags: tags
    storageAccountName: storage.outputs.storageAccountName
  }
}

// ---------------------------------------------------------------------------
// Module: Functions
// ---------------------------------------------------------------------------

module functions 'modules/functions.bicep' = {
  name: 'functions-${environment}'
  params: {
    environment: environment
    location: location
    tags: tags
    mediaStorageAccountName: storage.outputs.storageAccountName
    mediaStorageAccountKey: storage.outputs.storageAccountKey
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
    cdnEndpointHostName: cdn.outputs.cdnEndpointHostName
    functionAppHostName: functions.outputs.functionAppHostName
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
// Role Assignments — Storage Blob Data Contributor
// ---------------------------------------------------------------------------

var storageBlobDataContributorRole = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

// Use the known storage account name directly for the existing reference
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

resource funcStorageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccountName, 'func', storageBlobDataContributorRole)
  scope: storageAccountRef
  properties: {
    principalId: functions.outputs.functionAppPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRole)
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------

output apiUrl string = appService.outputs.apiUrl
output apiStagingUrl string = appService.outputs.stagingUrl
output staticWebAppUrl string = staticWebApp.outputs.staticWebAppUrl
output cdnEndpointUrl string = cdn.outputs.cdnEndpointUrl
output storageAccountName string = storage.outputs.storageAccountName
output postgresHost string = postgresql.outputs.host
output functionAppUrl string = functions.outputs.functionAppUrl
