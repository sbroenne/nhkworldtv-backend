using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace sbroennelab.nhkworldtv
{

    public static class TriggerUpdateBlob
    {

        [FunctionName("UpdateBlob")]
        public static async Task Run([TimerTrigger("0 5 */4 * * *")] TimerInfo myTimer, ILogger log)
        {
            var success = await JsonBlob.Create(4, log);
        }
    }
}
