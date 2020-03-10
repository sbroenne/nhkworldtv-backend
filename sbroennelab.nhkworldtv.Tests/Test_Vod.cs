using System;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace sbroennelab.nhkworldtv.Tests
{
    public class Test_Vod
    {
        [Fact]
        public async void Test_GetProgramUuid()
        {
            string vodId = "U1d2xiaDE6qTdDXmxFFeDzQgE4930P88";
            var programUuid = await sbroennelab.nhkworldtv.Vod.GetProgramUuid(vodId);
            Assert.NotEmpty(programUuid);
        }

        [Fact]
        public async void Test_GetVodProgram()
        {
            string vodId = "U1d2xiaDE6qTdDXmxFFeDzQgE4930P88";
            var programEntitiy = await sbroennelab.nhkworldtv.Vod.GetVodProgram(vodId);
            
            Assert.NotEmpty(programEntitiy.ProgramUuid);

        }
    }
}

