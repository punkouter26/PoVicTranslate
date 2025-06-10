@description('The location used for all deployed resources')
param location string = resourceGroup().location

@description('Tags that will be applied to all resources')
param tags object = {}

@description('Resource token for unique naming')
param resourceToken string

var abbrs = loadJsonContent('./abbreviations.json')

// Monitor application with Azure Monitor
module monitoring 'br/public:avm/ptn/azd/monitoring:0.1.0' = {
  name: 'monitoring'
  params: {
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
    applicationInsightsDashboardName: '${abbrs.portalDashboards}${resourceToken}'
    location: location
    tags: tags
  }
}

// User Assigned Managed Identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${abbrs.managedIdentityUserAssignedIdentities}${resourceToken}'
  location: location
  tags: tags
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'plan-${resourceToken}'
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true // Linux
  }
  tags: tags
}

// App Service (Web App)
resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-${resourceToken}'
  location: location
  kind: 'app,linux'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNET|9.0'
      cors: {
        allowedOrigins: ['*']
        supportCredentials: false
      }
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: monitoring.outputs.applicationInsightsConnectionString
        }
        {
          name: 'ApiSettings__AzureOpenAIApiKey'
          value: '4b214928e4ff4956b5f672aea805770b'
        }
        {
          name: 'ApiSettings__AzureOpenAIEndpoint'
          value: 'https://eastus.api.cognitive.microsoft.com/'
        }
        {
          name: 'ApiSettings__AzureOpenAIDeploymentName'
          value: 'victranslator'
        }
        {
          name: 'ApiSettings__AzureSpeechSubscriptionKey'
          value: '5811b9bab6924027aafc6129d6b65c0c'
        }
        {
          name: 'ApiSettings__AzureSpeechRegion'
          value: 'eastus'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
    }
    httpsOnly: true
  }
  tags: union(tags, { 'azd-service-name': 'victoriantranslator-server' })
}

output AZURE_RESOURCE_VICTORIANTRANSLATOR_SERVER_ID string = appService.id
