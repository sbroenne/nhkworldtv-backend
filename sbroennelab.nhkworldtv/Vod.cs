using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
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

    public static class Vod
    {
        // Create a single, static HttpClient
        private static HttpClient httpClient = new HttpClient();
        // Define a regular expression for repeated words.
        private static Regex rx = new Regex(@"'data-de-program-uuid','(.+?)'",
              RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=nhkdb;AccountKey=WGDqURUL63TNF1WnqQKNXCmnupVu3eMCZeqEF0DpSrOZdpQq9sVGYEpPzDYLuCeMUCFB2eL9krjZfQ3C6zrHrg==;TableEndpoint=https://nhkdb.table.cosmos.azure.com:443/;";
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudTable programTable = tableClient.GetTableReference("VodProgram");

        private static string partitionKey = "default";

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
            var playerUrl = String.Format("https://movie-s.nhk.or.jp/v/refid/nhkworld/prefid/{0}?embed=js&targetId=videoplayer&de-responsive=true&de-callback-method=nwCustomCallback&de-appid={1}&de-subtitle-on=false", vodId, vodId);
            var response = await httpClient.GetAsync(playerUrl);
            var contents = await response.Content.ReadAsStringAsync();

            // Extract the Program Uuid
            MatchCollection matches = rx.Matches(contents.ToString());
            string matched = matches[0].Value;
            matched = matched.Replace("\"", "");
            matched = matched.Replace("'", "");
            string programUuid = matched.Split(",")[1];

            return (programUuid);
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

        [FunctionName("GetVodProgram")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Program/{vodId}")] HttpRequest req, string vodId,
            ILogger log)
        {
            var vodProgram = await GetVodProgram(vodId);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString;
            jsonString = JsonSerializer.Serialize(vodProgram, options);

            return new OkObjectResult(jsonString);
        }
    }
}
