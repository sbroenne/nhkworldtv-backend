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

    public static class GetVodProgram
    {

        [FunctionName("GetVodProgram")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Program/{vodId}")] HttpRequest req, string vodId,
            ILogger log)
        {
            var vodProgram = new VodProgram(vodId);
            bool success = await vodProgram.Get();
            if (success)
            {
                string jsonString = JsonSerializer.Serialize(vodProgram);
                return new OkObjectResult(jsonString);
            }
            else
            {
                return new NotFoundResult();
            }
        }
    }
}
