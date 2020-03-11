using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos.Table;



namespace sbroennelab.nhkworldtv
{
    public class ProgramEntity : TableEntity
    {
        public ProgramEntity()
        {
        }

        public ProgramEntity(string vodId, string partitionKey)
        {
            PartitionKey = partitionKey;
            RowKey = vodId;
        }

        public string ProgramUuid { get; set; }

    }

    public static class Program
    {
        public static string GetEnvironmentVariable(string name)
        {
            string envVariable = System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            return envVariable;
        }

        // Create a static clients (HTTP, Table, etc.)
        private static HttpClient httpClient = new HttpClient();

        // CosmosDB
        private static string cosmosConnectionString = String.Format("DefaultEndpointsProtocol=https;AccountName=nhkdb;AccountKey={0};TableEndpoint=https://nhkdb.table.cosmos.azure.com:443/;", GetEnvironmentVariable("COSMOS_ACCOUNT_KEY"));
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cosmosConnectionString);
        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudTable programTable = tableClient.GetTableReference("VodProgram");
        private static string partitionKey = "default";

        //RegExes
        private static Regex rxPlayer = new Regex(@"'data-de-program-uuid','(.+?)'",
              RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex rxVodId = new Regex("'vod_id':?'(.+?)'",
                                 RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // NHK API
        private static string NHK_API_KEY = GetEnvironmentVariable("NHK_API_KEY");
        private static string NHK_PLAYER_URL = GetEnvironmentVariable("NHK_PLAYER_URL");
        private static string NHK_ALL_EPISODES_URL = GetEnvironmentVariable("NHK_ALL_EPISODES_URL");

        public static async Task<ProgramEntity> InsertProgramEntity(CloudTable table, ProgramEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                ProgramEntity insertedProgram = result.Result as ProgramEntity;
                return insertedProgram;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public static async Task<ProgramEntity> RetrieveProgramEntitiy(CloudTable table, string partitionKey, string rowKey)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<ProgramEntity>(partitionKey, rowKey);
                TableResult result = await table.ExecuteAsync(retrieveOperation);
                ProgramEntity customer = result.Result as ProgramEntity;
                return customer;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
        public static async Task<String> GetProgramUuid(string vodId)
        {
            string playerUrl = String.Format(NHK_PLAYER_URL, vodId, vodId);
            var response = await httpClient.GetAsync(playerUrl);
            var contents = await response.Content.ReadAsStringAsync();

            // Extract the Program Uuid
            MatchCollection matches = rxPlayer.Matches(contents);
            string matched = matches[0].Value;
            matched = matched.Replace("\"", "");
            matched = matched.Replace("'", "");
            string programUuid = matched.Split(",")[1];

            return (programUuid);
        }


        public static async Task<bool> PopulateCloudCache()
        {

            string getAllEpisodes = String.Format(NHK_ALL_EPISODES_URL, NHK_API_KEY);

            var response = await httpClient.GetAsync(getAllEpisodes);
            var contents = await response.Content.ReadAsStringAsync();
            contents = contents.Replace("\"", "'");

            MatchCollection matches = rxVodId.Matches(contents);

            foreach (Match match in matches)
            {
                string matched = match.Value;
                matched = matched.Replace("'", "");
                string vod_id = matched.Split(":")[1];
                ProgramEntity vodProgram = await GetVodProgram(vod_id);

            }

            return (true);
        }
        public static async Task<ProgramEntity> GetVodProgram(string vodId)
        {

            // Retrieve Program from CosmosDB
            var vodProgram = await RetrieveProgramEntitiy(programTable, partitionKey, vodId);
            if (vodProgram == null)
            {
                // Not ins CosmosDB Retrieve Program from HNK
                var programUuid = await GetProgramUuid(vodId);

                // Store in CosmosDB
                vodProgram = new ProgramEntity(vodId, partitionKey);
                vodProgram.ProgramUuid = programUuid;
                vodProgram = await InsertProgramEntity(programTable, vodProgram);
            }
            // Return the program
            return (vodProgram);
        }
    }
}
