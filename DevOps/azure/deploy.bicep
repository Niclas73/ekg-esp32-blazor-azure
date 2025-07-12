// ─────────────────────────────────────────────────────────────
// deploy.bicep (resource group-level)
// Creates: RG-scoped free App Service Plan, Web App, SignalR Free
// ─────────────────────────────────────────────────────────────
param location string = resourceGroup().location
param appName  string = 'ekg-blazor-${uniqueString(resourceGroup().id)}'
param signalRName string = '${appName}-signalr'

// Free App Service Plan (F1)
resource plan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: 'ekgFreePlan'
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
    size: 'F1'
  }
  properties: {
    maximumElasticWorkerCount: 1
  }
}

// Web App
resource web 'Microsoft.Web/sites@2023-01-01' = {
  name: appName
  location: location
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: ''        // Windows (default)
      websocketsEnabled: true   // required by Blazor + SignalR
    }
  }
}

// Enable “WEBSITES_PORT” app setting only for containers. Not needed now.

// SignalR Service (Free tier: 20 connections, 20k msg/day)
resource signalr 'Microsoft.SignalRService/SignalR@2022-11-01' = {
  name: signalRName
  location: location
  sku: {
    name: 'Free_F1'
    tier: 'Free'
    capacity: 1
  }
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
    ]
  }
}

// Output app URLs
output webAppUrl string = 'https://${web.properties.defaultHostName}'
output signalREndpoint string = signalr.properties.externalIp
