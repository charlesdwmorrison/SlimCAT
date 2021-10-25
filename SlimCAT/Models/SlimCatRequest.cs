using System;
using System.Collections.Generic;

namespace SlimCAT
{
    public class SlimCatReq
    {
        public string uri;
        public string reqNameForChart = "Unnamed"; // normally, the chart will use the uri as the plot point. But in some cases we need to override the name for Chart.js (e.g., in the case of REST requests with changing data in the uri.)
        public string body;
        public RestSharp.Method restSharpMethod;
        public System.Net.Http.HttpMethod httpClientMethod;
        public System.Net.Http.HttpRequestMessage httpReqMsg;
        public int requestId = 0;
        public int uriIdx = 0; // position of request within it's script.
        public int taskOrThreadId = 0; // Id of the task which is sending the request

        public bool clearHeaders = false;

        public DateTime reqStartTime;

        public Dictionary<string, string> correlations;
        public string nameForCorrelatedVariable;
        public string correlatedValue;
        public bool extractText_FromResponseBody = false;
        public bool useExtractedUriText = false;
        public bool useExtractedBodyText = false;
        public bool useExtractedHeaderText = false;

        public string leftBoundary; // the correlation boundary for the target in the *response*
        public string rightBoundary;
        public string regExPattern;

    }
}
