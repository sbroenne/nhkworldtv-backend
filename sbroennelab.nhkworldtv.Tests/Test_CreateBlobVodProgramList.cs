using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace sbroennelab.nhkworldtv.Tests
{
    /// <summary>
    /// Tests for creating the JSON Blob on Azure Storage
    /// </summary>

    public class Test_JsonBlob
    {
        private readonly ILogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);


        [Fact]
        public async Task Test_CreateJsonBlob()
        {
            bool success = await JsonBlob.Create(4, logger);
            Assert.True(success);
        }
    }
}

