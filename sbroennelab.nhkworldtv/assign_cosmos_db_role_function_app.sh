#!/bin/bash      
resourceGroupName="nhkworldtv"
accountName="nhkdb"
readWriteRoleDefinitionId="00000000-0000-0000-0000-000000000002" # Cosmos DB Built-in Data Contributor
principalId="<Object Id of your function app>" # Your function app managed identiy 
az cosmosdb sql role assignment create --account-name $accountName --resource-group $resourceGroupName --scope "/" --principal-id $principalId --role-definition-id $readWriteRoleDefinitionId
