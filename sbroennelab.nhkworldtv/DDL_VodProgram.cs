using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Cosmos;

namespace sbroennelab.nhkworldtv
{
    /// Code to set-up the Cosmos DB

    public static class DDL_VodProgram
    {
      
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        public static async Task<bool> CreateDatabaseAsync()
        {
            // Create a new database
            CosmosDatabase database = await Database.DatabaseClient.CreateDatabaseIfNotExistsAsync(Database.DatabaseId);
            Console.WriteLine("Created Database: {0}\n", database.Id);
            return true;
        }

        /// <summary>
        /// Create the VodProgram container if it does not exist. 
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CreateVodProgramContainerAsync()
        {
            // Create a new container
            CosmosContainer container = await Database.DatabaseClient.GetDatabase(Database.DatabaseId).CreateContainerIfNotExistsAsync(Database.ContainerVodProgram, "/PartitionKey");
            Console.WriteLine("Created Container: {0}\n", container.Id);
            return true;
        }
    }
}
