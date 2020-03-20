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

        [Fact]
        public void Test_NewProgram()
        {
            var vodProgram = new VodProgram("U1d2xiaDE6qTdDXmxFFeDzQgE4930P88");
            vodProgram.Title = "Test___TestItem";
            Assert.NotNull(vodProgram.LastUpdate);
        }

        [Fact]
        public async Task<VodProgram> Test_GetProgramUuid()
        {
            var vodProgram = new VodProgram("U1d2xiaDE6qTdDXmxFFeDzQgE4930P88");
            Assert.True(await vodProgram.GetProgramUuidFromNHK());
            Assert.NotEmpty(vodProgram.ProgramUuid);
            return vodProgram;
        }

        [Fact]
        public async void Test_GetReferenceFile()
        {
            var vodProgram = await Test_GetProgramUuid();
            Assert.True(await vodProgram.GetReferenceFile());
            Assert.NotEmpty(vodProgram.PlayPath);
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
            var vodProgram = new VodProgram("U1d2xiaDE6qTdDXmxFFeDzQgE4930P88");
            Assert.True(await vodProgram.GetFromNHK());
            Assert.NotEmpty(vodProgram.Title);
        }


        [Fact]
        public async void Test_Load()
        {
            var vodProgram = new VodProgram("U1d2xiaDE6qTdDXmxFFeDzQgE4930P88");
            Assert.True(await vodProgram.Load());
            Assert.NotEmpty(vodProgram.Title);
        }

        [Fact]
        public async void Test_Save()
        {
            var vodProgram = new VodProgram("U1d2xiaDE6qTdDXmxFFeDzQgE4930P88");
            Assert.True(await vodProgram.GetFromNHK());
            Assert.True(await vodProgram.Save());
            Assert.NotEmpty(vodProgram.Title);
        }

        [Fact]
        public async void Test_Exists()
        {
            var vodProgram = new VodProgram("U1d2xiaDE6qTdDXmxFFeDzQgE4930P88");
            Assert.True(await vodProgram.Exists());
        }

        [Fact]
        public async void Test_Get()
        {
            var vodProgram = new VodProgram("U1d2xiaDE6qTdDXmxFFeDzQgE4930P88");
            Assert.True(await vodProgram.Get());
            Assert.NotEmpty(vodProgram.Title);
        }



        /* 
                [Fact]
                public async void Test_GetReferenceFile()
                {
                    string vodId = "U1d2xiaDE6qTdDXmxFFeDzQgE4930P88";
                    var programUuid = await Program.GetProgramUuid(vodId);
                    var referenceFile = await Program.GetReferenceFile(programUuid);
                    Assert.NotEmpty(referenceFile);
                }

                [Fact]
                public async void Test_GetEpisodeDetail()
                {
                    string vodId = "U1d2xiaDE6qTdDXmxFFeDzQgE4930P88";
                    var episode = await Program.GetEpisodeDetail(vodId);
                    Assert.NotEmpty(episode);
                }

                [Fact]
                public async void Test_GetVodProgram()
                {
                    string vodId = "U1d2xiaDE6qTdDXmxFFeDzQgE4930P88";
                    var programEntitiy = await Program.GetVodProgram(vodId);
                    Assert.NotEmpty(programEntitiy.ProgramUuid);
                }

                [Fact]
                public async void Test_PopulateCache()
                {
                    int counter = await Program.PopulateCloudCache();
                    Console.WriteLine("Processed {0} cache entries", counter);
                    Assert.True(counter > 0);
                }

                [Fact]
                public async void Test_GetProgramList()
                {
                    string json = await Program.GetProgramList(100, logger);
                    Assert.NotEmpty(json);
                } */

    }
}

