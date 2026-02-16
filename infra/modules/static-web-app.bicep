// =============================================================================
// Module: Static Web App — React frontend
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

var staticWebAppName = 'stapp-sfc-${environment}'

// ---------------------------------------------------------------------------
// Static Web App
// ---------------------------------------------------------------------------

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: staticWebAppName
  location: location
  tags: tags
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    buildProperties: {
      appLocation: '/src/web'
      outputLocation: 'dist'
    }
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------

output staticWebAppName string = staticWebApp.name
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'
