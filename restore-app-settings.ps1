# Restore PoVicTranslate App Settings
$appName = "PoVicTranslate"
$resourceGroup = "PoVicTranslate"

# Set each setting individually
az webapp config appsettings set --name $appName --resource-group $resourceGroup --settings "APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=4b2e08ae-5aef-49db-8821-32747b96eee0;IngestionEndpoint=https://eastus2-3.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus2.livediagnostics.monitor.azure.com/;ApplicationId=f9bc666f-122d-4cb5-95ce-4a7bc24151a6"

az webapp config appsettings set --name $appName --resource-group $resourceGroup --settings "WEBSITE_RUN_FROM_PACKAGE=1"

az webapp config appsettings set --name $appName --resource-group $resourceGroup --settings "ApiSettings__AzureOpenAIApiKey=c6d7b92e244949c39277c9973f532a86"

az webapp config appsettings set --name $appName --resource-group $resourceGroup --settings "ApiSettings__AzureOpenAIEndpoint=https://povictranslate-openai.openai.azure.com/"

az webapp config appsettings set --name $appName --resource-group $resourceGroup --settings "ApiSettings__AzureOpenAIDeploymentName=gpt-4o"

az webapp config appsettings set --name $appName --resource-group $resourceGroup --settings "ApiSettings__AzureSpeechSubscriptionKey=41266d9a935649c48a4233a4861eb837"

az webapp config appsettings set --name $appName --resource-group $resourceGroup --settings "ApiSettings__AzureSpeechRegion=eastus2"

az webapp config appsettings set --name $appName --resource-group $resourceGroup --settings "ASPNETCORE_ENVIRONMENT=Production"

Write-Host "App settings restored successfully!"

# Restart the app
az webapp restart --name $appName --resource-group $resourceGroup

Write-Host "App restarted!"
