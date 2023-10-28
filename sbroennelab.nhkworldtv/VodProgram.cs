using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
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

        public bool HasReferenceFile { get; set; }

        public string Height { get; set; }

        public DateTime LastUpdate { get; set; }

        public string OnAir { get; set; }

        public string PartitionKey { get; }

        public string Path1080P { get; set; }

        public string Path720P { get; set; }

        public string PgmNo { get; set; }

        public string Plot { get; set; }

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
                ItemResponse<VodProgram> vodProgramResponse = await Database.VodProgram.DeleteItemAsync<VodProgram>(VodId, new PartitionKey(PartitionKey));
                log.LogDebug("Delete episode from CosmosDB : {0}", VodId);
                return true;
            }
            catch (CosmosException ex)
            {
                // Couldn't delete it, log it!
                log.LogError("Delete failure: {0} - Status code: {1}", VodId, ex.StatusCode);
                throw;
            }
        }

        /// <summary>
        /// Get the VodProgram - either from Cosmos or directly from NHK
        /// </summary>
        public async Task<bool> Get()
        {
            // Retrieve Program from CosmosDB
            if (await Load())
            {
                // Already in CosmosDB
                return true;
            }
            // Get from NHK
            else if (await GetFromNHK())
            {
                // Save it to CosmosDB
                return await Save();
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
            if (VodId == null)
            {
                log.LogError("vodId IS NOT SET - cannot retrieve Stream");
                return false;
            }
            var stream = await NhkApi.GetStream(VodId, log);
            if (stream != null)
            {
                Path720P = stream.meta[0].movie_url.mb_hd;
                Width = stream.meta[0].movie_definition.mb_hd.Split(":")[0];
                Height = stream.meta[0].movie_definition.mb_hd.Split(":")[1];
                Aspect = Math.Round(decimal.Parse(Width) / decimal.Parse(Height), 2).ToString(CultureInfo.InvariantCulture);
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
            if (VodId == null)
            {
                log.LogError("GetEpisodeDetail - VodId IS NOT SET - cannot retrieve episode details");
                return false;
            }

            var episode = await NhkApi.GetEpisode(VodId, log);

            if (episode != null)
            {
                Title = episode.title_clean;
                Plot = episode.description_clean;
                PgmNo = episode.pgm_no;
                OnAir = episode.onair.ToString();
                Duration = episode.movie_duration.ToString();
                return true;
            }
            else
            {
                // Episode lookup does not work for all Playlist episodes
                // Set a dummy Title
                Title = "From Playlist";
                PgmNo = "0";
                Duration = "0";
                return true;
            }
        }

        /// <summary>
        /// Gets all the program details from NHK
        /// </summary>
        public async Task<bool> GetFromNHK()
        {
            bool success = false;

            if (await GetAsset())
            {
                if (await GetEpisodeDetail())
                    success = true;
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
                VodProgram item = await Database.VodProgram.ReadItemAsync<VodProgram>(VodId, new PartitionKey(PartitionKey));
                OnAir = item.OnAir;
                PgmNo = item.PgmNo;
                Path1080P = item.Path1080P;
                Path720P = item.Path720P;
                Plot = item.Plot;
                Width = item.Width;
                Height = item.Height;
                Aspect = item.Aspect;
                Duration = item.Duration;
                Title = item.Title;
                LastUpdate = item.LastUpdate;
                if (item.Path1080P == null)
                    HasReferenceFile = false;
                else
                    HasReferenceFile = true;
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                log.LogDebug("Episode does not exist in CosmosDB : {0}", VodId);
                return false;
            }
            catch (CosmosException ex)
            {
                // Unexpected exception
                log.LogError("Unexpected: Couldn't load episode from CosmosDB : {0} - Status code: {1}", VodId, ex.StatusCode);
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
                ItemResponse<VodProgram> vodProgramResponse = await Database.VodProgram.ReadItemAsync<VodProgram>(VodId, new PartitionKey(PartitionKey));
                log.LogDebug("Update episode in CosmosDB : {0}", VodId);
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container
                ItemResponse<VodProgram> vodProgramResponse = await Database.VodProgram.CreateItemAsync<VodProgram>(this, new PartitionKey(PartitionKey));
                log.LogDebug("Insert episode in CosmosDB : {0}", VodId);
                return true;
            }
        }
    }
}