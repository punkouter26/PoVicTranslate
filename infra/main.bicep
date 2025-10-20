targetScope = 'resourceGroup'

@minLength(1)
@maxLength(64)
@description('Name of the environment used for resource naming')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string = resourceGroup().location

// Reference to the existing shared App Service Plan in PoShared resource group
// Using PoShared2 plan (centralus location)
resource existingAppServicePlan 'Microsoft.Web/serverfarms@2023-12-01' existing = {
  name: 'PoShared2'
  scope: resourceGroup('PoShared')
}

// Create Log Analytics Workspace for Application Insights
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'PoVicTranslate'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// Create Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'PoVicTranslate'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    RetentionInDays: 30
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: 'PoVicTranslate'
  location: 'centralus'  // Must match the PoShared2 plan location
  kind: 'app'
  properties: {
    serverFarmId: existingAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      use32BitWorkerProcess: true  // Required for F1 tier
      alwaysOn: false  // Not supported in F1 tier
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        // These values will be overridden by GitHub Actions or manually configured
        {
          name: 'ApiSettings__AzureOpenAIApiKey'
          value: ''
        }
        {
          name: 'ApiSettings__AzureOpenAIEndpoint'
          value: ''
        }
        {
          name: 'ApiSettings__AzureOpenAIDeploymentName'
          value: ''
        }
        {
          name: 'ApiSettings__AzureSpeechSubscriptionKey'
          value: ''
        }
        {
          name: 'ApiSettings__AzureSpeechRegion'
          value: ''
        }
      ]
    }
  }
  tags: {
    'azd-env-name': environmentName
    'azd-service-name': 'victorianupdater-server'
  }
}

//  App Service diagnostic settings
resource appServiceDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: appService
  name: 'app-service-diagnostics'
  properties: {
    workspaceId: logAnalyticsWorkspace.id
    logs: [
      {
        category: 'AppServiceHTTPLogs'
        enabled: true
      }
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
      }
      {
        category: 'AppServiceAppLogs'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

// Outputs required by azd
output RESOURCE_GROUP_ID string = resourceGroup().id
output AZURE_LOCATION string = location
output APPLICATIONINSIGHTS_CONNECTION_STRING string = applicationInsights.properties.ConnectionString
output SERVICE_VICTORIANUPDATER_SERVER_NAME string = appService.name
output SERVICE_VICTORIANUPDATER_SERVER_URI string = 'https://${appService.properties.defaultHostName}'
