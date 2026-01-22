// PoVicTranslate Resources Module
// Container App using shared resources from PoShared
targetScope = 'resourceGroup'

@description('Location for all resources')
param location string

@description('Environment name')
param environmentName string

@description('Resource tags')
param tags object

@description('Shared resource group name')
param sharedResourceGroupName string = 'PoShared'

// Shared resource names from PoShared
var sharedContainerAppEnvName = 'cae-poshared'
var sharedAcrName = 'acrposhared'
var sharedAppInsightsName = 'appi-poshared'
var sharedKeyVaultName = 'kv-poshared'
var sharedManagedIdentityName = 'mi-poshared-apps'

// Container app name
var containerAppName = 'ca-povictranslate'

// Reference existing Container Apps Environment from PoShared
resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: sharedContainerAppEnvName
  scope: resourceGroup(sharedResourceGroupName)
}

// Reference existing Container Registry from PoShared
resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
  name: sharedAcrName
  scope: resourceGroup(sharedResourceGroupName)
}

// Reference existing Application Insights from PoShared
resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: sharedAppInsightsName
  scope: resourceGroup(sharedResourceGroupName)
}

// Reference existing Key Vault from PoShared
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: sharedKeyVaultName
  scope: resourceGroup(sharedResourceGroupName)
}

// Reference existing Managed Identity from PoShared
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: sharedManagedIdentityName
  scope: resourceGroup(sharedResourceGroupName)
}

// Container App
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      registries: [
        {
          server: '${acr.name}.azurecr.io'
          identity: managedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'appinsights-connection-string'
          value: appInsights.properties.ConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'povictranslate-web'
          image: '${acr.name}.azurecr.io/povictranslate-web:latest'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              secretRef: 'appinsights-connection-string'
            }
            {
              name: 'KeyVault__Name'
              value: sharedKeyVaultName
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: managedIdentity.properties.clientId
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 3
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '10'
              }
            }
          }
        ]
      }
    }
  }
}

// Outputs
output containerAppName string = containerApp.name
output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output appInsightsConnectionString string = appInsights.properties.ConnectionString
