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
    public class Test_GetVodProgramList
    {
        private ILogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

        [Fact]
        public async void Test_PopulateCache()
        {
            var counter = await VodProgramList.PopulateCloudCache(logger);
            Assert.True(counter > 0);
        }

        [Fact]
        public async void Test_GetProgramList()
        {
            string json = await VodProgramList.GetProgramList(2000);
            Assert.NotEmpty(json);
        }

        [Fact]
        public async void Test_GetProgramListV2()
        {
            string json = await VodProgramList.GetProgramListV2(2000);
            Assert.NotEmpty(json);
        }
    }
}

