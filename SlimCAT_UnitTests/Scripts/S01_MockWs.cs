//using SlimCAT;
//using RestSharp;
//using System.Collections.Generic;
//using System.Threading;

//namespace ADP_DAP_LoadTest
//{
//    public class S01_MockWS
//    {

//        private static string urlPrefix = "http://localhost:8080";
//        private SendRequests sr;

//        public S01_MockWS(int thinkTime)
//        {
//            sr = new SendRequests(thinkTime);

//        }

//        // correlation variables, initialized value is empty. Will fill after getting response
//        //public string empId = "";
//        public static Dictionary<string, string> empId = new Dictionary<string, string>
//        {
//            {"empId"," "}
//        };

//        /// <summary>
//        /// This work load is the action that each thread will perform.
//        /// </summary>
//        public static void MockWorkloadDefinition()
//        {
//            S01_MockWS S01MWS = new S01_MockWS(0);
//            for (int i = 1; i <= 10000; i++)
//            {
//                S01MWS.Req00();
//            }
//        }

//        public static void ClickPathAndIterations()
//        {
//            S01_MockWS S01MWS = new S01_MockWS(0);
//            for (int i = 1; i <= 10000; i++)
//            {
//                S01MWS.Req00();
//            }
//        }

//        public void Req00()
//        {
//            Req req = new Req
//            {
//                uri = urlPrefix + "/test",
//                method = Method.GET,

//                extractText = empId,
//                // looking for {"id":"10","employee_name
//                // This means "find a string from the *response* to this request
//                // This is just like LoadRunner correlation. 
//                leftBoundary = "{\"id\":\"",
//                rightBoundary = "\",\"employee_name",
//            };
//             sr.SendRequest(req);
//        }


//        public void Pacing(int pacingTimeinMs)
//        {
//            Thread.Sleep(pacingTimeinMs);
//        }


//    }
//}
