using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace sbroennelab.nhkworldtv
{

    public class TriggerPopulateCache
    {
        private readonly ILogger _logger;

        public TriggerPopulateCache(ILogger<TriggerPopulateCache> logger)
        {
            _logger = logger;
        }


        [Function("PopulateCache")]
        public async Task Run([TimerTrigger("0 0 */4 * * *")] TimerInfo myTimer)
        {
            var success = await VodProgramList.PopulateCloudCache(_logger);
        }
    }
}
