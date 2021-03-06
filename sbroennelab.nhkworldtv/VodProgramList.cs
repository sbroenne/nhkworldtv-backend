using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using System.Linq;

namespace sbroennelab.nhkworldtv
{

    public class CacheEpisode
    {
        public string P1080P { get; set; }
        public string P720P { get; set; }
        public string OnAir { get; set; }
    }

    /// <summary>
    /// Get list of programs
    /// </summary>
    public static class VodProgramList
    {


        /// <summary>
        /// Populates CosmosDB with the latest list of programs
        /// </summary>
        /// <returns>Number of items written to CosmosDB</returns>
        public static async Task<bool> PopulateCloudCache(ILogger log)
        {

            var episodes = await NhkApi.GetVodIdList(log);

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
                    var program = new VodProgram(vodId, log);
                    success = await program.Get();
                    if (success)
                    {
                        insertCounter++;
                    }
                }
            }

            // Delete stale DB Entries
            foreach (var vodId in dbVodIds)
            {
                // Check if vodId no longer exists in NHK episode list
                if (!episodes.Contains(vodId))
                {
                    // It doesn't exist, delete it
                    success = false;
                    var program = new VodProgram(vodId, log);
                    success = await program.Delete();
                    if (success)
                    {
                        deleteCounter++;
                    }
                }
            }

            log.LogInformation(String.Format("Inserted {0} - deleted {1} episodes from CosmosDB", insertCounter, deleteCounter));

            return (true);

        }


        /// <summary>
        /// Get a list of program meta data from CosmosDB - API Version 2
        /// </summary>
        /// <param name="maxItems">Number of programs to return</param>
        /// <returns>Minimal JSON with just the 1080p and 720p Url plus the onAir-Timestamp </returns>
        public static async Task<string> GetProgramList(int maxItems)
        {
            var cacheEpisodeDict = new Dictionary<string, CacheEpisode>();
            var sqlQueryText = String.Format("SELECT TOP {0} c.id, c.Path1080P, c.Path720P, c.OnAir FROM c ORDER by c.LastUpdate DESC", maxItems);
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = Database.VodProgram.GetItemQueryIterator<VodProgram>(queryDefinition);
            var baseUrl = "https://nhkw-mzvod.akamaized.net/www60/mz-nhk10/_definst_/mp4:mm/flvmedia/5905";

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (VodProgram program in currentResultSet)
                {
                    var cacheEpisode = new CacheEpisode();
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