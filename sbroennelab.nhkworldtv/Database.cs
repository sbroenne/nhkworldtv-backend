using Azure.Identity;
using Microsoft.Azure.Cosmos;
using System;

namespace sbroennelab.nhkworldtv
{
    /// Code to set-up the Cosmos DB

    public static class Database
    {

        private static readonly string ContainerVodProgram = Environment.GetEnvironmentVariable("DATABASE_CONTAINER_VOD_PROGRAM", EnvironmentVariableTarget.Process);

        private static readonly CosmosClient DatabaseClient = new(Environment.GetEnvironmentVariable("ENDPOINT_URL", EnvironmentVariableTarget.Process), new DefaultAzureCredential());

        // CosmosDB
        private static readonly string DatabaseId = Environment.GetEnvironmentVariable("DATABASE_ID", EnvironmentVariableTarget.Process);

        public static readonly Container VodProgram = DatabaseClient.GetContainer(DatabaseId, ContainerVodProgram);

    }
}