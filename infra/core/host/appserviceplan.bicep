// Creates an Azure App Service Plan
// Using Linux-based plan for .NET applications with minimal B1 SKU

@description('The Azure region into which the resources should be deployed.')
param location string

@description('A unique suffix to add to resource names that need to be globally unique.')
param name string

@description('The name of the SKU to use when creating the App Service Plan.')
param sku object = {
  name: 'B1'
  capacity: 1
}

@description('The kind of app service plan to create.')
param kind string = 'linux'

@description('Tags to apply to the App Service Plan.')
param tags object = {}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: name
  location: location
  sku: sku
  kind: kind
  properties: {
    reserved: kind == 'linux'
  }
  tags: tags
}

output id string = appServicePlan.id
output name string = appServicePlan.name
