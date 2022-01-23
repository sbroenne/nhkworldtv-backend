using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;


namespace sbroennelab.nhkworldtv
{
    public class JsonBlob
    /// Creates cache.json file that contains the program cache on Azure Storage
    {
        private static string connectionString = System.Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING", EnvironmentVariableTarget.Process);

        private static string containerName = "program-list-v2";

        public static async Task<Boolean> Create(ILogger log)
        {

            string fileName = "cache.json";

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Get the program cache
            var jsonContent = await VodProgramList.GetProgramList(2000);

            // Upload it
            await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(jsonContent)), overwrite: true);


            // Populate Cache runs every four hours
            // Set cache-control to four hours
            int expirySeconds = 4 * 60 * 60;
            string cacheControl = String.Format("public, max-age={0}", expirySeconds);

            // Create headers
            BlobHttpHeaders headers = new BlobHttpHeaders
            {
                // Set the MIME ContentType every time the properties 
                // are updated or the field will be cleared
                ContentType = "application/json",
                CacheControl = cacheControl
            };

            // Set the blob's properties
            await blobClient.SetHttpHeadersAsync(headers);

            return true;
        }
    }
}