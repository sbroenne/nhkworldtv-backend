using Microsoft.Extensions.Logging;
using System.Text.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace sbroennelab.nhkworldtv
{
    /// <summary> Implements NHK API </summary>
    public static class NhkApi
    {
        /// <summary>
        /// Gets a substring from a source
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strStart"></param>
        /// <param name="strEnd"></param>
        /// <returns></returns>
        private static string getBetween(string strSource, string strStart, string strEnd)
        {
            const int kNotFound = -1;

            var startIdx = strSource.IndexOf(strStart);
            if (startIdx != kNotFound)
            {
                startIdx += strStart.Length;
                var endIdx = strSource.IndexOf(strEnd, startIdx);
                if (endIdx > startIdx)
                {
                    return strSource.Substring(startIdx, endIdx - startIdx);
                }
            }
            return string.Empty;
        }

        // Create static classes so that they will be re-used between calls

        private static readonly string NHK_ALL_EPISODES_URL = "https://nwapi.nhk.jp/nhkworld/vodesdlist/v7b/all/all/en/all/all.json";

        private static readonly string NHK_GET_EPISODE_DETAIL_URL = "https://nwapi.nhk.jp/nhkworld/vodesdlist/v7b/vod_id/{0}/en/all/1.json";


        // NHK APIs
        private static readonly string NHK_MOVIE_CONTENT_PLAYER_URL = "https://www3.nhk.or.jp/nhkworld/common/player/tv/vod/world/player/js/movie-content-player.js";

        // HTTP
        private static readonly HttpClient NHKHttpClient = new();


        /// <summary>Retrieves the video asset information for a program/episode</summary>
        /// <param name="VodId">VodId for retrieving the asset</param>
        /// <param name="log"></param>
        /// <returns>A Stream with the video asset information</returns>
        public static async Task<Stream> GetStream(string VodId, ILogger log)
        {
            string videoUrl = await GetMediaInformationApiUrl(VodId, log);
            try
            {
                var response = await NHKHttpClient.GetAsync(videoUrl);
                if (!response.IsSuccessStatusCode)
                {
                    log.LogInformation("Retrying getting stream information from NHK for VodId: {0}", VodId);
                    return null;
                }
                var contents = await response.Content.ReadAsStringAsync();
                try
                {
                    Stream stream = JsonSerializer.Deserialize<Stream>(contents);
                    return stream;
                }
                catch (JsonException ex)
                {
                    throw ex;
                }
            }
            catch (HttpRequestException ex)
            {
                log.LogError("Couldn't load asset for VodId: {0} - Message: {1}", VodId, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Retrieves the detail information for an episode
        /// <param name="VodId">VodId from the NHK Api</param>
        /// </summary>
        public static async Task<Episode> GetEpisode(string VodId, ILogger log)
        {
            string episodeUrl = string.Format(NHK_GET_EPISODE_DETAIL_URL, VodId);
            try
            {
                var response = await NHKHttpClient.GetAsync(episodeUrl);
                var contents = await response.Content.ReadAsStringAsync();

                try
                {
                    var vodEpisodes = JsonSerializer.Deserialize<VodEpisodes>(contents);
                    if (vodEpisodes.data.episodes.Length == 1)
                    {
                        var episode = vodEpisodes.data.episodes[0];
                        return episode;
                    }
                    else
                    {
                        log.LogWarning("No episode detail for: {0}", VodId);
                        return null;
                    }
                }
                catch (JsonException ex)
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

        // <summary>Extracts the Media Information Url from the NHK Web Player - very slow operation!</summary>
        /// <param name="VodId">VodId from the NHK Api</param>
        /// <param name="log"></param>
        public static async Task<string> GetMediaInformationApiUrl(string VodId, ILogger log)
        {
            var playerUrl = NHK_MOVIE_CONTENT_PLAYER_URL;
            try
            {
                var response = await NHKHttpClient.GetAsync(playerUrl);
                if (response.IsSuccessStatusCode)
                {
                    var contents = await response.Content.ReadAsStringAsync();

                    // First, find the block in the js that contains the prod token and url
                    var prodBlock = getBetween(contents, "prod:{", "}.prod;");
                    // Extract the relevant information
                    var apiUrl = getBetween(prodBlock, "apiUrl:\"", "\"");
                    var token = getBetween(prodBlock, "token:\"", "\"");
                    string mediaInformationApiUrl = string.Format("{0}/?token={1}&type=json&optional_id={2}&active_flg=1", apiUrl, token, VodId);
                    
                    
                    // check if the URI is correct
                    if (Uri.IsWellFormedUriString(mediaInformationApiUrl, UriKind.Absolute))
                        return mediaInformationApiUrl;
                    else
                    {
                        log.LogError("Could not construct valid GetMediaInformationApiUrl: {0}", mediaInformationApiUrl);
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


        /// <summary>Get List of VodIds from NHK</summary>
        /// <param name="log"></param>
        /// <returns>List of VodIds</returns>
        public static async Task<List<string>> GetVodIdList(ILogger log)
        {
            string getAllEpisodes = NHK_ALL_EPISODES_URL;
            log.LogInformation("Getting Episode List from NHK");
            try
            {
                var response = await NHKHttpClient.GetAsync(getAllEpisodes);
                if (response.IsSuccessStatusCode)
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    VodEpisodes episodeList = JsonSerializer.Deserialize<VodEpisodes>(contents);

                    var episodes =
                    from p in episodeList.data.episodes
                    select p.vod_id;

                    var returnList = episodes.ToList<string>();
                    log.LogInformation("Retrieved episode list with {0} entries", returnList.Count);

                    return returnList;
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
    }
}