using System;
using Xunit;
using System.Threading.Tasks;
using sbroennelab.nhkworldtv;

namespace sbroennelab.nhkworldtv.Tests
{
    /// <summary>
    /// Unit tests for CosmosDB DDL operations - can also be used to verify that your
    /// dev environment works 
    /// If CosmsosDB already exists, the tests will still pass
    /// </summary>
    public class Test_DDL_VodProgram
    {
        [Fact]
        public async void Test_CreateDatabase()
        {
            Assert.True(await DDL_VodProgram.CreateDatabaseAsync());
        }

        [Fact]
        public async void Test_CreateVodProgramContainer()
        {
            Assert.True(await DDL_VodProgram.CreateVodProgramContainerAsync());
        }


    }
}

