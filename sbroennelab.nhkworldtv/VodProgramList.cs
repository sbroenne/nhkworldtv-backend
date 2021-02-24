using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using System.Linq;

namespace sbroennelab.nhkworldtv
{
    /// <summary>
    /// Class to hold the program meta data and will be serialized to JSON
    /// </summary>
    public class CacheEpisode
    {

        public string Path1080P { get; set; }
        public string Path720P { get; set; }
        public string Aspect { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string OnAir { get; set; }
    }

    public class CacheEpisodeV2
    {
        public string P1080P { get; set; }
        public string P720P { get; set; }
        public string OnAir { get; set; }
    }

    /// <summary>
    /// Get list of programs
    /// </summary>
    public class VodProgramList
    {
        private static string NHK_ALL_EPISODES_URL = "https://api.nhk.or.jp/nhkworld/vodesdlist/v7a/all/all/en/all/all.json?apikey={0}";

        /// <summary>
        /// Populates CosmosDB with the latest list of programs
        /// </summary>
        /// <returns>Number of items written to CosmosDB</returns>
        public static async Task<int> PopulateCloudCache(ILogger log)
        {
            string getAllEpisodes = String.Format(NHK_ALL_EPISODES_URL, VodProgram.NHK_API_KEY);
            log.LogDebug("Getting Episode List from NHK");
            var response = await VodProgram.NHKHttpClient.GetAsync(getAllEpisodes);
            var contents = await response.Content.ReadAsStringAsync();
            JObject episodeList = JObject.Parse(contents);

            var episodes =
            from p in episodeList["data"]["episodes"]
            select (string)p["vod_id"];

            // Get the existing entries from CosmosDB
            var sqlQueryText = "SELECT c.id FROM c";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<VodProgram> queryResultSetIterator = Database.VodProgram.GetItemQueryIterator<VodProgram>(queryDefinition);


            var dbEpisodes = new List<VodProgram>();
            int insertCounter = 0;
            int deleteCounter = 0;
            bool success = false;
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<VodProgram> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                dbEpisodes.AddRange(currentResultSet);
            }

            var dbVodIds = from p in dbEpisodes
                           select p.VodId;

            // Update CosmosDB with new entries
            foreach (var vodId in episodes)
            {
                // Check if vodId is already in CosmosDB
                if (!dbVodIds.Contains(vodId))
                {
                    // No, create it it!
                    success = false;
                    var program = new VodProgram(vodId);
                    success = await program.Get();
                    if (success)
                    {
                        insertCounter++;
                    }
                }
            }

            // Delete stale DB Entrie
            foreach (var vodId in dbVodIds)
            {
                // Check if vodId no longer exists in NHK episode list
                if (!episodes.Contains(vodId))
                {
                    // It doesn't exist, delete it
                    success = false;
                    var program = new VodProgram(vodId);
                    success = await program.Delete();
                    if (success)
                    {
                        deleteCounter++;
                    }
                }
            }

            log.LogInformation(String.Format("Inserted {0} - deleted {1} episodes", insertCounter, deleteCounter));

            return (insertCounter);

        }



        /// <summary>
        /// Get a list of program meta data from CosmosDB
        /// </summary>
        /// <param name="maxItems">Number of prgrams to return</param>
        /// <returns>JSON with key attributes like Path1080P, Width, etc.</returns>
        public static async Task<string> GetProgramList(int maxItems)
        {
            Dictionary<string, CacheEpisode> cacheEpisodeDict = new Dictionary<string, CacheEpisode>();
            var sqlQueryText = String.Format("SELECT TOP {0} c.id, c.Path1080P, c.Path720P, c.Aspect, c.Width, c.Height, c.OnAir FROM c ORDER by c.LastUpdate DESC", maxItems);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<VodProgram> queryResultSetIterator = Database.VodProgram.GetItemQueryIterator<VodProgram>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<VodProgram> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (VodProgram program in currentResultSet)
                {
                    var cacheEpisode = new CacheEpisode();
                    string vodId = program.VodId;
                    cacheEpisode.Path1080P = program.Path1080P;
                    cacheEpisode.Path720P = program.Path720P;
                    cacheEpisode.Aspect = program.Aspect;
                    cacheEpisode.Width = program.Width;
                    cacheEpisode.Height = program.Height;
                    cacheEpisode.OnAir = program.OnAir;
                    cacheEpisodeDict.Add(vodId, cacheEpisode);
                }
            }

            string jsonString = JsonConvert.SerializeObject(cacheEpisodeDict);
            return (jsonString);
        }

        /// <summary>
        /// Get a list of program meta data from CosmosDB - Version 2
        /// </summary>
        /// <param name="maxItems">Number of programs to return</param>
        /// <returns>Minimal JSON</returns>
        public static async Task<string> GetProgramListV2(int maxItems)
        {
            var cacheEpisodeDict = new Dictionary<string, CacheEpisodeV2>();
            var sqlQueryText = String.Format("SELECT TOP {0} c.id, c.Path1080P, c.Path720P, c.OnAir FROM c ORDER by c.LastUpdate DESC", maxItems);
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = Database.VodProgram.GetItemQueryIterator<VodProgram>(queryDefinition);
            var baseUrl = "https://nhkw-mzvod.akamaized.net/www60/mz-nhk10/_definst_/mp4:mm/flvmedia/5905";

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (VodProgram program in currentResultSet)
                {
                    var cacheEpisode = new CacheEpisodeV2();
                    string vodId = program.VodId;
                    /// Some episodes do not have a 1080P file
                    if (program.Path1080P != null)
                        cacheEpisode.P1080P = program.Path1080P.Replace(baseUrl, "");

                    cacheEpisode.P720P = program.Path720P.Replace(baseUrl, "");
                    cacheEpisode.OnAir = program.OnAir;
                    cacheEpisodeDict.Add(vodId, cacheEpisode);
                }
            }

            string jsonString = JsonConvert.SerializeObject(cacheEpisodeDict);
            return (jsonString);
        }
    }
}