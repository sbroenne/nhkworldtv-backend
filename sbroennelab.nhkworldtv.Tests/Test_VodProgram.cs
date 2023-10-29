using Microsoft.Extensions.Logging;
using Xunit;

namespace sbroennelab.nhkworldtv.Tests
{
    /// <summary>
    /// Unit tests for all VodPrograms related functionality
    /// </summary>


    [Collection("VOD collection")]
    public class Test_VodProgram
    {
        private readonly ILogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
        readonly VodFixture fixture;
        public Test_VodProgram(VodFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Test_NewProgram()
        {
            var vodProgram = new VodProgram(fixture.VodId, logger)
            {
                Title = "Test___TestItem"
            };
            Assert.NotNull(vodProgram.PartitionKey);
        }


        [Fact]
        public async void Test_GetAssets()
        {
            var vodProgram = new VodProgram(fixture.VodId, logger)
            {
                VodId = fixture.VodId
            };
            Assert.True(await vodProgram.GetAsset());
            Assert.NotEmpty(vodProgram.Path720P);
        }

        [Fact]
        public async void Test_GetEpisodeDetail()
        {
            var vodProgram = new VodProgram(fixture.VodId, logger);
            Assert.True(await vodProgram.GetEpisodeDetail());
            Assert.NotEmpty(vodProgram.Title);
        }
        [Fact]
        public async void Test_GetFromNHK()
        {
            var vodProgram = new VodProgram(fixture.VodId, logger);
            Assert.True(await vodProgram.GetFromNHK());
            Assert.NotEmpty(vodProgram.Title);
        }


        [Fact]
        public async void Test_Load()
        {
            var vodProgram = new VodProgram(fixture.VodId, logger);
            Assert.True(await vodProgram.Load());
            Assert.NotEmpty(vodProgram.Title);
        }

        [Fact]
        public async void Test_Save()
        {
            var vodProgram = new VodProgram(fixture.VodId, logger);
            Assert.True(await vodProgram.GetFromNHK());
            Assert.True(await vodProgram.Save());
            Assert.NotEmpty(vodProgram.Title);
        }

        [Fact]
        public async void Test_Get()
        {
            var vodProgram = new VodProgram(fixture.VodId, logger);
            Assert.True(await vodProgram.Get());
            Assert.NotEmpty(vodProgram.Title);
        }

        [Fact]
        public async void Test_Delete()
        {
            var vodProgram = new VodProgram(fixture.VodId, logger);

            // Load the program and save it Cosmos DB
            Assert.True(await vodProgram.Get());
            // Delete it from Cosmos DB
            Assert.True(await vodProgram.Delete());
            // Load the program and save it Cosmos DB again so that it can be returned to the client
            Assert.True(await vodProgram.Get());
        }

    }
}

