using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace sbroennelab.nhkworldtv.Tests
{
    public class VodFixture : IDisposable
    {
        /// Get a VodId from NHK to be used by all tests

        public VodFixture()
        {
            ILogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

            // Hardcoded Ids
            VodId = "nw_vod_v_en_2058_916_20220623101500_01_1655948082";
            ProgramUuid = "iv681ycz";
        }

        public void Dispose()
        {
            // Nothing to clean up

        }

        public string VodId { get; private set; }
        public string ProgramUuid { get; private set; }

    }

    [CollectionDefinition("VOD collection")]
    public class VodIdCollection : ICollectionFixture<VodFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
