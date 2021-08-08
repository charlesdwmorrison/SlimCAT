using RestSharp;
using SlimCAT.Models;
using SlimCAT.SignalR;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;


// ToDos:
// C# Issues
// ---------------
// Fix the issue with the REST sample correlation on 2nd request. Why not correlating? 
// Example of adding data - borrow LINQ method from PhoenixRunner . Now see Jack Henry project
// Make hub start with a test? 
// Put in "LogEverywhere" code 
// Issue with the chart label not really being unique. HTML Agility?
// Create samples with different script types, ASP.Net, SOAP
// Fix Assert in Online reset test. Total test time should not be measured.
// push the throughput metric to chart(?) It could be the JavaScript could calcualate this. 
// More unit tests, better mock web server
// this is a completely ligitimate way of launching threads. Is it better?: https://stackoverflow.com/questions/12710732/visual-studio-load-test-to-simulate-many-users-in-data-driven-fashion
// https://github.com/SoftCircuits/CsvParser
// -- http status code in SignalR console
// need a sample WCF proxy with Channel factory. This could be the _AX_Proxy_Initialize.cs class
// Import a har file. There is a library for this
// Validation rules
// Warm up 



// Docs
// Make an animated GIF. I have tool to do this. ShareX does this the best. Free
// Docs: How to combat the "We have been challanged on tests. We have to use standard load tools." Issue.
//     -- needs some notes about what is standard. 
//  -- Part of the problem here is lack of analysis tools. They have to run the test 2 and 3 times and compare if there was a difference.
// A big issue is number of connections. Especially in SOAP connections. Overloading the server under test with proxy connections
// My scripts look almost exactly like WCAT scripts https://blogs.iis.net/thomad/using-the-wcat-fiddler-extension-for-web-server-performance-tests
//https://www.stresstimulus.com/load-testing-tool/how-it-works

//https://stackoverflow.com/questions/5028878/asp-net-iis7-obtaining-cpu-usage-per-request 
// https://stackoverflow.com/questions/16101483/can-i-send-multiple-requests-at-one-time-using-fiddler
// https://stackoverflow.com/questions/3804386/web-capacity-analysis-tool-wcat-tutorial
// http://blogs.lessthandot.com/index.php/enterprisedev/application-lifecycle-management/implementing-wcat-for-load-testing/

// https://www.iis.net/downloads/community/2007/05/wcat-63-x86
// Need reference to Fiddler and also POSTMan







// Chart Issues
// ----------------------------------------
// Add table on chart to show average, 90% etc. (Throughput)
// Remove buttons on Chart (except maybe in a Div)
// Make is so chart always starts with blue, green, yellow.
// Have Chart automatically refresh itself (Redraw chart)
// Remove extra parameter in JavaScript -- the line ID



namespace SlimCAT
{
    public class SendRequests
    {

        public static int numRestClients = 0;
        public int responseIdForCurrentClient = 0;  // not static. Count is unique per client instance.
        public int _clientId;
        public static int responseIdForConcurrentDict = -1;
        public static int responseIdForLog = 0;
        private static DateTime testStartTime;
        private SignalRClient sgnlR = new SignalRClient(); // might need to make only one of these, not one per script. Singleton?
        private Stopwatch stopwatch;
        private RestClient client;
        private LogWriter writer;
        private int thinkTimeBetweenRequests; // this IS think time between requests, not between script iterations 

        /// <summary>
        /// Constructor ensures that a new client is created for every user (thread).
        /// Note, therefore, that multiple threads do NOT pass through this class's methods. Each thread has its own instance of this class.
        /// Each thread executes its requests one at a time (synchronously).
        /// </summary>
        public SendRequests(int clientId, int _thinkTimeBetweenRequests)
        {
            Interlocked.Increment(ref numRestClients); // ToDo: This might be better as a class, "Clients".
            client = new RestClient();
            writer = LogWriter.Instance;
            thinkTimeBetweenRequests = _thinkTimeBetweenRequests;
            _clientId = clientId;
            stopwatch = new Stopwatch(); // note that there is only 1 stopwatch per SendRequest object. See note below about debugging.

        }


