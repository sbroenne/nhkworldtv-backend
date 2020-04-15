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
    /// <summary>
    /// NHK Video On Demand
    /// </summary>
    public class VodProgram
    {
        public VodProgram(string vodId)
        {
            VodId = vodId;
            LastUpdate = DateTime.UtcNow;
            PartitionKey = "default";

        }

        public VodProgram()
        {
            LastUpdate = DateTime.UtcNow;
            PartitionKey = "default";

        }


        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string VodId { get; set; }
        public string PartitionKey { get; }
        public string ProgramUuid { get; set; }
        public string PlayPath { get; set; }
        public string M3u8Path { get; set; }
        public string Aspect { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Title { get; set; }
        public string Plot { get; set; }
        public string PgmNo { get; set; }
        public string OnAir { get; set; }
        public string Duration { get; set; }
        public Boolean HasReferenceFile { get; set; }
        public DateTime LastUpdate { get; set; }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }

        // NHK API
        public static string NHK_API_KEY = "EJfK8jdS57GqlupFgAfAAwr573q01y6k";
        private static string NHK_PLAYER_URL = "https://movie-s.nhk.or.jp/v/refid/nhkworld/prefid/{0}?embed=js&targetId=videoplayer&de-responsive=true&de-callback-method=nwCustomCallback&de-appid={1}&de-subtitle-on=false";
        private static string NHK_VIDEO_URL = "https://movie-s.nhk.or.jp/ws/ws_program/api/67f5b750-b419-11e9-8a16-0e45e8988f42/apiv/5/mode/json?v={0}";
        private static string NHK_GET_EPISODE_DETAIL_URL = "https://api.nhk.or.jp/nhkworld/vodesdlist/v7a/vod_id/{0}/en/all/1.json?apikey={1}";

        // Create a static clients (HTTP, Table, etc.)
        public static HttpClient NHKHttpClient = new HttpClient();

        //RegExes
        private static Regex rxPlayer = new Regex(@"'data-de-program-uuid','(.+?)'",
             RegexOptions.Compiled | RegexOptions.IgnoreCase);


        /// <summary>
        /// Extracts the Program Uuid for a given VodId from the NHK Web Player - very slow operation!
        /// </summary>
        public async Task<bool> GetProgramUuidFromNHK()
        {
            string playerUrl = String.Format(NHK_PLAYER_URL, this.VodId, this.VodId);
            var response = await NHKHttpClient.GetAsync(playerUrl);
            var contents = await response.Content.ReadAsStringAsync();

            // Extract the Program Uuid
            MatchCollection matches = rxPlayer.Matches(contents);
            string matched = matches[0].Value;
            matched = matched.Replace("\"", "");
            matched = matched.Replace("'", "");
            string programUuid = matched.Split(",")[1];

            this.ProgramUuid = programUuid;
            return true;
        }

        /// <summary>
        /// Get all the assets (video) information (e.g. PlayPath)
        /// </summary>
        public async Task<bool> GetAssets()
        {
            if (this.ProgramUuid == null)
            {
                throw new Exception("ProgramUuid IS NOT SET - cannot retrieve ReferenceFile");

            }
            string playerUrl = String.Format(NHK_VIDEO_URL, this.ProgramUuid);
            var response = await NHKHttpClient.GetAsync(playerUrl);
            if (!response.IsSuccessStatusCode)
                // Try again for second time, NHK sometimes has issues
                response = await NHKHttpClient.GetAsync(playerUrl);

            var contents = await response.Content.ReadAsStringAsync();
            JObject video = new JObject();
            try
            {
                video = JObject.Parse(contents);
            }
            catch (JsonReaderException ex)
            {
                throw ex;
            }

            var referenceFile = (JObject)video["response"]["WsProgramResponse"]["program"]["asset"]["referenceFile"];
            var assets = (JArray)video["response"]["WsProgramResponse"]["program"]["asset"]["assetFiles"];


            // Collect all the information to create an M3U8 file
            var m3u8Elements = new List<string>();
            var bitrate = String.Empty;
            var aspect = String.Empty;
            var width = String.Empty;
            var height = String.Empty;
            var m3u8Path = String.Empty;
            var m3u8Element = String.Empty;
            var directory = String.Empty;
            var playPath = String.Empty;
            var hasReferenceFile = false;

            // Get the reference file (HD)
            directory = (string)referenceFile["rtmp"]["directory"] + "/";
            playPath = (string)referenceFile["rtmp"]["play_path"];
            playPath = playPath.Split('?')[0];

            // Check if reference file actually exists (sometimes it doesn't)
            var reference_url = String.Format("https://nhkw-mzvod.akamaized.net/www60/mz-nhk10/_definst_/{0}/chunklist.m3u8'", playPath);
            response = await NHKHttpClient.GetAsync(reference_url);
            if (response.IsSuccessStatusCode)
            {
                // Exists, add it,
                bitrate = (string)referenceFile["videoBitrate"];
                aspect = (string)referenceFile["aspectRatio"];
                width = (string)referenceFile["videoWidth"];
                height = (string)referenceFile["videoHeight"];
                m3u8Path = playPath.Replace(directory, "");
                m3u8Element = String.Format("{0}:{1}", bitrate, m3u8Path);
                m3u8Elements.Add(m3u8Element);
                this.PlayPath = playPath;
                this.Aspect = aspect;
                this.Width = width;
                this.Height = height;
                this.HasReferenceFile = true;
                hasReferenceFile = true;
                
            }

            int index = 0;
            foreach (var asset in assets)
            {
                directory = (string)asset["rtmp"]["directory"] + "/";
                playPath = (string)asset["rtmp"]["play_path"];
                playPath = playPath.Split('?')[0];
                m3u8Path = playPath.Replace(directory, "");
                bitrate = (string)asset["videoBitrate"];
                if (index == 0)
                {
                    // This is the first asset, if we do not have a reference file
                    // use the video information from this asset
                    if (!hasReferenceFile)
                    {
                        this.PlayPath = playPath;
                        this.Aspect = aspect;
                        this.Width = width;
                        this.Height = height;
                        this.HasReferenceFile = false;
                    }
                }
                m3u8Element = String.Format("{0}:{1}", bitrate, m3u8Path);
                m3u8Elements.Add(m3u8Element);
                index++;
            }

            this.M3u8Path = String.Join(",", m3u8Elements);
            return true;

        }

        /// <summary>
        /// Extracts the EpisodeDetakl information (e.g. Title)
        /// </summary>
        public async Task<bool> GetEpisodeDetail()
        {
            if (this.VodId == null)
            {
                throw new Exception("VodId IS NOT SET - cannot retrieve episode details");

            }
            string playerUrl = String.Format(NHK_GET_EPISODE_DETAIL_URL, this.VodId, NHK_API_KEY);
            var response = await NHKHttpClient.GetAsync(playerUrl);
            var contents = await response.Content.ReadAsStringAsync();

            JObject episodes = JObject.Parse(contents);

            if (episodes["data"]["episodes"].Count() == 1)
            {
                JObject episode = (JObject)episodes["data"]["episodes"][0];
                this.Title = (string)episode["title_clean"];
                this.Plot = (string)episode["description_clean"];
                this.PgmNo = (string)episode["pgm_no"];
                this.OnAir = (string)episode["onair"];
                this.Duration = (string)episode["movie_duration"];
                return true;
            }
            else
            {
                // Episode lookup does not work for all Playlist episodes
                // Set a dummy Title
                this.Title = "From Playlist";
                this.PgmNo = "0";
                this.Duration = "0";
                return true;
            }

        }


        /// <summary>
        /// Gets all the program details from NHK
        /// </summary>
        public async Task<bool> GetFromNHK()
        {
            bool success = false;

            if (await GetProgramUuidFromNHK())
            {
                if (await GetAssets())
                {
                    if (await GetEpisodeDetail())
                        return true;

                }
            }
            return success;
        }



        /// <summary>
        /// Laod the program from CosmosDB
        /// </summary>
        public async Task<bool> Load()
        {
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<VodProgram> vodProgramResponse = await Database.VodProgram.ReadItemAsync<VodProgram>(this.VodId, new PartitionKey(this.PartitionKey));
                this.OnAir = vodProgramResponse.Value.OnAir;
                this.PgmNo = vodProgramResponse.Value.PgmNo;
                this.PlayPath = vodProgramResponse.Value.PlayPath;
                this.M3u8Path = vodProgramResponse.Value.M3u8Path;
                this.Plot = vodProgramResponse.Value.Plot;
                this.ProgramUuid = vodProgramResponse.Value.ProgramUuid;
                this.Width = vodProgramResponse.Value.Width;
                this.Height = vodProgramResponse.Value.Height;
                this.Aspect = vodProgramResponse.Value.Aspect;
                this.Duration = vodProgramResponse.Value.Duration;
                this.Title = vodProgramResponse.Value.Title;
                this.LastUpdate = vodProgramResponse.Value.LastUpdate;
                return true;
            }
            catch (CosmosException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                return false;
            }
        }


        /// <summary>
        /// Save the entity if it does not exist yet‚
        /// </summary>
        public async Task<bool> Save()
        {
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<VodProgram> vodProgramResponse = await Database.VodProgram.ReadItemAsync<VodProgram>(this.VodId, new PartitionKey(this.PartitionKey));
                return true;
            }
            catch (CosmosException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                // Create an item in the container
                ItemResponse<VodProgram> vodProgramResponse = await Database.VodProgram.CreateItemAsync<VodProgram>(this, new PartitionKey(this.PartitionKey));
                return true;
            }
        }

        /// <summary>
        /// Upserts the entity
        /// </summary>
        public async Task<bool> Upsert()
        {
            // Upsert an item in the container
            ItemResponse<VodProgram> vodProgramResponse = await Database.VodProgram.UpsertItemAsync<VodProgram>(this, new PartitionKey(this.PartitionKey));
            return true;
        }

        /// <summary>
        /// Get the VodProgram - either from Cosmos or directly from NHK
        /// </summary>
        public async Task<bool> Get()
        {
            // Retrieve Program from CosmosDB
            if (await this.Load())
            {
                // Already in CosmosDB
                return true;
            }
            // Get from NHK
            else if (await this.GetFromNHK())
            {
                // Save it to CosmosDB
                return await this.Save();
            }
            else
            {
                // Couldn"t find it in CosmosDB and NHK
                return false;
            }

        }

    }
}