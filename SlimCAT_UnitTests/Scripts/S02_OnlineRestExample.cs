using SlimCAT;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SlimCAT_UnitTests.Scripts
{
    internal class S02_OnlineRestExampleScript : Script
    {

        // http://dummy.restapiexample.com  is a live, online example rest service. 
        private static string urlPrefix = "http://dummy.restapiexample.com/api/v1";

        // ToDo: write some tests for
        // https://httpbin.org/#/HTTP_Methods - this one has a swagger
        // and http://test.k6.io/
        /* Try these also
            https://reqres.in/
            https://dummyapi.io/  -- only 500 requests per day
            https://medium.com/swlh/fake-rest-apis-that-we-can-use-to-build-prototypes-2a7946704726
            https://jsonplaceholder.typicode.com/
            https://documenter.getpostman.com/view/2062352/Szmb5eXv
        // Need Some asp.net
         */


        public S02_OnlineRestExampleScript()
        {
            // Usage Notes:
            // Register correlations by adding another element to this dictionary.
            // For each correlated value we must register the key name for that value.
            // This dictionary is inherted from class "Script". 
            // ToDo: This is not completely kosher that we build the dictionary in the constructor.
            correlationsDict.Add("empId", "Correlated Value Not Initialized");
            scriptName = GetType().Name;
        }


        public List<SlimCatReq> BuildRequestList()
        {
            slimCatRequestList = new List<SlimCatReq>();
            SlimCatReq get01 = new SlimCatReq()
            {
                reqNameForChart = "01_GetEmployees",
                uri = urlPrefix + "/employees",
                httpClientMethod = HttpMethod.Get,
                httpReqMsg = new HttpRequestMessage(HttpMethod.Get, urlPrefix + "/employees")
                {
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                },

                extractText_FromResponseBody = true,
                correlatedVariableKeyName = "empId",
                // Correlation Tips:
                // 1. Copy the resultBody into https://rubular.com/
                // 2. left and right boundary basic format: (?<=  <left str>    )(.*?)(?=  < rt string> )
                // 3. Use https://onlinestringtools.com/escape-string to escape what you build in Rubular. 
                // Loooking for: [{"id":1,"employee_name":"Tiger              
                regExPattern = "(?<={\"id\":)(.*?)(?=,\"employee_name)",
                thinkTimeInMs = 10000
            };


            SlimCatReq get02 = new SlimCatReq()
            {
                reqNameForChart = "02_GetEmployee",
               
                // Example of using correlated value from above
                httpReqMsg = new HttpRequestMessage(HttpMethod.Get, urlPrefix + "/employee/" + correlationsDict["empId"])
                {
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                },
               useExtractedUriText = true, // instructs SendRequest() to use a correlated value ToDo: I think this could be simplified.
               correlatedVariableKeyName = "empId",
            };


            SlimCatReq post01 = new SlimCatReq()
            {
                reqNameForChart = "03_AddEmployee",
                uri = urlPrefix + "/create",
               // body = "{\"name\":\"test\",\"salary\":\"123\",\"age\":\"23\"}",

                httpReqMsg = new HttpRequestMessage(HttpMethod.Post, urlPrefix + "/employees")
                {
                    Content = new StringContent("{\"name\":\"test\",\"salary\":\"123\",\"age\":\"23\"}", Encoding.UTF8, "application/json")
                }
            };

            SlimCatReq put01 = new SlimCatReq()
            {
                reqNameForChart = "04_UpdateEmployee",
                uri = urlPrefix + "/update/1",
                //body = "{\"name\":\"test\",\"salary\":\"126\",\"age\":\"23\"}",

                httpReqMsg = new HttpRequestMessage(HttpMethod.Put, urlPrefix + "/employees")
                {
                    Content = new StringContent("{\"name\":\"test\",\"salary\":\"126\",\"age\":\"23\"}", Encoding.UTF8, "application/json")
                }
            };

            SlimCatReq delete01 = new SlimCatReq()
            {
                reqNameForChart = "05_DeleteEmployee",
                uri = urlPrefix + "/delete/2",
                httpReqMsg = new HttpRequestMessage(HttpMethod.Delete, urlPrefix + "/employees")
                {
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                }
            };

            slimCatRequestList.Add(get01);
            slimCatRequestList.Add(get02);
            slimCatRequestList.Add(post01);
            slimCatRequestList.Add(put01);
            slimCatRequestList.Add(delete01);

            return slimCatRequestList;
        }
    }
}
