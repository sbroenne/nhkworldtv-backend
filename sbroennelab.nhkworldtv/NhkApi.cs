using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace sbroennelab.nhkworldtv
{

    /// <summary>
    /// Get list of programs
    /// </summary>
    public static class NhkApi
    {
        // Create a static classes so that they will be re-used between calls

        // HTTP
        private static HttpClient NHKHttpClient = new HttpClient();

        //RegEx
        private static Regex rxPlayer = new Regex(@"'data-de-program-uuid','(.+?)'",
             RegexOptions.Compiled | RegexOptions.IgnoreCase);


        // NHK APIs

        private static string NHK_ALL_EPISODES_URL = "https://api.nhk.or.jp/nhkworld/vodesdlist/v7a/all/all/en/all/all.json?apikey={0}";

        private static string NHK_PLAYER_URL = "https://movie-s.nhk.or.jp/v/refid/nhkworld/prefid/{0}?embed=js&targetId=videoplayer&de-responsive=true&de-callback-method=nwCustomCallback&de-appid={1}&de-subtitle-on=false";
        public static string NHK_API_KEY = "EJfK8jdS57GqlupFgAfAAwr573q01y6k";
        private static string NHK_VIDEO_URL = "https://movie-s.nhk.or.jp/ws/ws_program/api/67f5b750-b419-11e9-8a16-0e45e8988f42/apiv/5/mode/json?v={0}";
        private static string NHK_GET_EPISODE_DETAIL_URL = "https://api.nhk.or.jp/nhkworld/vodesdlist/v7a/vod_id/{0}/en/all/1.json?apikey={1}";


        /// <summary>
        /// Get List of VodIds from NHK
        /// </summary>
        /// <returns>List of VodIds</returns>
        public static async Task<List<string>> GetVodIdList(ILogger log)
        {

            string getAllEpisodes = String.Format(NHK_ALL_EPISODES_URL, NHK_API_KEY);
            log.LogInformation("Getting Episode List from NHK");
            try
            {
                var response = await NHKHttpClient.GetAsync(getAllEpisodes);
                if (response.IsSuccessStatusCode)
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    JObject episodeList = JObject.Parse(contents);

                    var episodes =
                    from p in episodeList["data"]["episodes"]
                    select (string)p["vod_id"];

                    var returnList = episodes.ToList<string>();
                    log.LogInformation("Retrieved episode list with {0} entries", returnList.Count);

                    return (returnList);
                }
                else
                {
                    log.LogError("Couldn't retrieve episode list - HTTP Status: {0} ", response.StatusCode);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                log.LogError("Couldn't load VodIdList - Message: {0}", ex.Message);
                return null;
            }

        }


        /// <summary>
        /// Extracts the Program Uuid for a given VodId from the NHK Web Player - very slow operation!
        /// </summary>
        public static async Task<string> GetProgramUuid(string VodId, ILogger log)
        {
            var playerUrl = String.Format(NHK_PLAYER_URL, VodId, VodId);
            try
            {
                var response = await NHKHttpClient.GetAsync(playerUrl);
                if (response.IsSuccessStatusCode)
                {
                    var contents = await response.Content.ReadAsStringAsync();

                    // Extract the Program Uuid
                    MatchCollection matches = rxPlayer.Matches(contents);
                    if (matches.Count == 1)
                    {
                        // Extract the Program Uuid
                        var matched = matches[0].Value;
                        matched = matched.Replace("\"", "");
                        matched = matched.Replace("'", "");
                        var programUuid = matched.Split(",")[1];
                        log.LogInformation("Extracted Program Uuid: {0} - VodId: {1}", programUuid, VodId);
                        return (programUuid);
                    }
                    else
                    {
                        log.LogError("Couldn't extract Program Uuid for VodId: {0}", VodId);
                        return null;
                    }
                }
                else
                {
                    log.LogError("Couldn't successfully load Player Url for VodId: {0} - HTTP Status: {1}", VodId, response.StatusCode);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                log.LogError("Couldn't load Player Url for VodId: {0} - Message: {1}", VodId, ex.Message);
                return null;
            }

        }


        /// <summary>
        /// Retrieves the video asset information for a program/episode
        /// </summary>
        public static async Task<JObject> GetAsset(string ProgramUuid, ILogger log)
        {
            string videoUrl = String.Format(NHK_VIDEO_URL, ProgramUuid);
            try
            {
                var response = await NHKHttpClient.GetAsync(videoUrl);
                if (!response.IsSuccessStatusCode)
                {
                    // Wait two sec & try again for second time, NHK sometimes has issues
                    log.LogInformation("Retrying getting assets from NHK for program: {0}", ProgramUuid);
                    System.Threading.Thread.Sleep(2000);
                    response = await NHKHttpClient.GetAsync(videoUrl);
                }
                var contents = await response.Content.ReadAsStringAsync();
                try
                {
                    var video = JObject.Parse(contents);
                    var asset = (JObject)video["response"]["WsProgramResponse"]["program"]["asset"];
                    return (asset);
                }
                catch (JsonReaderException ex)
                {
                    throw ex;
                }
            }
            catch (HttpRequestException ex)
            {
                log.LogError("Couldn't load asset for ProgramUuid: {0} - Message: {1}", ProgramUuid, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Retrieves the detail information for an episode
        /// </summary>
        public static async Task<JObject> GetEpisode(string VodId, ILogger log)
        {
            string episodeUrl = String.Format(NHK_GET_EPISODE_DETAIL_URL, VodId, NHK_API_KEY);
            try
            {
                var response = await NHKHttpClient.GetAsync(episodeUrl);
                var contents = await response.Content.ReadAsStringAsync();

                try
                {
                    var episode = JObject.Parse(contents);
                    if (episode["data"]["episodes"].Count() == 1)
                    {
                        var episodeDetail = (JObject)episode["data"]["episodes"][0];
                        return (episodeDetail);
                    }
                    else
                    {
                        log.LogWarning("No episode detail for: {0}", VodId);
                        return null;
                    }
                }
                catch (JsonReaderException ex)
                {
                    log.LogWarning("Couldn't parse JSON for episode: {0} - Message : {1}", VodId, ex.Message);
                    throw ex;
                }
            }
            catch (HttpRequestException ex)
            {
                log.LogError("Couldn't get episode details for VodId: {0} - Message: {1}", VodId, ex.Message);
                return null;
            }

        }
    }
}