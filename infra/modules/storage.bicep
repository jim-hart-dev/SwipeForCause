// =============================================================================
// Module: Azure Blob Storage — media storage with private containers
// =============================================================================

@description('Deployment environment.')
param environment string

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

// ---------------------------------------------------------------------------
// Variables
// ---------------------------------------------------------------------------

var storageAccountName = 'stsfc${environment}'
var blobContainers = ['uploads', 'videos', 'images', 'avatars', 'logos']

// ---------------------------------------------------------------------------
// Storage Account
// ---------------------------------------------------------------------------

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
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

// Private containers
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
// Outputs
// ---------------------------------------------------------------------------

output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output storageAccountKey string = storageAccount.listKeys().keys[0].value
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
