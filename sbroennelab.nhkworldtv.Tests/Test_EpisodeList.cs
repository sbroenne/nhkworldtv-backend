using System;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using sbroennelab.nhkworldtv;

namespace sbroennelab.nhkworldtv.Tests
{
    /// <summary>
    /// Unit tests for all VodPrograms related functionality
    /// </summary>
    public class Test_EpisodeList
    {
        private ILogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

        [Fact]
        public async void Test_PopulateCache()
        {
            var counter = await EpisodeList.PopulateCloudCache();
            Assert.True(counter > 0);
        }
        [Fact]
        public async void Test_GetProgramList()
        {
            string json = await EpisodeList.GetProgramList(100);
            Assert.NotEmpty(json);
        }
    }
}

