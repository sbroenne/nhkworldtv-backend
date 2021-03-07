using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace sbroennelab.nhkworldtv
{
    /// <summary>
    /// Azure Function GetVodProgramListV2
    /// </summary>
    public static class GetVodProgramListV2
    {
        /// <summary>
        /// Get a list of minimal program meta data from the backend cache - Version 2
        /// </summary>
        /// <param name="[HttpTrigger(AuthorizationLevel.Function">API Key</param>
        /// <param name=""Program/v2/List/{maxItems}"">Number of programs to return</param>
        /// <returns>JSON with minimal information</returns>
        [FunctionName("GetVodListV2")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Program/v2/List/{maxItems}")] HttpRequest req, int maxItems,
            ILogger log)
        {
            string jsonString = await VodProgramList.GetProgramList(maxItems);

            return new OkObjectResult(jsonString);
        }
    }
}