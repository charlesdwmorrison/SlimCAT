using SlimCAT.SignalR;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;


// Things to do 10/6/21
// done - get throughput on the chart -- done
// done - fix or add flag for logging full requests and responses - done
// done - add validaton rule using a regex and log that instead of substring in C:\log - done
// done - in the script, need transaction names or URI names and these need to go on the graph - done
// done - Make it so chart is cleared every run

// Add a user line
// Add an error line 
// need scaling factor for lines. Could be a config file? app.json?
// dump perf metrics to console or log after test
// log needs iterations per second as well as rps
// why is CPU so high on chrome?
// Unit tests
//  -- is correlation working?
// separate files for "Pre-REquest" and "Post-request"
// abort if bad validation found
// Throughput, if longer than 1 min, needs to recalcuate based on last 50 requests or something
// chart is taking 25% CPU because legend is redrawn every request
// faster cloning mechanism?
// Move logging to different file
// In the chart javascript elimiate graphing 1st request.
// ambiguity between extractText and extractBodyText. Do I need extact text? ANS. NO! "useExtractText" means use a pre-existing value from the dictionary. Like from the first request. "extractText" means do a re-extraction. 
// need to do correlation for URI's
// If signalR console is not up, let test run anyway. (I think)
// It could be on correlation that I can just refer to the name in the dictionary. I don't have to re-add the regex if the 
//   request is merely going to reuse it. Make it more like LoadRunner. Put it at the top of the request, before URL? 

namespace SlimCAT
{
    public partial class SendHttpClientRequest
    {
        public static int responseIdForConcurrentDict = -1;
        public static long responseIdForLog = 0;
        private static DateTime testStartTime;
        private SignalRClient sgnlR = new SignalRClient(); // might need to make only one of these, not one per script. Singleton?
        private Stopwatch stopwatch;
        //public static int numHttpClients = 0;
        public int responseIdForCurrentClient = 0;  // not static. Count is unique per client instance.
        public int _clientId;
        private LogWriter writer;
        private int thinkTimeBetweenRequestsInMs; // this IS think time between requests, not between script iterations 
                                                  //public static int responseIdForConcurrentDict = -1;
        private static HttpClient _httpClient;

        public SendHttpClientRequest(int _thinkTimeBetweenRequestsInMs)
        {
            writer = LogWriter.Instance;
            thinkTimeBetweenRequestsInMs = _thinkTimeBetweenRequestsInMs;
            stopwatch = new Stopwatch(); // note that there is only 1 stopwatch per SendRequest object. See note below about debugging.
        }

        private void EnsureHttpClientCreated()
        {
            if (_httpClient == null)
            {
                CreateHttpClient();
            }
        }


        public async void SendRequest(Tuple<Script, int, SlimCatReq> slimCatReqInfo)
        {
            Script scriptInstance = slimCatReqInfo.Item1;
            int taskOrThreadId = slimCatReqInfo.Item2;
            SlimCatReq slimCatReq = slimCatReqInfo.Item3;

            SlimCatResponse slimCatResponse = new SlimCatResponse();
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

            EnsureHttpClientCreated();


            if (responseIdForConcurrentDict == -1)
            {
                testStartTime = DateTime.Now; 
            }

         
            // Debug Request and request headers
            var theRequest = slimCatReq.httpReqMsg.Content.ReadAsStringAsync().Result;
            string stringOfHeaders = "";
            var headerColl = slimCatReq.httpReqMsg.Headers;
            foreach (var hdr in headerColl)
            {
                stringOfHeaders = stringOfHeaders + hdr.Key + ":" + hdr.Value.FirstOrDefault() + "\n";
            }


            // Add headers
            var theHeaders = slimCatReq.httpReqMsg.Headers;
            foreach (var hdr in theHeaders)
            {
                _httpClient.DefaultRequestHeaders.Add(hdr.Key, hdr.Value);
            }

            if (logFullReqAndReponse == true)
            {
                var requestDebug = slimCatReq.httpReqMsg.Content.ReadAsStringAsync().Result;
                using (StreamWriter swReq = new StreamWriter(@"C:\log\" + "FullRequestLog.log", true))
                {
                    swReq.WriteLine(requestDebug);
                    swReq.WriteLine(slimCatReq.httpReqMsg.Headers.ToString());
                    swReq.Flush();
                    swReq.Close();
                }
            }

            //Thread.Sleep(slimCatReq.thinkTimeInMs);
            Thread.Sleep(3000);

            slimCatResponse.requestTimeSent = DateTime.Now;           
            stopwatch.Restart();

            // One Try Catch around all POST, GET, Delete,PUT
            try
            {
                if (slimCatReq.httpReqMsg.Method == HttpMethod.Post)
                {
                    httpResponseMessage = await _httpClient.SendAsync(slimCatReq.httpReqMsg).ConfigureAwait(false);
                    //httpResponseMessage.EnsureSuccessStatusCode(); // do not use. Makes the response null.
                }

                if (slimCatReq.httpReqMsg.Method == HttpMethod.Get)
                {
                    httpResponseMessage = _httpClient.GetAsync(slimCatReq.httpReqMsg.RequestUri).Result;                    
                }

                // https://www.c-sharpcorner.com/UploadFile/dacca2/http-request-methods-get-post-put-and-delete/
                if (slimCatReq.httpReqMsg.Method == HttpMethod.Put)
                {
                    httpResponseMessage = _httpClient.PutAsync(slimCatReq.httpReqMsg.RequestUri.ToString(), slimCatReq.httpReqMsg.Content).Result;
                }

                if (slimCatReq.httpReqMsg.Method == HttpMethod.Delete)
                {
                   httpResponseMessage = _httpClient.DeleteAsync(slimCatReq.httpReqMsg.RequestUri).Result;                   
                }

                string resultStr = httpResponseMessage.Content.ReadAsStringAsync().Result;
                slimCatResponse.httpResponseMessage = httpResponseMessage; 

            }
            catch (Exception ex)
            {
                //ToDo: We need a log and a chart line here about errors.
                // Do the log first.
                // E.g., OnlineRestExampple throws "too many requests." 
                string msg = ex.Message;
                slimCatResponse.responseExceptionThrown = true;
                slimCatResponse.responseExceptionMessage = msg;
                writer.WriteToLog(msg);
            }
            finally
            {
                if (slimCatReq.clearHeaders == true)
                {
                    _httpClient.DefaultRequestHeaders.Clear();
                }
            }

            Tuple<Script, SlimCatReq, SlimCatResponse> slimCatReponseInfo =
                new Tuple<Script, SlimCatReq, SlimCatResponse>(scriptInstance, slimCatReq, slimCatResponse);

            AfterRequestActions(slimCatReponseInfo);
        }
    }
}

