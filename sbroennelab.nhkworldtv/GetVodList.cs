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

    public static class GetVodList
    {

        [FunctionName("GetVodList")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Program/List/{maxItems}")] HttpRequest req, int maxItems,
            ILogger log)
        {
            string jsonString = await EpisodeList.GetProgramList(maxItems);

            return new OkObjectResult(jsonString);
        }
    }
}
