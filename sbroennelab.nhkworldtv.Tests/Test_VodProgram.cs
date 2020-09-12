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
    public class Test_VodProgram
    {
        private ILogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
        private string vod_id = "nw_vod_v_en_2067_021_20200911133000_01_1599800682";

        [Fact]
        public void Test_NewProgram()
        {
            var vodProgram = new VodProgram(vod_id);
            vodProgram.Title = "Test___TestItem";
            Assert.NotNull(vodProgram.PartitionKey);
        }

        [Fact]
        public async Task<VodProgram> Test_GetProgramUuid()
        {
            var vodProgram = new VodProgram(vod_id);
            Assert.True(await vodProgram.GetProgramUuidFromNHK());
            Assert.NotEmpty(vodProgram.ProgramUuid);
            return vodProgram;
        }

        [Fact]
        public async void Test_GetAssets()
        {
            var vodProgram = await Test_GetProgramUuid();
            Assert.True(await vodProgram.GetAssets());
            Assert.NotEmpty(vodProgram.Path720P);
        }

        [Fact]
        public async void Test_GetEpisodeDetail()
        {
            var vodProgram = await Test_GetProgramUuid();
            Assert.True(await vodProgram.GetEpisodeDetail());
            Assert.NotEmpty(vodProgram.Title);
        }
        [Fact]
        public async void Test_GetFromNHK()
        {
            var vodProgram = new VodProgram(vod_id);
            Assert.True(await vodProgram.GetFromNHK());
            Assert.NotEmpty(vodProgram.Title);
        }


        [Fact]
        public async void Test_Load()
        {
            var vodProgram = new VodProgram(vod_id);
            Assert.True(await vodProgram.Load());
            Assert.NotEmpty(vodProgram.Title);
        }

        [Fact]
        public async void Test_Save()
        {
            var vodProgram = new VodProgram(vod_id);
            Assert.True(await vodProgram.GetFromNHK());
            Assert.True(await vodProgram.Save());
            Assert.NotEmpty(vodProgram.Title);
        }

        [Fact]
        public async void Test_Get()
        {
            var vodProgram = new VodProgram(vod_id);
            Assert.True(await vodProgram.Get());
            Assert.NotEmpty(vodProgram.Title);
        }
    }
}

