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

            int counter = 0;

            bool success = false;

            log.LogDebug("Processing episodes");
            foreach (var vodId in episodes)
            {
                success = false;
                var program = new VodProgram(vodId);
                success = await program.Get();
                if (success)
                {
                    counter++;
                }
            }
            return (counter);
        }

        /// <summary>
        /// Reload CosmosDB with the latest list of programs from NHK - used to update all entries
        /// </summary>
        /// <returns>Number of items written to CosmosDB</returns>
        public static async Task<int> ReloadCloudCacheFromNHK(ILogger log)
        {
            string getAllEpisodes = String.Format(NHK_ALL_EPISODES_URL, VodProgram.NHK_API_KEY);
            log.LogDebug("Getting Episode List from NHK");
            var response = await VodProgram.NHKHttpClient.GetAsync(getAllEpisodes);
            var contents = await response.Content.ReadAsStringAsync();
            JObject episodeList = JObject.Parse(contents);

            var episodes =
            from p in episodeList["data"]["episodes"]
            select (string)p["vod_id"];

            int counter = 0;

            bool success = false;

            log.LogDebug("Reloading episodes");
            foreach (var vodId in episodes)
            {
                success = false;
                var program = new VodProgram(vodId);
                success = await program.GetFromNHK();
                if (success)
                {
                    success = await program.Upsert();
                    if (success)
                        counter++;
                }
            }
            return (counter);
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
    }
}