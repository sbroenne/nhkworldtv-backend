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
        public async void Test_ReloadCloudCacheFromNHK()
        {
            var counter = await VodProgramList.ReloadCloudCacheFromNHK(logger);
            Assert.True(counter > 0);
        }
        [Fact]
        public async void Test_GetProgramList()
        {
            string json = await VodProgramList.GetProgramList(100);
            Assert.NotEmpty(json);
        }
    }
}

