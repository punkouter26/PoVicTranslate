// PoVicTranslate - Main Bicep Infrastructure File
// Uses Container Apps and references shared resources from PoShared
targetScope = 'subscription'

@description('Environment name for azd')
param environmentName string = 'PoVicTranslate'

@description('Primary location for resources')
param location string = 'eastus'

// Resource group names
var resourceGroupName = 'PoVicTranslate'
var sharedResourceGroupName = 'PoShared'

var tags = {
  'azd-env-name': environmentName
  'app-name': 'PoVicTranslate'
}

// Create application resource group (or use existing)
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Deploy resources
module resources './resources.bicep' = {
  name: 'resources-deployment'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    tags: tags
    sharedResourceGroupName: sharedResourceGroupName
  }
}

output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_CONTAINER_APP_NAME string = resources.outputs.containerAppName
output AZURE_CONTAINER_APP_FQDN string = resources.outputs.containerAppFqdn
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.appInsightsConnectionString
