using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace sbroennelab.nhkworldtv
{

    public static class Vod
    {

        [FunctionName("GetVodProgram")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Program/{vodId}")] HttpRequest req, string vodId,
            ILogger log)
        {
            var vodProgram = await Program.GetVodProgram(vodId);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString;
            jsonString = JsonSerializer.Serialize(vodProgram, options);

            return new OkObjectResult(jsonString);
        }
    }
}
