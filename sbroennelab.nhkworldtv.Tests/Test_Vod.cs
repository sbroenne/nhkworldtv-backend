using System;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using sbroennelab.nhkworldtv;

namespace sbroennelab.nhkworldtv.Tests
{
    public class Test_Vod
    {
        [Fact]
        public async void Test_GetProgramUuid()
        {
            string vodId = "U1d2xiaDE6qTdDXmxFFeDzQgE4930P88";
            var programUuid = await Program.GetProgramUuid(vodId);
            Assert.NotEmpty(programUuid);
        }

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
            Assert.True(counter>0);
        }

        [Fact]
        public async void Test_GetProgramList()
        {
            string json = await Program.GetProgramList(100);
            Assert.NotEmpty(json);
        }
    }
}

