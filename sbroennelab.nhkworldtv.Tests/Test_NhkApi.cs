using Microsoft.Extensions.Logging;
using Xunit;

namespace sbroennelab.nhkworldtv.Tests
{
    /// <summary>
    /// Unit tests for all NhkApi related functionality
    /// </summary>

    [Collection("VOD collection")]
    public class Test_NhkApi
    {
        readonly VodFixture fixture;
        public Test_NhkApi(VodFixture fixture)
        {
            this.fixture = fixture;
        }

        private readonly ILogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);


        [Fact]
        public async void Test_GetNHKVodIdList()
        {
            var vodIdList = await NhkApi.GetVodIdList(logger);
            Assert.True(vodIdList.Count > 0);
        }


        [Fact]
        public async void Test_GetMediaInformationApiUrl()
        {
            var mediaInformationApiUrl = await NhkApi.GetMediaInformationApiUrl(fixture.VodId, logger);
            Assert.NotNull(mediaInformationApiUrl);
        }


        [Fact]
        public async void Test_GetStream()
        {
            var stream = await NhkApi.GetStream(fixture.VodId, logger);
            Assert.NotNull(stream);
            Assert.IsType<Stream>(stream);

        }

        [Fact]
        public async void Test_GetEpisode()
        {
            var episode = await NhkApi.GetEpisode(fixture.VodId, logger);
            Assert.NotNull(episode);
            Assert.IsType<Episode>(episode);
        }
    }
}

