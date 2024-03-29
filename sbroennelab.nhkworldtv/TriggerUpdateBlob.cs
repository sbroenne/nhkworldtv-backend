using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace sbroennelab.nhkworldtv
{

    public class TriggerUpdateBlob
    {
        private readonly ILogger _logger;

        public TriggerUpdateBlob(ILogger<TriggerUpdateBlob> logger)
        {
            _logger = logger;
        }


        [Function("UpdateBlob")]
        public async Task Run([TimerTrigger("0 5 */4 * * *")] TimerInfo myTimer)
        {
            var success = await JsonBlob.Create(4, _logger);
        }
    }
}
