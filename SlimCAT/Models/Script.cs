using System.Collections.Generic;

namespace SlimCAT
{
    public class Script
    {
        public Dictionary<string, string> correlationsDict = new Dictionary<string, string>();
        public List<Req> requestList;
        public int reqNumber = 0;
        public string scriptName = "";
    }

}
