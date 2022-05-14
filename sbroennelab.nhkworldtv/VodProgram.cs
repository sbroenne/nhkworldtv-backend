using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace sbroennelab.nhkworldtv
{
    /// <summary>
    /// NHK Video On Demand
    /// </summary>
    public class VodProgram
    {
        // Create a static clients
        private static readonly HttpClient AkamaiHttpClient = new();

        private readonly ILogger log;

        /// <summary>Initializes a new instance of the <see cref="VodProgram" /> class.</summary>
        /// <param name="vodId">The Id of the program/episode</param>
        /// <param name="Logger">The logger.</param>
        public VodProgram(string vodId, ILogger Logger)
        {
            VodId = vodId;
            LastUpdate = DateTime.UtcNow;
            PartitionKey = "default";
            log = Logger;
        }

        /// <summary>Initializes a new instance of the <see cref="VodProgram" /> class. Only used when serialized to JSON.</summary>
        [JsonConstructor]
        public VodProgram()
        {
            // Only used when serialized to JSON
        }
        public string Aspect { get; set; }

        public string Duration { get; set; }

        public Boolean HasReferenceFile { get; set; }

        public string Height { get; set; }

        public DateTime LastUpdate { get; set; }

        public string OnAir { get; set; }

        public string PartitionKey { get; }

        public string Path1080P { get; set; }

        public string Path720P { get; set; }

        public string PgmNo { get; set; }

        public string Plot { get; set; }

        public string ProgramUuid { get; set; }

        public string Title { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string VodId { get; set; }
        public string Width { get; set; }
        /// <summary>
        /// Delete the entity
        /// </summary>
        public async Task<bool> Delete()
        {
            try
            {
                // Delete an item in the container
                ItemResponse<VodProgram> vodProgramResponse = await Database.VodProgram.DeleteItemAsync<VodProgram>(this.VodId, new PartitionKey(this.PartitionKey));
                log.LogDebug("Delete episode from CosmosDB : {0}", this.VodId);
                return true;
            }
            catch (CosmosException ex)
            {
                // Couldn't delete it, log it!
                log.LogError("Delete failure: {0} - Status code: {1}", this.VodId, ex.StatusCode);
                throw;
            }
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

        /// <summary>
        /// Get all the assets (video) information (e.g. PlayPath)
        /// </summary>
        public async Task<bool> GetAsset()
        {
            if (this.ProgramUuid == null)
            {
                log.LogError("ProgramUuid IS NOT SET - cannot retrieve ReferenceFile");
                return false;
            }
            var asset = await NhkApi.GetAsset(this.ProgramUuid, log);
            if (asset != null)
            {
                var referenceFile = (JObject)asset["referenceFile"];
                var assetFiles = (JArray)asset["assetFiles"];

                // Collect all the information to create an M3U8 file
                var bitrate = String.Empty;
                var aspect = String.Empty;
                var width = String.Empty;
                var height = String.Empty;
                var playPath = String.Empty;
                var hasReferenceFile = false;

                // Get the reference file (HD)
                playPath = (string)referenceFile["rtmp"]["play_path"];
                playPath = playPath.Split('?')[0];

                // Check if reference file actually exists (sometimes it doesn't)
                var reference_url = String.Format("https://nhkw-mzvod.akamaized.net/www60/mz-nhk10/_definst_/{0}/chunklist.m3u8", playPath);
                var response = await AkamaiHttpClient.GetAsync(reference_url);
                if (response.IsSuccessStatusCode)
                {
                    // Exists, add it and use the metadata
                    bitrate = (string)referenceFile["videoBitrate"];
                    aspect = (string)referenceFile["aspectRatio"];
                    width = (string)referenceFile["videoWidth"];
                    height = (string)referenceFile["videoHeight"];
                    this.Path1080P = reference_url;
                    hasReferenceFile = true;
                }

                // Get the 720P Version
                var asset720p = assetFiles[0];
                playPath = (string)asset720p["rtmp"]["play_path"];
                playPath = playPath.Split('?')[0];
                this.Path720P = String.Format("https://nhkw-mzvod.akamaized.net/www60/mz-nhk10/_definst_/{0}/chunklist.m3u8", playPath);

                // If we do not have a reference file
                // use the video information from 720P
                if (!hasReferenceFile)
                {
                    bitrate = (string)asset720p["videoBitrate"];
                    aspect = (string)asset720p["aspectRatio"];
                    width = (string)asset720p["videoWidth"];
                    height = (string)asset720p["videoHeight"];
                    hasReferenceFile = false;
                }

                this.Aspect = aspect;
                this.Width = width;
                this.Height = height;
                this.HasReferenceFile = hasReferenceFile;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts the Episode detail information (e.g. Title)
        /// </summary>
        public async Task<bool> GetEpisodeDetail()
        {
            if (this.VodId == null)
            {
                log.LogError("GetEpisodeDetail - VodId IS NOT SET - cannot retrieve episode details");
                return false;
            }

            var detail = await NhkApi.GetEpisode(this.VodId, log);

            if (detail != null)
            {
                this.Title = (string)detail["title_clean"];
                this.Plot = (string)detail["description_clean"];
                this.PgmNo = (string)detail["pgm_no"];
                this.OnAir = (string)detail["onair"];
                this.Duration = (string)detail["movie_duration"];
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
            this.ProgramUuid = await NhkApi.GetProgramUuid(this.VodId, log);

            if (this.ProgramUuid != null)
            {
                if (await GetAsset())
                {
                    if (await GetEpisodeDetail())
                        success = true;
                }
            }

            return success;
        }

        /// <summary>
        /// Load the program from CosmosDB
        /// </summary>
        public async Task<bool> Load()
        {
            try
            {
                // Read the item to see if it exists.
                VodProgram item = await Database.VodProgram.ReadItemAsync<VodProgram>(this.VodId, new PartitionKey(this.PartitionKey));
                this.OnAir = item.OnAir;
                this.PgmNo = item.PgmNo;
                this.Path1080P = item.Path1080P;
                this.Path720P = item.Path720P;
                this.HasReferenceFile = item.HasReferenceFile;
                this.Plot = item.Plot;
                this.ProgramUuid = item.ProgramUuid;
                this.Width = item.Width;
                this.Height = item.Height;
                this.Aspect = item.Aspect;
                this.Duration = item.Duration;
                this.Title = item.Title;
                this.LastUpdate = item.LastUpdate;
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                log.LogDebug("Episode does not exist in CosmosDB : {0}", this.VodId);
                return false;
            }
            catch (CosmosException ex)
            {
                // Unexpected exception
                log.LogError("Unexpected: Couldn't load episode from CosmosDB : {0} - Status code: {1}", this.VodId, ex.StatusCode);
                throw;
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
                log.LogDebug("Update episode in CosmosDB : {0}", this.VodId);
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container
                ItemResponse<VodProgram> vodProgramResponse = await Database.VodProgram.CreateItemAsync<VodProgram>(this, new PartitionKey(this.PartitionKey));
                log.LogDebug("Insert episode in CosmosDB : {0}", this.VodId);
                return true;
            }
        }
    }
}