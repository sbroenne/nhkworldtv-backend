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
    public static class TriggerGetVodProgramListV2
    {
        /// <summary>
        /// Get a list of minimal program meta data from the backend cache - Version 2
        /// </summary>
        /// <param name="[HttpTrigger(AuthorizationLevel.Function">API Key</param>
        /// <param name=""Program/v2/List/{maxItems}"">Number of programs to return</param>
        /// <returns>JSON with minimal metadata information</returns>
        [FunctionName("GetVodListV2")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Program/v2/List/{maxItems}")] HttpRequest req, string maxItems,
            ILogger log)
        {
            int items;

            // Check if the maxItems parameter is an int
            if (int.TryParse(maxItems, out items))
            {
                // Set sensible default value
                if (items < 0) items = 1;

                var jsonString = await VodProgramList.GetProgramList(items);
                if (jsonString != null)
                {
                    // return ProgramList
                    log.LogInformation("Executed GetProgramList({0}) - Size: {1}", items, jsonString.Length);
                    return new OkObjectResult(jsonString);
                }
                else
                {
                    // No content - this should not really happen!
                    log.LogError("No content returned for GetProgramList({0})", items);
                    return new NoContentResult();
                }
            }
            else
            {
                // Invalid maxItems parameter
                log.LogError("Invalid maxItems parameter provided for GetProgramList: {0}", maxItems);
                return new BadRequestResult();
            }
        }
    }
}