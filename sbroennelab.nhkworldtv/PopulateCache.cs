using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace sbroennelab.nhkworldtv
{

    public static class PopulateCache
    {

        [FunctionName("PopulateCache")]
        public static async void Run([TimerTrigger("0 0 */2 * * *")] TimerInfo myTimer, ILogger log)
        {
            var success = await VodProgramList.PopulateCloudCache(log);
        }
    }
}
