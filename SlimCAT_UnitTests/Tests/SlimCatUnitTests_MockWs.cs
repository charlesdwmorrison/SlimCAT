using APD_DAP.UnitTests.MockWebServer;
using NUnit.Framework;
using System.IO;
using System.Net;

namespace SlimCAT_UnitTests_MockWebServer
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            // Run the web server
            int wsCount = 0;
            if (wsCount == 0)
            {
                WebServer ws = WebServer.Instance;
                ws.Run();
            }

        }




        //[Test]
        //public async Task Mock_MultiUser_100Users()
        //{
        //    UserController uc = new UserController();
        //    await Task.Run(() => uc.RampUpUsers(S01_MockWS.ClickPathAndIterations, newUserEvery: 1000, maxUsers: 100, testDurationSecs: 20));

        //    PerformanceViolationChecker pvc = new PerformanceViolationChecker();

        //    var perfMetrics = pvc.CalcualteAllMetrics(SendRequests.conCurResponseDict);
        //}


        //[Test]
        //public async Task Mock_MultiUser_100Users_Alt()
        //{
        //    UserController uc = new UserController();
        //    await Task.Run(() => uc.RampUpUsers(S01_MockWS.ClickPathAndIterations, newUserEvery: 2000, maxUsers: 100, testDurationSecs: 200));

        //    PerformanceViolationChecker pvc = new PerformanceViolationChecker();

        //    var perfMetrics = pvc.CalcualteAllMetrics(SendRequests.conCurResponseDict);
        //}



        /// <summary>
        /// This is just to test if our mocked webserver is working. Not a PhonenixRunner unit tset. 
        /// </summary>
        [Test, Category("UnitTests")]
        public void Test_MockWebServer_Get()
        {
            // web server must be running. 

            string urlAddress = "http://localhost:8080/test/";
            string responseData = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, System.Text.Encoding.GetEncoding(response.CharacterSet));
                }
                responseData = readStream.ReadToEnd();
                response.Close();
                readStream.Close();
            }

            Assert.IsTrue(responseData.Contains("My web page"), $"The words 'My web page' should appear in the response. Expected: 'My web page' ");
        }



    }
}