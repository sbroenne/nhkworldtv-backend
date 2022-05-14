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
            VodId = "nw_vod_v_en_2061_516_20220514013000_01_1652461456";
            ProgramUuid = "my4mrx00";

            /*   // This is bad unit test practice but I need to get a VodId from NHK to set-up the tests since they are not static and expire
              var taskVodIdList = Task.Run(() => NhkApi.GetVodIdList(logger));
              var vodIdList = taskVodIdList.Result;
              VodId = vodIdList[0];
              var taskProgramUuid = Task.Run(() => NhkApi.GetProgramUuid(this.VodId));
              ProgramUuid = taskProgramUuid.Result;
          */
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
