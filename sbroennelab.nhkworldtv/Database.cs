using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Cosmos;

namespace sbroennelab.nhkworldtv
{
    /// Code to set-up the Cosmos DB

    public static class Database
    {

        public static string GetEnvironmentVariable(string name)
        {
            string envVariable = System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            return envVariable;
        }

        // CosmosDB

        public static CosmosClient DatabaseClient = new CosmosClient(GetEnvironmentVariable("ENDPOINT_URL"), GetEnvironmentVariable("COSMOS_ACCOUNT_KEY"));
        public static string DatabaseId = GetEnvironmentVariable("DATABASE_ID");
        public static string ContainerVodProgram = GetEnvironmentVariable("DATABSE_CONTAINER_VOD_PROGRAM");
        public static CosmosContainer VodProgram = DatabaseClient.GetContainer(DatabaseId, ContainerVodProgram);
 

    }
}