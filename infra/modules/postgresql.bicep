// =============================================================================
// Module: Azure Database for PostgreSQL Flexible Server
// =============================================================================

@description('Deployment environment.')
param environment string

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('Administrator login username.')
param adminUser string = 'sfcadmin'

@description('Administrator login password.')
@secure()
param adminPassword string

@description('Name of the application database.')
param databaseName string = 'swipeforcause'

// ---------------------------------------------------------------------------
// Variables
// ---------------------------------------------------------------------------

var serverName = 'db-sfc-${environment}'

// ---------------------------------------------------------------------------
// PostgreSQL Flexible Server
// ---------------------------------------------------------------------------

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: serverName
  location: location
  tags: tags
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: adminUser
    administratorLoginPassword: adminPassword
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

// Allow Azure services to connect
resource firewallAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Enforce SSL for all connections
resource sslConfig 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2023-12-01-preview' = {
  parent: postgresServer
  name: 'require_secure_transport'
  properties: {
    value: 'on'
    source: 'user-override'
  }
}

// Application database
resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgresServer
  name: databaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------

output serverName string = postgresServer.name
output host string = postgresServer.properties.fullyQualifiedDomainName
output databaseName string = databaseName
output adminUser string = adminUser
