using SlimCAT.Models;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace SlimCAT
{
    public partial class SendHttpClientRequest
    {
        private static long testCompletions = 0;

        /// <summary>
        /// Takes care of correlation.
        /// </summary>
        /// <param name="scriptInstance"></param>
        /// <param name="slimCatReq"></param>
        /// <param name="slimCatResponse"></param>
        public void AfterRequestActions(Tuple<Script, SlimCatReq, SlimCatResponse> slimCatResponseInfo)
        {
            Script scriptInstance = slimCatResponseInfo.Item1;
            SlimCatReq slimCatReq = slimCatResponseInfo.Item2;
            SlimCatResponse slimCatResponse = slimCatResponseInfo.Item3;

            int chartScalingFactor_TestThroughput = Convert.ToInt32(Extensions.GetConfiguration("chartScalingFactor_TestThroughput"));
            bool logFullReqAndReponse = Convert.ToBoolean(Extensions.GetConfiguration("logFullReqAndReponse"));
            if (logFullReqAndReponse == true)
            {
                string fullResponse;
                string statusCode;
                string reasonPhrase;
                if (slimCatResponse.httpResponseMessage != null)
                {
                    fullResponse = slimCatResponse.httpResponseMessage.Content.ReadAsStringAsync().Result;
                    reasonPhrase = slimCatResponse.httpResponseMessage.ReasonPhrase;
                    statusCode = slimCatResponse.httpResponseMessage.StatusCode.ToString();
                }
                else
                {
                    fullResponse = "Response was null or bad";
                    reasonPhrase = "Response was null or bad";
                }

                using (StreamWriter sw = new StreamWriter(@"C:\log\" + "FullResponseLog.log", true))
                {                    
                    sw.WriteLine($" {responseIdForLog},{reasonPhrase}, {fullResponse}");
                    sw.WriteLine("");
                    sw.Flush();
                    sw.Close();
                }
            }


            // correlatiion: grab values from response to use in next request in the script
            if (slimCatReq.extractText_FromResponseBody == true)
            {
                ExtractText(slimCatReq, scriptInstance, slimCatResponse.responseBody);
            }



            //ToDo: These are throwing an error if null
            //slimCatResponse.responseExceptionMessage = slimCatResponse.httpResponseMessage.ReasonPhrase;

           slimCatResponse.responseBody = slimCatResponse.httpResponseMessage.Content.ReadAsStringAsync().Result;

            slimCatResponse.reqUri = slimCatReq.uri;
            slimCatResponse.reqBody = slimCatReq.body;
            // response.reqVerb = req.method;
            // response.reqDescription = req.body;
            // response.requestTimeSent = req.body;
            // response.reqUriParams = req.body;
            slimCatResponse.responseTtlb = stopwatch.ElapsedMilliseconds;
            slimCatResponse.clientId = _clientId;
            slimCatResponse.responseId = responseIdForConcurrentDict;
            slimCatResponse.responseTimeReceived = DateTime.Now;
            slimCatResponse.responseStatus = "Finished";
            slimCatResponse.responseIdForCurrentClient = responseIdForCurrentClient++;
            slimCatResponse.responseStatusCode = slimCatResponse.httpResponseMessage.StatusCode.ToString();

            ResponseDb.conCurResponseDict.TryAdd(Interlocked.Increment(ref responseIdForConcurrentDict), slimCatResponse);

            // calculate response time of request
            double responseTimeInSec = slimCatResponse.responseTtlb / 1000.0;


            // Here is what we need to put a point on a line:
            // 1. Which line does this data belong to? 
            // 2. What position along that line?
            // 3. What is the value to put on that line? (TTLB)
            // Here we plot the response time or throughput data and then increment the pointer in the dictionary
            sgnlR.AddData(slimCatReq.uriIdx, ChartData.countPerUriDict[slimCatReq.uriIdx], responseTimeInSec);
            ChartData.countPerUriDict[slimCatReq.uriIdx] += 1;

            // caclulate TEST throughput (not request throughput!)
            double testThroughPut = 0;
            if (Interlocked.Read(ref testCompletions) > 3)
            {
                //// 10/14/21 - this throughput calcuation is still not right. This is response-based, not test iteration based.
                //// need the timestamp of each test completion to do this right.
                //// test completion timestamp needs to be stored in conCurResponseDict
                //// we can get the time embeded in the request
                //// the idea here is to wait until we have a few requests.              

                //var valuesLst = ResponseDb.conCurResponseDict.Values.Where(x => x.testCompletionTime > testStartTime).ToList();
                //var completionTimeXIterationsAgo = valuesLst[(int)testCompletions -3].testCompletionTime;

                //TimeSpan aDuration = DateTime.Now - completionTimeXIterationsAgo;
                //testThroughPut = 3 / aDuration.TotalSeconds;

                // same as below
                TimeSpan testDuration = DateTime.Now - testStartTime;
                testThroughPut = Math.Round(Interlocked.Read(ref testCompletions) / testDuration.TotalSeconds, 2);

            }
            else
            {
                TimeSpan testDuration = DateTime.Now - testStartTime;
                testThroughPut = Math.Round(Interlocked.Read(ref testCompletions) / testDuration.TotalSeconds, 2);
            }


            ///9/22/22 - this is throwing an error on flows with only 1 request.
            ///This puts the throughput point on the chart. 
            // int chartScalingFactor_TestThroughput = 10;
            // add throughput to chart only on first request (1 per test iteration).
            if (slimCatReq.uriIdx == scriptInstance.slimCatRequestList.Count - 1)
            {
                Interlocked.Increment(ref testCompletions);
                ResponseDb.conCurResponseDict[responseIdForConcurrentDict].testCompletionTime = DateTime.Now;
              //  sgnlR.AddData(ChartData.testThroughputIdx, ChartData.countPerUriDict[ChartData.testThroughputIdx], Math.Round(testThroughPut * chartScalingFactor_TestThroughput, 2));
               // ChartData.countPerUriDict[ChartData.testThroughputIdx] += 1;
            }


            try
            {

                TimeSpan duration = DateTime.Now - testStartTime;
                double reqPerSec = Math.Round(Interlocked.Increment(ref responseIdForLog) / duration.TotalSeconds, 2);

                if (Interlocked.Read(ref responseIdForLog) % 25 == 0 || Interlocked.Read(ref responseIdForLog) == 1)
                {
                    writer.WriteToLog(" #Resps \tThrdRespId \tThrds \tTTLB \tTest/sec \tRPS \tVerb \tCode \tTransName \t\t\tURI");
                    writer.WriteToLog(" ====== \t========== \t===== \t==== \t======== \t=== \t==== \t==== \t========= \t\t\t===");
                }

                writer.WriteToLog(" " + responseIdForLog
                  // + "\t\t" + slimCatReq.taskOrThreadId
                  + "\t\t" + slimCatResponse.responseIdForCurrentClient
                  // + "\t" + numHttpClients
                  + "\t\t" + SlimCAT.UserController.curNumThreads
                  + "\t" + slimCatResponse.responseTtlb
                  + "\t\t" + Math.Round(testThroughPut, 2)
                  + "\t" + Math.Round(reqPerSec, 2)
                  + "\t" + slimCatReq.httpReqMsg.Method.ToString()
                  + "\t" + slimCatResponse.responseStatusCode
                  + "\t" + slimCatReq.reqNameForChart
                  + "\t\t\t" + slimCatReq.httpReqMsg.RequestUri
            // + "\t" + slimCatResponse.responseBody.Substring(0, 100) // Full response is now logged using appConfig switch

            ); ;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            if (slimCatReq.extractText_FromResponseBody == true)
            {
                // Regex for correlation is in the script. 
                // Correlation Hints:
                // 1. Copy the resultBody into https://rubular.com/
                // 2. left and right boundary basic format: (?<=  <left str>    )(.*?)(?=  < rt string> )
                // 3. Use https://onlinestringtools.com/escape-string to escape what you build in Rubular. 
                // 4. Remember that Visual Studio might display the response with backslashes. However, copying the response
                //    to notepad or rubular will shows that it does not actually contain back slashes. 

                Regex regEx = new Regex(slimCatReq.regExPattern);
                string extractedValue = regEx.Match(slimCatResponse.responseBody).Value;

                // place value into the correlation dictionary
                scriptInstance.correlationsDict[slimCatReq.correlatedVariableKeyName] = extractedValue;
            }
        }
    }
}