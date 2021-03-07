using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Xunit;

namespace sbroennelab.nhkworldtv.Tests
{
    /// <summary>
    /// Unit tests for all NhkApi related functionality
    /// </summary>

    [Collection("VOD collection")]
    public class Test_NhkApi
    {

        VodFixture fixture;
        public Test_NhkApi(VodFixture fixture)
        {
            this.fixture = fixture;
        }

        private ILogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);


        [Fact]
        public async void Test_GetNHKVodIdList()
        {
            var vodIdList = await NhkApi.GetVodIdList(logger);
            Assert.True(vodIdList.Count > 0);
        }

        [Fact]
        public async void Test_GetProgramUuid()
        {
            Assert.NotNull(await NhkApi.GetProgramUuid(fixture.VodId, logger));
        }


        [Fact]
        public async void Test_GetAssets()
        {
            var asset = await NhkApi.GetAsset(fixture.ProgramUuid, logger);
            Assert.NotNull(asset);
            Assert.IsType<JObject>(asset);

        }

        [Fact]
        public async void Test_GetEpisode()
        {
            var episode = (await NhkApi.GetEpisode(fixture.VodId, logger));
            Assert.NotNull(episode);
            Assert.IsType<JObject>(episode);
        }
    }
}

