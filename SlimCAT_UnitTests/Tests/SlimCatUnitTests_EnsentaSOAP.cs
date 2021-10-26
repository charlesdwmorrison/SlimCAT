using NUnit.Framework;
using SlimCAT_UnitTests.Scripts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlimCAT
{
    class S03_EnsentaSOAPExample
    {

        static int perfViolationChkCount = 0;


        [Test]
        public async Task S03_EnsentaSOAP_10_Users_10Iterations()
        {
            //ToDo: put build request list in contructor of other tests.            
            //S03_Ensenta_Mobile ensentaMobile = new S03_Ensenta_Mobile(); 

            UserController usrCtrlHttp = new UserController(); 
           // await Task.Run(() => usrCtrlHttp.AddUsersByRampUp(script: ensentaMobile, stepDurationSecs: 5, maxUsers: 300, testDurationSecs: 300));

            PerfMetrics pvc = new PerfMetrics();
            Dictionary<string, double> perfMetrics = pvc.CalcualteAllMetrics(ResponseDb.conCurResponseDict);

            Assert.IsTrue(perfMetrics["avgResponseTime"] < 6, "Expected:No request greater than 6 seconds.");
        }



        [Test]
        public async Task S04_PublicSOAPExample()
        {
            S04_PublicSOAPExample numberToWords = new S04_PublicSOAPExample();         

            UserController usrCtrlHttp = new UserController();
            await Task.Run(() => usrCtrlHttp.AddUsersByRampUp(script: numberToWords, stepDurationSecs: 1, maxUsers: 1, testDurationSecs: 300));

            PerfMetrics pvc = new PerfMetrics();
            Dictionary<string, double> perfMetrics = pvc.CalcualteAllMetrics(ResponseDb.conCurResponseDict);

            Assert.IsTrue(perfMetrics["totalTestDuration"] < 180, "Expected:Test Duration less than 2 minutes");
        }


    }
}
