using RestSharp;
using System.Collections.Generic;
using SlimCAT;


namespace SlimCAT
{
    public class S02_OnlineRestExampleScript : Script
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


        /// <summary>
        /// </summary>
        /// <returns></returns>
        public List<SlimCAT.Req> BuildRequestList()
        {
            requestList = new List<Req>()
              {
                new Req()
                {
                    uri = urlPrefix + "/employees",
                    method = Method.GET,
                    extractText = true,
                    nameForCorrelatedVariable = "empId",

                    // Correlation Tips:
                    // 1. Copy the resultBody into https://rubular.com/
                    // 2. left and right boundary basic format: (?<=  <left str>    )(.*?)(?=  < rt string> )
                    // 3. Use https://onlinestringtools.com/escape-string to escape what you build in Rubular. 
                    // Loooking for: [{"id":1,"employee_name":"Tiger              
                    regExPattern = "(?<={\"id\":)(.*?)(?=,\"employee_name)"
                },
                new Req()
                {
                    method = Method.GET,
                    // Example of using correlated value from above
                    //uri = urlPrefix + "/employee/1", // original
                    useExtractedText = true, // instructs SendRequest() to use a correlated value
                    nameForCorrelatedVariable = "empId",
                    uri = urlPrefix + "/employee/" + correlationsDict["empId"],
                    reqNameForChart = "employee"
                },
                new Req
                {
                    uri = urlPrefix + "/create",
                    method = Method.POST,
                    body = "{\"name\":\"test\",\"salary\":\"123\",\"age\":\"23\"}"
                },
                 new Req
                {
                    uri = urlPrefix + "/update/1",
                    method = Method.PUT,
                    body = "{\"name\":\"test\",\"salary\":\"123\",\"age\":\"23\"}"
                },
                new Req
                {
                    uri = urlPrefix + "/delete/2",
                    method = Method.DELETE,
                }
            };

            return requestList;
        }
    }
}
