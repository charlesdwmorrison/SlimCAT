using SlimCAT.SignalR;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace SlimCAT.Models
{
    public class ChartData
    {
        private static SignalRClient sgnlR;

        // holds a request ID number and the number of times that request has been executed.
        public static Dictionary<int, int> countPerUriDict;
        public static int testThroughputIdx;
        private static int lineCount = -1;

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

            for (int i = countPerUriDict.Count; i < script.slimCatRequestList.Count; i++)
            {
                countPerUriDict.Add(i, 0);
            }

            // Remember! This index is zero based, 
            // Initialize dictionary with throughput item and set to zero.
            testThroughputIdx = script.slimCatRequestList.Count;
            countPerUriDict.Add(testThroughputIdx, 0);

            for (int i = 0; i <= script.slimCatRequestList.Count - 1; i++)
            {
                string chartLabelName;
                if (script.slimCatRequestList[i].reqNameForChart == "Unnamed")
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
                    var uri = new Uri(script.slimCatRequestList[i].httpReqMsg.RequestUri.ToString());
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
                    // what we are looking for is the REST method call.
                    // it should be:
                    // 1. get the host name
                    // 2. Eliminate the host name
                    // 3. 
                    // 
                    // 


                    //setup labels for chart
                    string fullUri = uri.AbsoluteUri;
                    int lastIndexOfBackSlash = fullUri.LastIndexOf('/');
                    int secondLastIndex = lastIndexOfBackSlash > 0 ? fullUri.LastIndexOf('/', lastIndexOfBackSlash - 1) : -1;
                    chartLabelName = fullUri.Substring(secondLastIndex, fullUri.Length - secondLastIndex);
                    Regex regex = new Regex("/");
                    chartLabelName = regex.Replace(chartLabelName, "", 1);
                }
                else
                {
                    chartLabelName = script.slimCatRequestList[i].reqNameForChart;
                }

                //// ToDo: always make user load == orange; throughput = green, 1st response time blue, 2nd yellow or something
                //Random rndColor = new Random();
                //int red = rndColor.Next(0, 254);
                //int grn = rndColor.Next(0, 254);
                //int blu = rndColor.Next(0, 254);
                //string rgbaStr = "rgba(" + red + "," + grn + "," + blu + ",1)";
                List<string> colorLst = new List<string>()
                {
                  "176,196,222", // light steel blue
                    "30,144,255", // dodger blue
                    "255,165,0", // orange
                    "128,0,0",  // maroon
                    "0,128,0", // green
                    "255,255,0", // yellow
                    "128,128,128" // gray
                };

                // Good Colors
                /*
                 * https://www.rapidtables.com/web/color/RGB_Color.html
                 * Reserved Colors
                 * Red	#FF0000	(255,0,0) -- Errors
                 * Lime	#00FF00	(0,255,0) -- users
                 * Blue	#0000FF	(0,0,255) -- throughput
                 * 

                // Currently in use for lines
                -- light steel blue	#B0C4DE	(176,196,222)
                -- dodger blue	#1E90FF	(30,144,255)
                -- orange	#FFA500	(255,165,0)
                -- Maroon	#800000	(128,0,0)
                --   Green	#008000	(0,128,0)       
                --Yellow	#FFFF00	(255,255,0)
                -- Gray	#808080	(128,128,128)

                    Cyan / Aqua	#00FFFF	(0,255,255) 
                    Black	#000000	(0,0,0)                                
                    Olive	#808000	(128,128,0)
                    
                    Purple	#800080	(128,0,128)
                    Teal	#008080	(0,128,128)
                    Navy	#000080	(0,0,128)                
                    gold	#FFD700	(255,215,0)
                    dark green	#006400	(0,100,0)
                    light green	#90EE90	(144,238,144)                
                    blue violet	#8A2BE2	(138,43,226)
                    medium orchid	#BA55D3	(186,85,211)
                    hot pink	#FF69B4	(255,105,180) 
                Magenta / Fuchsia	#FF00FF	(255,0,255)  
                saddle brown	#8B4513	(139,69,19) -- looks like magenta
                    * 
                 */


                // countPerUriDict.Add(countPerUriDict.Count, 0); // "Count" will give next index position

                // ToDo: always make user load == orange; throughput = green, 1st response time blue, 2nd yellow or something
                //Random rndColor = new Random();
                //int red = rndColor.Next(0, 254);
                //int grn = rndColor.Next(0, 254);
                //int blu = rndColor.Next(0, 254);
                // string rgbaStr = "rgba(" + red + "," + grn + "," + blu + ",1)";

                string rgbaStr = "rgba(" + colorLst[Interlocked.Increment(ref lineCount)] + ", 1)";

                sgnlR.InitializeChartLine(chartLabelName, rgbaStr, 0);
            }

        }


        
        public void InitializeChartTestThroughputLine()
        {
            string rgbaStr = "rgba( 0,0,255,1)";
            sgnlR.InitializeChartLine("Test Iterations/sec", rgbaStr, 0);
        }

        public void MakeChartJsChart()
        {
            sgnlR.MakeChartJsChart();
        }
    }
}
