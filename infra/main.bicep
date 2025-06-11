// Main Bicep template for Victorian Translator application
// Follows Azure deployment best practices with minimal resource configuration

targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

// Optional parameters
@description('Name of the existing Application Insights in PoShared resource group')
param sharedApplicationInsightsName string = 'app-insights-poshared'

// Variables
var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = { 'azd-env-name': environmentName }

// Reference to existing shared resources in PoShared resource group
resource sharedRg 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
  name: 'PoShared'
}

resource sharedApplicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: sharedApplicationInsightsName
  scope: sharedRg
}

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${abbrs.resourcesResourceGroups}${environmentName}'
  location: location
  tags: tags
}

// The application frontend
module web './app/web.bicep' = {
  name: 'web'
  scope: rg
  params: {
    name: '${abbrs.webSitesAppService}web-${resourceToken}'
    location: location
    tags: tags
    applicationInsightsConnectionString: sharedApplicationInsights.properties.ConnectionString
    appServicePlanId: appServicePlan.outputs.id
  }
}

// Create an App Service Plan to group applications under the same payment plan and SKU
module appServicePlan './core/host/appserviceplan.bicep' = {
  name: 'appserviceplan'
  scope: rg
  params: {
    name: '${abbrs.webServerFarms}${resourceToken}'
    location: location
    tags: tags
    sku: {
      name: 'B1'
      capacity: 1
    }
    kind: 'linux'
  }
}

// App outputs
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_RESOURCE_GROUP string = rg.name

// Service outputs
output SERVICE_WEB_IDENTITY_PRINCIPAL_ID string = web.outputs.identityPrincipalId
output SERVICE_WEB_NAME string = web.outputs.name
output SERVICE_WEB_URI string = web.outputs.uri
