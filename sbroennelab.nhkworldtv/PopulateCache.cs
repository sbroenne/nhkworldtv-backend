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

    public static class PopulateCache
    {

        [FunctionName("PopulateCache")]
        public static void Run([TimerTrigger("0 5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            bool success = await Program.PopulateCloudCache();
            return new OkObjectResult(success);
        }
    }
}
