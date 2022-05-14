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
        private static readonly string connectionString = System.Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING", EnvironmentVariableTarget.Process);

        private static readonly string containerName = "program-list-v2";

        public static async Task<Boolean> Create(int expiryHours, ILogger log)
        {

            string fileName = "cache.json";

            BlobServiceClient blobServiceClient = new(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Get the program cache
            var jsonContent = await VodProgramList.GetProgramList(3000);

            // Upload it
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
            await blobClient.SetHttpHeadersAsync(headers);

            log.LogInformation("Uploaded {0} blob - size: {1}", fileName, jsonContent.Length);

            return true;
        }
    }
}