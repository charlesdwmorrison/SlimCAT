using RestSharp;
using System;
using System.Collections.Generic;

namespace SlimCAT
{
    public class Req
    {
        public string uri;
        public string reqNameForChart; // normally, the chart will use the uri as the plot point. But in some cases we need to override the name for Chart.js (e.g., in the case of REST requests with changing data in the uri.)
        public string body;
        public Method method;
        public DateTime reqStartTime;

        public Dictionary<string, string> correlations;
        public string nameForCorrelatedVariable;
        public string correlatedValue;
        public bool extractText = false;
        public bool useExtractedText = false;

        public string leftBoundary; // the correlation boundary for the target in the *response*
        public string rightBoundary;
        public string regExPattern;

    }
}
