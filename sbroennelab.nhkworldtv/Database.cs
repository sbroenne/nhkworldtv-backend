using Microsoft.Azure.Cosmos;
using System;

namespace sbroennelab.nhkworldtv
{
    /// Code to set-up the Cosmos DB

    public static class Database
    {

        public static string ContainerVodProgram = GetEnvironmentVariable("DATABSE_CONTAINER_VOD_PROGRAM");

        public static CosmosClient DatabaseClient = new(GetEnvironmentVariable("ENDPOINT_URL"), GetEnvironmentVariable("COSMOS_ACCOUNT_KEY"));

        // CosmosDB
        public static string DatabaseId = GetEnvironmentVariable("DATABASE_ID");

        public static Container VodProgram = DatabaseClient.GetContainer(DatabaseId, ContainerVodProgram);

        public static string GetEnvironmentVariable(string name)
        {
            string envVariable = System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            return envVariable;
        }
    }
}