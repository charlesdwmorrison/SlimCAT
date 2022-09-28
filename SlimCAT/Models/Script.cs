using System.Collections.Generic;

namespace SlimCAT
{
    public class Script
    {
        public Dictionary<string, string> correlationsDict = new Dictionary<string, string>();
        public List<SlimCatReq> slimCatRequestList;
       // public List<HttpRequestMessage> httpReqList; 
        public int reqNumber = 0;
        public string scriptName = "";
        public Dictionary<string, string> header01; 
        public Dictionary<string, string> header02 = new Dictionary<string, string>();
        public Dictionary<string, string> header03 = new Dictionary<string, string>();
        public Dictionary<string, string> header04 = new Dictionary<string, string>();
        public Dictionary<string, string> header05 = new Dictionary<string, string>();
    }
}
