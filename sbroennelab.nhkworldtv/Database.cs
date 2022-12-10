using Microsoft.Azure.Cosmos;
using System;
using Azure.Identity;

namespace sbroennelab.nhkworldtv
{
    /// Code to set-up the Cosmos DB

    public static class Database
    {

        private static readonly string ContainerVodProgram = GetEnvironmentVariable("DATABASE_CONTAINER_VOD_PROGRAM");

        private static readonly CosmosClient DatabaseClient = new(GetEnvironmentVariable("ENDPOINT_URL"), new DefaultAzureCredential());

        // CosmosDB
        private static readonly string DatabaseId = GetEnvironmentVariable("DATABASE_ID");

        public static readonly Container VodProgram = DatabaseClient.GetContainer(DatabaseId, ContainerVodProgram);

        public static string GetEnvironmentVariable(string name)
        {
            string envVariable = System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            return envVariable;
        }
    }
}