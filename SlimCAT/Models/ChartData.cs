using SlimCAT.SignalR;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SlimCAT.Models
{
    public class ChartData
    {
        private static SignalRClient sgnlR;

        // holds a request ID number and the number of times that request has been executed.
        public static Dictionary<int, int> countPerUriDict;

        public ChartData(Script script)
        {
            sgnlR = new SignalRClient(); // ToDo: might need to make only one of these, not one per script. Singleton?
            countPerUriDict = new Dictionary<int, int>();
        }



        /// <summary>
        /// Counts the number of unique requests in a script.
        /// Constructs a label for the chart
        /// Calls SignalR to put label on the chart
        /// Calls SignalR hub to create a new line on the chart for each request in the script.
        /// ToDo:We need to add some logic here in the case SignalR is not started
        /// </summary>
        public void InitializeChartLines(Script script)
        {
            for (int i = countPerUriDict.Count; i < script.requestList.Count; i++)
            {
                countPerUriDict.Add(i, 0);
            }

            for (int i = 0; i <= script.requestList.Count - 1; i++)
            {
                //  reqCountPerUriDict.Add(i, new List<int>()); 
                // https://stackoverflow.com/questions/4630249/get-url-without-querystring/6015377 -- maybe some things with question mark here
                // https://stackoverflow.com/questions/1188096/truncating-query-string-returning-clean-url-c-sharp-asp-net/1188180#1188180
                // https://docs.microsoft.com/en-us/dotnet/api/system.uri?view=netframework-4.8
                // **** try the last one here: https://stackoverflow.com/questions/11529326/remove-last-segment-of-request-url
                // Also try HTML agility
                // ** regex for 6th segment: https://stackoverflow.com/questions/55579249/how-to-extract-a-specific-url-segment-with-regex-c-sharp
                // *** the URI template in the above is interesting too. 
                // Hostname becomes left boundary of the regex
                var uri = new Uri(script.requestList[i].uri);
                // var path = uri.MakeRelativeUri(uri);
                var path2 = uri.Segments;
                var path3 = uri.Host;
                var path4 = uri.IdnHost;
                var path5 = uri.Fragment;
                var path6 = uri.DnsSafeHost;
                var path7 = uri.GetLeftPart(UriPartial.Authority);
                var path8 = uri.GetLeftPart(UriPartial.Scheme);
                var path9 = uri.AbsolutePath;
                var path10 = uri.LocalPath; // These last two might be the answer
                // what we are looking for is is the REST method call.
                // it should be:
                // 1. get the host name
                // 2. Eliminate the host name
                // 3. 
                // 
                // 


                //setup labels for chart
                string fullUri = script.requestList[i].uri;
                int lastIndexOfBackSlash = fullUri.LastIndexOf('/');
                int secondLastIndex = lastIndexOfBackSlash > 0 ? fullUri.LastIndexOf('/', lastIndexOfBackSlash - 1) : -1;
                string chartLabelName = fullUri.Substring(secondLastIndex, fullUri.Length - secondLastIndex);
                Regex regex = new Regex("/");
                chartLabelName = regex.Replace(chartLabelName, "", 1);


                // ToDo: always make user load == orange; throughput = green, 1st response time blue, 2nd yellow or something
                Random rndColor = new Random();
                int red = rndColor.Next(0, 254);
                int grn = rndColor.Next(0, 254);
                int blu = rndColor.Next(0, 254);
                string rgbaStr = "rgba(" + red + "," + grn + "," + blu + ",1)";

                sgnlR.InitializeChartLine(chartLabelName, rgbaStr, 0);
            }

        }



    }
}
