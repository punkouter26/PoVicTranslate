var builder = DistributedApplication.CreateBuilder(args);

// Reference existing Azure resources from PoShared resource group
var existingKeyVaultName = builder.AddParameter("existingKeyVaultName", secret: false);
var existingResourceGroup = builder.AddParameter("existingResourceGroup", secret: false);

// Connect to existing Key Vault in PoShared resource group
var keyVault = builder.AddAzureKeyVault("keyvault")
    .AsExisting(existingKeyVaultName, existingResourceGroup);

// Add the Blazor Web App (Server)
var web = builder.AddProject<Projects.PoVicTranslate_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(keyVault)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
