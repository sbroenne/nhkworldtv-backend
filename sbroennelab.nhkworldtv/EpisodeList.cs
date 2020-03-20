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
using Azure.Cosmos;
using System.Linq;

namespace sbroennelab.nhkworldtv
{
    public class CacheEpisode
    {

        public string PlayPath { get; set; }
        public string Aspect { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string OnAir { get; set; }
    }

    /// <summary>
    /// NHK Video On Demand
    /// </summary>
    public class EpisodeList
    {
        private static string NHK_ALL_EPISODES_URL = "https://api.nhk.or.jp/nhkworld/vodesdlist/v7a/all/all/en/all/all.json?apikey={0}";

        public static async Task<int> PopulateCloudCache()
        {
            string getAllEpisodes = String.Format(NHK_ALL_EPISODES_URL, VodProgram.NHK_API_KEY);
            var response = await VodProgram.NHKHttpClient.GetAsync(getAllEpisodes);
            var contents = await response.Content.ReadAsStringAsync();
            JObject episodeList = JObject.Parse(contents);

            var episodes =
            from p in episodeList["data"]["episodes"]
            select (string)p["vod_id"];

            int counter = 0;

            bool success = false;

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

        public static async Task<string> GetProgramList(int maxItems)
        {
            Dictionary<string, CacheEpisode> cacheEpisodeDict = new Dictionary<string, CacheEpisode>();
            var sqlQueryText = String.Format("SELECT c.id, c.PlayPath, c.OnAir, c.Aspect, c.Width, c.Height FROM c ORDER by c.LastUpdate DESC OFFSET 0 LIMIT {0}", maxItems);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            List<VodProgram> programs = new List<VodProgram>();

            await foreach (VodProgram program in Database.VodProgram.GetItemQueryIterator<VodProgram>(queryDefinition))
            {
                var cacheEpisode = new CacheEpisode();
                string vodId = program.VodId;
                cacheEpisode.PlayPath = program.PlayPath;
                cacheEpisode.Aspect = program.Aspect;
                cacheEpisode.Width = program.Width;
                cacheEpisode.Height = program.Height;
                cacheEpisode.OnAir = program.OnAir;
                cacheEpisodeDict.Add(vodId, cacheEpisode);
            }

            string jsonString = JsonConvert.SerializeObject(cacheEpisodeDict);
            return (jsonString);

        }
    }
}