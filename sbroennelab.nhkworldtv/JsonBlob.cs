using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace sbroennelab.nhkworldtv
{
    public class JsonBlob
    /// Creates cache.json file that contains the program cache on Azure Storage
    {
        // Construct the blob container endpoint from the arguments.
        private static readonly string containerEndpoint = string.Format("https://{0}.blob.core.windows.net/{1}",
                                                    Environment.GetEnvironmentVariable("BLOB_NAME", EnvironmentVariableTarget.Process),
                                                "program-list-v2");

        // Get a credential and create a service client object for the blob container.
        private static readonly BlobContainerClient containerClient = new BlobContainerClient(new Uri(containerEndpoint),
                                                                        new DefaultAzureCredential());


        public static async Task<Boolean> Create(int expiryHours, ILogger log)
        {

            string fileName = "cache.json";
            log.LogInformation($"Creating cache file {fileName} file on storage account {containerEndpoint}");
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Get the program cache
            log.LogInformation("Getting content from CosmosDB");
            var jsonContent = await VodProgramList.GetProgramList(3000);

            // Upload it
            log.LogInformation("Uploading file");
            await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(jsonContent)), overwrite: true);

            // Populate Cache runs every four hours
            // Set cache-control to four hours
            int expirySeconds = expiryHours * 60 * 60;
            string cacheControl = $"max-age={expirySeconds}";

            // Create headers
            BlobHttpHeaders headers = new()
            {
                // Set the MIME ContentType every time the properties 
                // are updated or the field will be cleared
                ContentType = "application/json",
                CacheControl = cacheControl
            };

            // Set the blob's properties
            log.LogInformation("Updating headers");
            await blobClient.SetHttpHeadersAsync(headers);

            log.LogInformation($"Uploaded {fileName} blob - size: {jsonContent.Length}");

            return true;
        }
    }
}