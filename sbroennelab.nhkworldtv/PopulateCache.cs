using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace sbroennelab.nhkworldtv
{

    public static class PopulateCache
    {

        [FunctionName("PopulateCache")]
        public static async Task Run([TimerTrigger("0 0 */2 * * *")] TimerInfo myTimer, ILogger log)
        {
            var success = await VodProgramList.PopulateCloudCache(log);
        }
    }
}
