using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace sbroennelab.nhkworldtv
{
    /// <summary>
    /// Azure Function GetVodProgramList
    /// </summary>
    public static class GetVodProgramList
    {
        /// <summary>
        /// Get a list of program meta data from the backend cache
        /// </summary>
        /// <param name="[HttpTrigger(AuthorizationLevel.Function">API Key</param>
        /// <param name=""Program/List/{maxItems}"">Number of prgrams to return</param>
        /// <returns>JSON with key attributes like PlayPath, Width, etc.</returns>
        [FunctionName("GetVodList")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Program/List/{maxItems}")] HttpRequest req, int maxItems,
            ILogger log)
        {
            string jsonString = await VodProgramList.GetProgramList(maxItems);

            return new OkObjectResult(jsonString);
        }
    }
}
