// PoVicTranslate - Main Bicep Infrastructure File
targetScope = 'subscription'

param environmentName string = 'PoVicTranslate'
param location string = 'eastus2'

var resourceGroupName = 'PoVicTranslate'
var tags = {
  'azd-env-name': environmentName
  'app-name': 'PoVicTranslate'
}

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

module resources './resources.bicep' = {
  name: 'resources-deployment'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    tags: tags
  }
}

output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_TENANT_ID string = tenant().tenantId
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.APPLICATIONINSIGHTS_CONNECTION_STRING
output AZURE_APPSERVICE_NAME string = resources.outputs.AZURE_APPSERVICE_NAME
output AZURE_APPSERVICE_URI string = resources.outputs.AZURE_APPSERVICE_URI
output AZURE_STORAGE_ACCOUNT_NAME string = resources.outputs.AZURE_STORAGE_ACCOUNT_NAME