        /// <summary>
        /// ToDo: add pacing and think time parameters here.
        /// One instance of this class will process a script sequentially for as long as the test case designated.
        /// This method sends only 1 request. It does not take an array, just one request from the script at at time.
        /// This is a *request-level* handler, not a script level handler.
        /// The script object is only used here to get the script name.
        /// ToDo: It might be more clear if we change the first paramter to a string, to only show the name. 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="req">req is the name for the static "request instructions" in the script.</param>
        public void SendRequest(Script script, Req req)
        {

            // position of request in the list of requests. Therefore, this is the Id of the request.     
            int uriIdx = script.requestList.IndexOf(req);

            var method = req.method;
            var uri = req.uri;
            var request = new RestRequest(uri, method);

            req.reqStartTime = DateTime.Now;

            if (client.CookieContainer == null)
            {
                client.CookieContainer = new System.Net.CookieContainer();
            }

            if (req.method == Method.POST)
            {
                request.AddJsonBody(req.body);
            }

            // modifiy request based on  correlated value
            if (req.useExtractedText == true)
            {
                string keyName = req.nameForCorrelatedVariable;
                req.uri = req.uri.Replace("Corrolated Value Not Initialized", script.correlationsDict[keyName]);
            }

            Response response = new Response();

            var dtNow = DateTime.Now;
            response.requestTimeSent = dtNow;

            if (responseIdForConcurrentDict == -1)
            {
                testStartTime = dtNow;
            }

            IRestResponse result = null;

            // ToDo: Verify this with Fiddler.
            // If executed in debug mode and a breakpoint with 2 threads, this does not yield accurate results because
            //  the breakpoint is stopping execution of both threads, one in the middle of executing, after the stopwatch has started.
            // Possibility: Starting and stopping the timer by means of an event might be better. 
            stopwatch.Restart();
            try
            {
                result = client.Execute(request, request.Method);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                response.responseExceptionThrown = true;
                response.responseExceptionMessage = msg;
            }

            response.reqUri = req.uri;
            response.reqBody = req.body;
            // response.reqVerb = req.method;
            // response.reqDescription = req.body;
            // response.requestTimeSent = req.body;
            // response.reqUriParams = req.body;


            response.responseTtlb = stopwatch.ElapsedMilliseconds;
            response.clientId = _clientId;
            response.responseId = responseIdForConcurrentDict;
            response.responseTimeReceived = DateTime.Now;
            response.responseStatus = "Finished";
            response.responseExceptionMessage = result.ErrorMessage;
            response.responseIdForCurrentClient = responseIdForCurrentClient++;
            response.responseStatsCode = result.StatusCode.ToString();
            response.responseBody = result.Content;

            ResponseDb.conCurResponseDict.TryAdd(Interlocked.Increment(ref responseIdForConcurrentDict), response);

            Debug.WriteLine($"Messages = {req.method}");
            double responseTimeInSec = response.responseTtlb / 1000.0;


            // Here is what we need to put a point on a line:
            // 1. Which line does this belong to? the "uriType" or position withing the request list so that we have one line per request type. We need this in order to determine which line to put the data point to.
            // 2. What position on that line?
            // 3. What is the value to put on that line? (TTLB)
            sgnlR.AddData(uriIdx, ChartData.countPerUriDict[uriIdx], responseTimeInSec);
            ChartData.countPerUriDict[uriIdx] += 1;

            TimeSpan duration;
            duration = DateTime.Now - testStartTime;
            double throughPut = Interlocked.Increment(ref responseIdForLog) / duration.TotalSeconds;

            //if (responseId > 15)
            //{
            //    // we can get the time embeded in the request
            //    // the idea here is to wait until we have a few requests.
            //    duration = DateTime.Now - conCurResponseDict[responseId - 10].responseTimeReceived;
            //    throughPut = 10 / duration.TotalSeconds;
            //}


            if (responseIdForLog % 25 == 0 || responseIdForLog == 1)
            {
                writer.WriteToLog(" TtlResps \tThrdId \tThrdRespId \tThrds \tTTLB \tRPS \tVerb \tCode \tURI");
                writer.WriteToLog(" ======== \t====== \t========== \t==== \t===== \t=== \t==== \t==== \t===");
            }

            writer.WriteToLog(" " + responseIdForLog
              + "\t\t" + response.clientId
              + "\t\t" + response.responseIdForCurrentClient
              + "\t" + numRestClients
              + "\t" + response.responseTtlb
              + "\t" + Math.Round(throughPut, 2)
              + "\t" + req.method
              + "\t" + response.responseStatsCode
              + "\t" + req.uri
              );


            if (req.extractText == true)
            {
                // Regex for correlation is in the script. 
                // Correlation Hints:
                // 1. Copy the resultBody into https://rubular.com/
                // 2. left and right boundary basic format: (?<=  <left str>    )(.*?)(?=  < rt string> )
                // 3. Use https://onlinestringtools.com/escape-string to escape what you build in Rubular. 
                // 4. Remember that Visual Studio might display the response with backslashes. However, copying the response
                //    to notepad or rubular will shows that it does not actually contain back slashes. 

                Regex regEx = new Regex(req.regExPattern);
                string extractedValue = regEx.Match(result.Content).Value;

                // place value into the correlation dictionary
                script.correlationsDict[req.nameForCorrelatedVariable] = extractedValue;
            }

            Thread.Sleep(thinkTimeBetweenRequests);

        }






    }


    /// <summary>
    /// This is not used but keeping it here just in case. 
    /// Usage example:
    /// using (new DisposableStopwatch(t => Console.WriteLine("{0} elapsed", t)) {
    /// do stuff that I want to measure
    ///}
    /// http://ashleyangell.com/2017/04/idisposable-stopwatch-to-benchmark-code-blocks-via-lambda-or-delegate-in-c/
    /// https://stackoverflow.com/questions/232848/wrapping-stopwatch-timing-with-a-delegate-or-lambda/855624
    /// </summary>
    public class DisposableStopwatch : IDisposable
    {
        private readonly Stopwatch sw;
        private readonly Action<TimeSpan> f;

        public DisposableStopwatch(Action<TimeSpan> f)
        {
            this.f = f;
            sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            sw.Stop();
            f(sw.Elapsed);
        }
    }




}
