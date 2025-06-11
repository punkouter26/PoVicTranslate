// Creates the web App Service for the Victorian Translator application
// Configures the .NET application with proper runtime and monitoring

@description('The Azure region into which the resources should be deployed.')
param location string

@description('The name of the App Service app to create.')
param name string

@description('The ID of the App Service plan into which the app should be placed.')
param appServicePlanId string

@description('The Application Insights connection string to use for monitoring.')
param applicationInsightsConnectionString string

@description('Tags to apply to the App Service.')
param tags object = {}

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: name
  location: location
  tags: union(tags, { 'azd-service-name': 'web' })
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsightsConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'Recommended'
        }
      ]
    }
  }
}

output identityPrincipalId string = appService.identity.principalId
output name string = appService.name
output uri string = 'https://${appService.properties.defaultHostName}'
