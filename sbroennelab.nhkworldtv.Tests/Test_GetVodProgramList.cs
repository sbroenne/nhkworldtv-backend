using Microsoft.Extensions.Logging;
using Xunit;

namespace sbroennelab.nhkworldtv.Tests
{
    /// <summary>
    /// Unit tests for all VodProgramList related functionality
    /// </summary>
    [Collection("VOD collection")]
    public class Test_GetVodProgramList
    {
        private readonly ILogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
        readonly VodFixture fixture;
        public Test_GetVodProgramList(VodFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async void Test_PopulateCache()
        {

            // Creating a dummy program that will be deleted so we can test that code path
            var vodProgram = new VodProgram("Unit Test Dummy", logger);
            Assert.True(await vodProgram.Save());
            vodProgram = new VodProgram(fixture.VodId, logger);
            // Delete an existing VodId that should be populated again
            Assert.True(await vodProgram.Delete());
            var success = await VodProgramList.PopulateCloudCache(logger);
            Assert.True(success);
            // Load the program and save it Cosmos DB again so that it can be returned to the client
            Assert.True(await vodProgram.Get());
        }


        [Fact]
        public async void Test_GetProgramList()
        {
            string json = await VodProgramList.GetProgramList(2000);
            Assert.NotEmpty(json);
        }
    }
}

