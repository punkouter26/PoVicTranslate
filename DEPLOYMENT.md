# CI/CD Setup Instructions

## Prerequisites for Azure Deployment

To use this CI/CD pipeline, you need to configure the following:

### 1. Azure Service Principal
Create a service principal for GitHub Actions authentication:

```powershell
# Login to Azure
az login

# Create a service principal
az ad sp create-for-rbac --name "PoVicTranslate-GitHub-Actions" --role contributor --scopes /subscriptions/{subscription-id}/resourceGroups/PoVicTranslate --sdk-auth
```

### 2. GitHub Secrets
Add the following secrets to your GitHub repository (Settings > Secrets and variables > Actions):

- **AZURE_CREDENTIALS**: The JSON output from the service principal creation command above

### 3. Azure Resources
Ensure the following Azure resources exist:
- **Resource Group**: `PoVicTranslate`
- **App Service**: `povictranslate` (Linux-based, .NET 9.0 runtime)
- **App Service Plan**: Associated with the App Service

### 4. App Service Configuration
The App Service should be configured with:
- **Runtime**: .NET 9.0
- **Operating System**: Linux
- **Publish**: Code (not container)

## Pipeline Features

This CI/CD pipeline includes:

✅ **Multi-branch support** (main/master)  
✅ **Manual workflow dispatch**  
✅ **Dependency caching** for faster builds  
✅ **Solution-level build** for both client and server projects  
✅ **Server project publishing** (includes Blazor WASM client)  
✅ **Azure authentication** via service principal  
✅ **Production deployment slot**  
✅ **Deployment verification** with health check  

## Project Structure

The pipeline expects:
- Solution file: `VictorianTranslator.sln` in root
- Server project: `VictorianTranslator.Server/VictorianTranslator.Server.csproj`
- Client project: `VictorianTranslator.Client/VictorianTranslator.Client.csproj` (referenced by server)

## Troubleshooting

### Common Issues:
1. **Authentication failures**: Verify AZURE_CREDENTIALS secret is correctly formatted
2. **Resource not found**: Ensure App Service and Resource Group exist
3. **Build failures**: Check .NET version compatibility (9.0.x)
4. **Deployment timeouts**: Increase verification wait time if needed

### Logs:
- Check GitHub Actions logs for build/deployment details
- Check Azure App Service logs for runtime issues
- Use Application Insights for application telemetry
