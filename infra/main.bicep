targetScope = 'resourceGroup'

@minLength(1)
@maxLength(64)
@description('Name of the environment used for resource naming')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string = resourceGroup().location

// Generate a unique token for resource names
var resourceToken = uniqueString(subscription().id, resourceGroup().id, location, environmentName)
var resourcePrefix = 'vts' // Victorian Translator Service

// Reference to the existing shared App Service Plan in PoShared resource group
resource existingAppServicePlan 'Microsoft.Web/serverfarms@2023-12-01' existing = {
  name: 'PoSharedAppServicePlan'
  scope: resourceGroup('PoShared')
}

// Reference to the existing shared Application Insights in PoShared resource group
resource existingApplicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: 'PoSharedApplicationInsights'
  scope: resourceGroup('PoShared')
}

// Reference to the existing shared Log Analytics Workspace in PoShared resource group
resource existingLogAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: 'log-iucwaxzqf3hni'
  scope: resourceGroup('PoShared')
}

// User-assigned managed identity for the app service
resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'az-${resourcePrefix}-${resourceToken}'
  location: location
  tags: {
    'azd-env-name': environmentName
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: 'PoVicTranslate' // Following the naming convention Po<SolutionName>
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: existingAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      cors: {
        allowedOrigins: ['*']
        supportCredentials: false
      }
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: existingApplicationInsights.properties.ConnectionString
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: userAssignedIdentity.properties.clientId
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        // Azure OpenAI configuration - to be set manually in Azure Portal
        {
          name: 'AZURE_OPENAI_ENDPOINT'
          value: 'https://posharedopenaieastus.openai.azure.com/'
        }
        {
          name: 'AZURE_OPENAI_API_KEY'
          value: 'your-api-key-here'
        }
        // Speech service configuration - to be set manually in Azure Portal
        {
          name: 'SPEECH_REGION'
          value: 'eastus2'
        }
        {
          name: 'AZURE_SPEECH_API_KEY'
          value: 'your-speech-key-here'
        }
      ]
    }
  }
  tags: {
    'azd-env-name': environmentName
    'azd-service-name': 'victorianupdater-server'
  }
}

// App Service diagnostic settings
resource appServiceDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: appService
  name: 'app-service-diagnostics'
  properties: {
    workspaceId: existingLogAnalyticsWorkspace.id
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

// Site extension for Application Insights
resource siteExtension 'Microsoft.Web/sites/siteextensions@2023-12-01' = {
  parent: appService
  name: 'Microsoft.ApplicationInsights.AzureWebSites'
}

// Outputs required by azd
output RESOURCE_GROUP_ID string = resourceGroup().id
output AZURE_LOCATION string = location
output APPLICATIONINSIGHTS_CONNECTION_STRING string = existingApplicationInsights.properties.ConnectionString
output SERVICE_VICTORIANUPDATER_SERVER_IDENTITY_PRINCIPAL_ID string = userAssignedIdentity.properties.principalId
output SERVICE_VICTORIANUPDATER_SERVER_NAME string = appService.name
output SERVICE_VICTORIANUPDATER_SERVER_URI string = 'https://${appService.properties.defaultHostName}'
