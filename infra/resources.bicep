// PoVicTranslate Resources Module
targetScope = 'resourceGroup'

@description('Location for all resources')
param location string

@description('Environment name')
param environmentName string

@description('Resource tags')
param tags object

@description('Azure OpenAI API Key')
@secure()
param azureOpenAIApiKey string

@description('Azure OpenAI Endpoint')
param azureOpenAIEndpoint string

@description('Azure OpenAI Deployment Name')
param azureOpenAIDeploymentName string

@description('Azure Speech Subscription Key')
@secure()
param azureSpeechSubscriptionKey string

@description('Azure Speech Region')
param azureSpeechRegion string

// Hard-coded App Service Plan reference (F1 in PoShared resource group)
var existingPlanName = 'PoShared3'
var existingPlanResourceGroup = 'PoShared'

// App Service name derived from environment
var appServiceName = 'PoVicTranslate'

// Storage account (cloud-only, not used locally)
var storageAccountName = toLower(replace('${environmentName}storage', '-', ''))

// Log Analytics
var logAnalyticsName = '${environmentName}-logs'

// Application Insights
var appInsightsName = '${environmentName}-insights'

// Reference existing App Service Plan
resource existingPlan 'Microsoft.Web/serverfarms@2023-12-01' existing = {
  name: existingPlanName
  scope: resourceGroup(existingPlanResourceGroup)
}

// Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// Storage Account (for cloud deployment, not Azurite)
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

// Table Service
resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

// App Service (using existing F1 plan)
resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: appServiceName
  location: location
  tags: tags
  properties: {
    serverFarmId: existingPlan.id
    httpsOnly: true
    siteConfig: {
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      netFrameworkVersion: 'v9.0'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'AZURE_STORAGE_CONNECTION_STRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'ApiSettings__AzureOpenAIApiKey'
          value: azureOpenAIApiKey
        }
        {
          name: 'ApiSettings__AzureOpenAIEndpoint'
          value: azureOpenAIEndpoint
        }
        {
          name: 'ApiSettings__AzureOpenAIDeploymentName'
          value: azureOpenAIDeploymentName
        }
        {
          name: 'ApiSettings__AzureSpeechSubscriptionKey'
          value: azureSpeechSubscriptionKey
        }
        {
          name: 'ApiSettings__AzureSpeechRegion'
          value: azureSpeechRegion
        }
      ]
    }
  }
}

// Outputs
output APPLICATIONINSIGHTS_CONNECTION_STRING string = appInsights.properties.ConnectionString
output AZURE_APPSERVICE_NAME string = appService.name
output AZURE_APPSERVICE_URI string = 'https://${appService.properties.defaultHostName}'
output AZURE_STORAGE_ACCOUNT_NAME string = storageAccount.name
output AZURE_LOG_ANALYTICS_WORKSPACE_ID string = logAnalytics.id
