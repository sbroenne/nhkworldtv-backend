using System;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace sbroennelab.nhkworldtv.Tests
{
    public class Test_GetVideoUrl
    {
        [Fact]
        public async void Test_GetProgramUuid()
        {
            string vidId = "U1d2xiaDE6qTdDXmxFFeDzQgE4930P88";
            var programUuid =  await sbroennelab.nhkworldtv.GetVideoUrl.GetProgramUuid(vidId);
            Assert.NotEmpty(programUuid);
        }
    }
}
