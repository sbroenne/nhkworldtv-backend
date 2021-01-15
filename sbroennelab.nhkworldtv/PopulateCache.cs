using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace sbroennelab.nhkworldtv
{

    public static class PopulateCache
    {

        [FunctionName("PopulateCache")]
        public static async void Run([TimerTrigger("0 0 */2 * * *")] TimerInfo myTimer, ILogger log)
        {
            int counter = await VodProgramList.PopulateCloudCache(log);
            log.LogInformation("Inserted {0} new cache entries from NHK", counter);
        }
    }
}
