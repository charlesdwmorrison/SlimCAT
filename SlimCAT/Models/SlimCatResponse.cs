using System;
using System.Net.Http;

namespace SlimCAT
{
    public class SlimCatResponse
    {
        public string reqUri = "";
        public string reqVerb = ""; // GET, POST, PUT, etc. 
        public string reqBody = "";
        public string reqUriParams = ""; // This would be for REST calls. 
        public string reqDescription = "";  // E.g., landing/takeoff
        public int reqIdx = 0; // position of request in the list of requests. Therefore the Id of the request.
        public string extractedText = "";
        public int responseIdForCurrentClient = 0;
        public int clientId = 0;

        public HttpResponseMessage httpResponseMessage; 

        public long responseId;
        public bool exceptionThrown;
        public string responseStatus = "{Response: 'Not Yet Received.'}";
        public string responseStatusCode = "-99"; // 500, 200 404, etc. -99 indicates not yet received.  This text is included in all HTTP responses, like OK=200; 
        public long responseTtlb = 0;
        public DateTime responseTimeReceived = new DateTime(1972, 1, 1, 0, 0, 0); //  We will use this to calculate throughput. 
        public bool responseExceptionThrown = false;
        public string responseExceptionMessage = "";
        public string responseBody = "";
        public DateTime requestTimeSent = new DateTime(1972, 1, 1, 0, 0, 0); //  We will use this to calculate throughput. 
        public DateTime testCompletionTime = new DateTime(1972, 1, 1, 0, 0, 0); // not stored every response. Is the timestamp of when a script iteration completes

    }
}
