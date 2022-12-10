#!/bin/bash      
resourceGroupName="nhkworldtv"
accountName="nhkdb"
readWriteRoleDefinitionId="00000000-0000-0000-0000-000000000002" # Cosmos DB Built-in Data Contributor
principalId="Your AAD user object id>" # Your AAD user object id
az cosmosdb sql role assignment create --account-name $accountName --resource-group $resourceGroupName --scope "/" --principal-id $principalId --role-definition-id $readWriteRoleDefinitionId
