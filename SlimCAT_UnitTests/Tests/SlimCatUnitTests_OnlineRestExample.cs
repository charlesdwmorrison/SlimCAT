using NUnit.Framework;
using SlimCAT_UnitTests.Scripts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SlimCAT
{
    class S02_OnlineRestExample
    {

        static int perfViolationChkCount = 0;


        [Test]
        public async Task S02_OnlineRest_3_Users_10Iterations()
        {

            S02_OnlineRestExampleScript onlineRestExampleScript = new S02_OnlineRestExampleScript();
            List<SlimCatReq> requestList = onlineRestExampleScript.BuildRequestList();

            UserController uc = new UserController();
            await Task.Run(() => uc.AddUsersByRampUp(script: onlineRestExampleScript, stepDurationSecs: 5, maxUsers: 1, testDurationSecs: 300));

            PerfMetrics pvc = new PerfMetrics();
            Dictionary<string, double> perfMetrics = pvc.CalcualteAllMetrics(ResponseDb.conCurResponseDict);

            Assert.IsTrue(perfMetrics["totalTestDuration"] < 180, "Expected:Test Duration less than 2 minutes");
        }



    }
}
