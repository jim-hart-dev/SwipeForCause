// =============================================================================
// SwipeForCause MVP — Azure Infrastructure
// =============================================================================
// Provisions: App Service (.NET API), Static Web App (React), PostgreSQL,
//             Storage + CDN, Function App (media processing)
// =============================================================================

// ---------------------------------------------------------------------------
// Parameters
// ---------------------------------------------------------------------------

@allowed(['dev', 'staging', 'prod'])
@description('Deployment environment.')
param environment string

@description('Azure region for all resources.')
param location string = 'eastus'

@secure()
@description('Administrator password for the PostgreSQL server.')
param postgresAdminPassword string

@secure()
@description('Clerk secret key for authentication.')
param clerkSecretKey string

@secure()
@description('SendGrid API key for transactional email.')
param sendGridApiKey string

// ---------------------------------------------------------------------------
// Variables
// ---------------------------------------------------------------------------

var prefix = 'sfc'
// Storage and CDN names must be globally unique and alphanumeric
var storageAccountName = '${prefix}storage${environment}'
var cdnProfileName = '${prefix}-cdn-${environment}'
var cdnEndpointName = '${prefix}-cdn-ep-${environment}'
var postgresServerName = '${prefix}-pg-${environment}'
var appServicePlanName = '${prefix}-plan-${environment}'
var apiAppName = '${prefix}-api-${environment}'
var staticWebAppName = '${prefix}-web-${environment}'
var functionAppName = '${prefix}-func-${environment}'
var functionPlanName = '${prefix}-funcplan-${environment}'
var functionStorageName = '${prefix}funcstor${environment}'
var postgresAdminUser = 'sfcadmin'
var postgresDbName = 'swipeforcause'

// ---------------------------------------------------------------------------
// App Service Plan (B1) — hosts the .NET API
// ---------------------------------------------------------------------------

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true // required for Linux
  }
}

// ---------------------------------------------------------------------------
// App Service — .NET API
// ---------------------------------------------------------------------------

resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: apiAppName
  location: location
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
      alwaysOn: true
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: environment == 'prod' ? 'Production' : 'Development' }
        { name: 'Clerk__SecretKey', value: clerkSecretKey }
        { name: 'SendGrid__ApiKey', value: sendGridApiKey }
        { name: 'Storage__ConnectionString', value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net' }
        { name: 'Storage__CdnBaseUrl', value: 'https://${cdnEndpoint.properties.hostName}' }
        { name: 'FunctionApp__BaseUrl', value: 'https://${functionApp.properties.defaultHostName}' }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: 'Host=${postgresServer.properties.fullyQualifiedDomainName};Port=5432;Database=${postgresDbName};Username=${postgresAdminUser};Password=${postgresAdminPassword};SSL Mode=Require;Trust Server Certificate=true'
          type: 'Custom'
        }
      ]
    }
  }
}

// ---------------------------------------------------------------------------
// Static Web App — React frontend
// ---------------------------------------------------------------------------

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: staticWebAppName
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    buildProperties: {
      appLocation: '/landing'
      outputLocation: 'dist'
    }
  }
}

// ---------------------------------------------------------------------------
// Azure Database for PostgreSQL Flexible Server
// ---------------------------------------------------------------------------

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: postgresServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: postgresAdminUser
    administratorLoginPassword: postgresAdminPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
  }
}

// Allow Azure services to connect (start IP 0.0.0.0 = Azure-internal)
resource postgresFirewallAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Require SSL for all connections
resource postgresSSLConfig 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2023-12-01-preview' = {
  parent: postgresServer
  name: 'require_secure_transport'
  properties: {
    value: 'on'
    source: 'user-override'
  }
}

// Default database
resource postgresDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgresServer
  name: postgresDbName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// ---------------------------------------------------------------------------
// Storage Account — blob media storage
// ---------------------------------------------------------------------------

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
  }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

// Private containers — no anonymous access
var blobContainers = ['uploads', 'videos', 'images', 'avatars', 'logos']

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [
  for name in blobContainers: {
    parent: blobServices
    name: name
    properties: {
      publicAccess: 'None'
    }
  }
]

// ---------------------------------------------------------------------------
// CDN — serves processed media (videos, images, avatars, logos — NOT uploads)
// ---------------------------------------------------------------------------

resource cdnProfile 'Microsoft.Cdn/profiles@2024-02-01' = {
  name: cdnProfileName
  location: 'global'
  sku: {
    name: 'Standard_Microsoft'
  }
}

resource cdnEndpoint 'Microsoft.Cdn/profiles/endpoints@2024-02-01' = {
  parent: cdnProfile
  name: cdnEndpointName
  location: 'global'
  properties: {
    isHttpAllowed: false // HTTPS only
    isHttpsAllowed: true
    originHostHeader: '${storageAccountName}.blob.core.windows.net'
    origins: [
      {
        name: 'blob-origin'
        properties: {
          hostName: '${storageAccountName}.blob.core.windows.net'
          httpsPort: 443
          originHostHeader: '${storageAccountName}.blob.core.windows.net'
        }
      }
    ]
    // Block the /uploads container from being served through CDN
    deliveryPolicy: {
      rules: [
        {
          name: 'BlockUploadsContainer'
          order: 1
          conditions: [
            {
              name: 'UrlPath'
              parameters: {
                typeName: 'DeliveryRuleUrlPathMatchConditionParameters'
                operator: 'BeginsWith'
                matchValues: ['/uploads/']
                negateCondition: false
                transforms: ['Lowercase']
              }
            }
          ]
          actions: [
            {
              name: 'UrlRewrite'
              parameters: {
                typeName: 'DeliveryRuleUrlRewriteActionParameters'
                sourcePattern: '/uploads/'
                destination: '/blocked'
                preserveUnmatchedPath: false
              }
            }
          ]
        }
      ]
    }
  }
}

// ---------------------------------------------------------------------------
// Function App — media processing (Consumption plan)
// ---------------------------------------------------------------------------

// Separate storage account for Function App runtime
resource functionStorage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: functionStorageName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

resource functionPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: functionPlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionapp'
  properties: {
    reserved: true // Linux
  }
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionPlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: [
        { name: 'AzureWebJobsStorage', value: 'DefaultEndpointsProtocol=https;AccountName=${functionStorageName};AccountKey=${functionStorage.listKeys().keys[0].value};EndpointSuffix=core.windows.net' }
        { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~4' }
        { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet-isolated' }
        { name: 'MediaStorage__ConnectionString', value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net' }
      ]
    }
  }
}

// ---------------------------------------------------------------------------
// Role assignment — give API managed identity "Storage Blob Data Contributor"
// on the media storage account
// ---------------------------------------------------------------------------

var storageBlobDataContributorRole = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

resource apiStorageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, apiApp.id, storageBlobDataContributorRole)
  scope: storageAccount
  properties: {
    principalId: apiApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRole)
  }
}

// Same for Function App
resource funcStorageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, functionApp.id, storageBlobDataContributorRole)
  scope: storageAccount
  properties: {
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRole)
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------

output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'
output cdnEndpointUrl string = 'https://${cdnEndpoint.properties.hostName}'
output storageAccountName string = storageAccount.name
output postgresHost string = postgresServer.properties.fullyQualifiedDomainName
output functionAppUrl string = 'https://${functionApp.properties.defaultHostName}'
output apiManagedIdentityPrincipalId string = apiApp.identity.principalId
output functionManagedIdentityPrincipalId string = functionApp.identity.principalId
