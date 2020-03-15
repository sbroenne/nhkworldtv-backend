using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace sbroennelab.nhkworldtv
{
    public static class KeepWarm
    {
        [FunctionName("KeepWarm")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Keeping function warm and cuddly at: {DateTime.Now}");
        }
    }
}
