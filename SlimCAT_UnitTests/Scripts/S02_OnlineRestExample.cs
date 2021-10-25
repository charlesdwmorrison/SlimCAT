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

        Public Soap API
        https://documenter.getpostman.com/view/8854915/Szf26WHn

        to find more search for "public  ___ API"
         */


        public S02_OnlineRestExampleScript()
        {
            // Correlation Usage Notes:
            // Register correlations by adding another element to this dictionary.
            // For each correlated value we must register the key name for that value.
            // This dictionary is inherted from class "Script". 
            // ToDo: This is not completely kosher that we build the dictionary in the constructor.
            correlationsDict.Add("empId", "Correlated Value Not Initialized");
            scriptName = GetType().Name;

            // Headers
            header01 = new Dictionary<string, string>{{ "Content-Type", "text/xml; charset=utf-8" }};


        }


        /// <summary>
        /// Think of this as a "template".
        /// </summary>
        /// <returns></returns>
        public List<SlimCAT.SlimCatReq> BuildRequestList()
        {
            slimCatRequestList = new List<SlimCatReq>()
              {
                new SlimCatReq()
                {
                    uri = urlPrefix + "/employees",
                    restSharpMethod = Method.GET,
                    extractText_FromResponseBody = true,
                    nameForCorrelatedVariable = "empId",

                    // Correlation Tips:
                    // 1. Copy the resultBody into https://rubular.com/
                    // 2. left and right boundary basic format: (?<=  <left str>    )(.*?)(?=  < rt string> )
                    // 3. Use https://onlinestringtools.com/escape-string to escape what you build in Rubular. 
                    // Loooking for: [{"id":1,"employee_name":"Tiger              
                    regExPattern = "(?<={\"id\":)(.*?)(?=,\"employee_name)"
                },
                new SlimCatReq()
                {
                    restSharpMethod = Method.GET,
                    // Example of using correlated value from above
                    //uri = urlPrefix + "/employee/1", // original
                    useExtractedUriText = true, // instructs SendRequest() to use a correlated value
                    nameForCorrelatedVariable = "empId",
                    uri = urlPrefix + "/employee/" + correlationsDict["empId"],
                    reqNameForChart = "employee"
                },
                new SlimCatReq
                {
                    uri = urlPrefix + "/create",
                    restSharpMethod = Method.POST,
                    body = "{\"name\":\"test\",\"salary\":\"123\",\"age\":\"23\"}"
                },
                 new SlimCatReq
                {
                    uri = urlPrefix + "/update/1",
                    restSharpMethod = Method.PUT,
                    body = "{\"name\":\"test\",\"salary\":\"123\",\"age\":\"23\"}"
                },
                new SlimCatReq
                {
                    uri = urlPrefix + "/delete/2",
                    restSharpMethod = Method.DELETE,
                }
            };

            return slimCatRequestList;
        }
    }
}
