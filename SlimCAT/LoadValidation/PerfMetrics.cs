using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SlimCAT
{

    // ToDo: https://stackoverflow.com/questions/30723766/calculating-average-and-percentiles-from-a-histogram-map
    // https://en.wikipedia.org/wiki/Percentile#Microsoft_Excel_method
    // https://stackoverflow.com/questions/8137391/percentile-calculation
    // https://stackoverflow.com/questions/3738349/fast-algorithm-for-repeated-calculation-of-percentile
    // https://stackoverflow.com/questions/41413544/calculate-percentile-from-a-long-array


    /// <summary>
    /// Executes performance checks against the results of a performance or load test.
    /// Required Parameters: PerformanceViolationChecker takes a dictionary of performance information.E.g., one of these:
    ///    ConcurrentDictionary<int, RequestResponseEntity>; 
    /// The above dictionary is created by the constructor of the PhoenixRunner class. 
    /// Example of calling PerformanceViolationChecker: 
    ///     PerformanceViolationChecker pvc = new PerformanceViolationChecker();
    ///     avgResponseTime = pvc.ResponseTimeAverage(conCurDicReqResponse);
    /// </summary>
    public class PerfMetrics
    {
        LogWriter writer = LogWriter.Instance;

        public Dictionary<string, double> CalcualteAllMetrics(ConcurrentDictionary<int, SlimCatResponse> conCurDicReqResponse = null, List<double> responseTimeArray = null, bool useTrim = false)
        {
            Dictionary<string, double> perfMetrics = new Dictionary<string, double>();

            //Part II: Start validating the load by using the PerformanceViolationChecker class. 
            double avgResponseTime = ResponseTimeAverage(conCurDicReqResponse, useTrim: true);
            double maxResponseTime = ResponseTimeMax(conCurDicReqResponse, useTrim: true);
            double throughPut = ThroughputAvg(conCurDicReqResponse);
            double exceptionsThrown = ExceptionThrown(conCurDicReqResponse);
            //double exceptionCount = pvc.Exceptions_Present(conCurDicReqResponse);
            double totalTestDuration = TotalTestDuration(conCurDicReqResponse);

            perfMetrics.Add("avgResponseTime", avgResponseTime);
            perfMetrics.Add("maxResponseTime", maxResponseTime); // note that I throw out the first two requests.
            perfMetrics.Add("throughPut", throughPut);
            perfMetrics.Add("exceptionsThrown", exceptionsThrown);
            perfMetrics.Add("totalTestDuration", totalTestDuration);

            writer.WriteToLog("");
            writer.WriteToLog("   avgResponseTime \tmaxResponseTime \tAvg. ThroughPut \tExceptionsThrown \tTotalTestDuration");
            writer.WriteToLog("   =============== \t=============== \t=============== \t================ \t=================");
            writer.WriteToLog("   " + avgResponseTime
          + "\t\t" + maxResponseTime
          + "\t\t\t" + throughPut
          + "\t\t\t" + exceptionsThrown
          + "\t\t\t" + totalTestDuration

          );

            return perfMetrics;
        }


        /// <summary>
        /// Finds average response time from a load test result set.
        /// </summary>
        /// <param name = "conCurDicReqResponse" > Dictionary object containing response times, error codes, and other 
        /// performance information collected during a load test.
        ///  Using coalescing operator to set a default collection if none supplied: 
        /// See: https://stackoverflow.com/questions/6947470/c-how-to-use-empty-liststring-as-optional-parameter.
        /// Uses optional parameters to allow the method to take either a concurrent dictionary of response times, or a list of response times. 
        /// To do: This could be refactored using a base class: https://stackoverflow.com/questions/3361891/alternative-to-a-series-of-overloaded-methods
        /// </param>
        /// <returns></returns>
        public virtual double ResponseTimeAverage(ConcurrentDictionary<int, SlimCatResponse> conCurDicReqResponse = null, List<double> responseTimeArray = null, bool useTrim = false)
        {
            //  use the passed -in parameters if supplied, else create new, blank, versions
            responseTimeArray ??= new List<double>();
            conCurDicReqResponse ??= new ConcurrentDictionary<int, SlimCatResponse>();

            if (responseTimeArray.Count == 0)
            {
                responseTimeArray = ConvertDictionaryToArray(conCurDicReqResponse);
            }

            double rtAvg = 0;
            double rtSum = 0;

            if (useTrim == false)
            {
                //calculate sum of all response times. 
                for (int j = 0; j <= responseTimeArray.Count - 1; j++)
                {
                    rtSum += responseTimeArray[j];
                }

                // calculate Average
                for (int j = 0; j <= responseTimeArray.Count - 1; j++)
                {
                    rtAvg = rtSum / responseTimeArray.Count;
                }
            }
            else if (useTrim == true)
            {
                // calculate sum of of response times using trim
                for (int j = 3; j <= responseTimeArray.Count - 1; j++)
                {
                    rtSum += responseTimeArray[j];
                }

                //calculate Average and trim.
                for (int j = 3; j <= responseTimeArray.Count - 1; j++)
                {
                    rtAvg = (rtSum / responseTimeArray.Count - 3);
                    int theCount = responseTimeArray.Count - 3;
                    rtAvg = (rtSum / theCount);
                }
            }

            double avgResponseTime = System.Math.Round(rtAvg, 2);
            return avgResponseTime;
        }




        public virtual double TotalTestDuration(ConcurrentDictionary<int, SlimCatResponse> conCurDicReqResponse = null, List<double> responseTimeArray = null, bool useTrim = false)
        {
            //  use the passed -in parameters if supplied, else create new, blank, versions
            responseTimeArray ??= new List<double>();
            conCurDicReqResponse ??= new ConcurrentDictionary<int, SlimCatResponse>();

            if (responseTimeArray.Count == 0)
            {
                responseTimeArray = ConvertDictionaryToArray(conCurDicReqResponse);
            }

            DateTime firstRequest = conCurDicReqResponse[0].requestTimeSent;
            DateTime lastResponse = conCurDicReqResponse[conCurDicReqResponse.Count - 1].requestTimeSent;

            double totalTestDuration = (lastResponse - firstRequest).TotalSeconds;
            totalTestDuration = Math.Round(totalTestDuration, 2);
            return totalTestDuration;
        }


        /// <summary>
        /// Finds the maximum response time from a load test result set.
        /// </summary>
        /// <param name = "conCurDicReqResponse" ></ param >
        /// < returns ></ returns >
        public double ResponseTimeMax(ConcurrentDictionary<int, SlimCatResponse> conCurDicReqResponse = null, List<double> responseTimeArray = null, bool useTrim = false)
        {
            double maxResponseTime = -99;

            // use the passed -in parameters if supplied, else create new versions
            responseTimeArray ??= new List<double>();
            conCurDicReqResponse ??= new ConcurrentDictionary<int, SlimCatResponse>();

            if (responseTimeArray.Count == 0)
            {
                responseTimeArray = ConvertDictionaryToArray(conCurDicReqResponse);
            }

            double rtMax = 0;

            if (responseTimeArray.Count > 0)
            {
                if (useTrim == false)
                {
                    rtMax = responseTimeArray.Max();
                    maxResponseTime = System.Math.Round(rtMax, 2);
                }
                else
                {
                    responseTimeArray.RemoveRange(0, 3); // remove the first three values which can have artifically long response times due to the JITting of the test method and also warm up of the application under test. 
                    rtMax = responseTimeArray.Max();
                    maxResponseTime = System.Math.Round(rtMax, 2);

                }
            }
            return maxResponseTime;
        }


        /// <summary>
        /// Calculates Average Throughput by dividing the count of received responses by the total time they have taken to execute.
        /// I am not going to impliment the trim feature here.
        /// This simple makes calcuations based on the time the request was received.
        /// </summary>
        /// <param name = "reqResponseDic" ></ param >
        /// < returns ></ returns >
        public double ThroughputAvg(ConcurrentDictionary<int, SlimCatResponse> conCurDicReqResponse)
        {
            //writer.WriteToLog("Number of elements in array=" + conCurDicReqResponse.Count.ToString());

            double throughPut = -99;
            DateTime[] dateTimeArr = null;

            try
            {
                dateTimeArr = new DateTime[conCurDicReqResponse.Count]; // Make a date time array equal in size to the elements of the concurrentDic. 
            }
            catch (Exception ex)
            {
                writer.WriteToLog("Error: # of elements in array=" + conCurDicReqResponse.Count.ToString());
                writer.WriteToLog(ex.Message.ToString());
            }
            finally
            {
                if (conCurDicReqResponse != null)
                {
                    int countOfReceivedResponses = conCurDicReqResponse.Count(x => x.Value.responseTtlb != -99);

                    //throw out the first response because we can only calculate starting FROM the time of the first response
                    for (int j = 0; j < countOfReceivedResponses; j++)
                    {
                        dateTimeArr.SetValue(conCurDicReqResponse[j].responseTimeReceived, j);
                    }

                    DateTime dateTimeMax = dateTimeArr.Max();
                    DateTime dateTimeMin = conCurDicReqResponse[0].responseTimeReceived; // idea here is to find the time received for the first response

                    double totalTime = dateTimeMax.Subtract(dateTimeMin).TotalSeconds;
                    double subtractFirstReq = countOfReceivedResponses - 1;
                    throughPut = subtractFirstReq / totalTime; //
                    throughPut = Math.Round(throughPut, 2);
                }
            }
            return throughPut;
        }



        /// <summary>
        /// Calculates standard deviation of the response times in the request/response dictionary.
        /// </summary>
        /// <param name = "conCurDicReqResponse" ></ param >
        /// < returns ></ returns >
        public double ResponseTimeStdDev(List<double> responseTimeArray = null, ConcurrentDictionary<int, SlimCatResponse> conCurDicReqResponse = null)
        {
            double stdDevResponseTime = -99;

            // use the passed -in parameters if supplied, else create new versions
            responseTimeArray ??= new List<double>();
            conCurDicReqResponse ??= new ConcurrentDictionary<int, SlimCatResponse>();

            if (responseTimeArray.Count == 0)
            {
                responseTimeArray = ConvertDictionaryToArray(conCurDicReqResponse);
            }

            double theStdDeviation = 0;
            double theSum = 0;

            //  calculate sum of all response times. 
            for (int j = 2; j < responseTimeArray.Count - 1; j++)
            {
                theSum += responseTimeArray[j];
            }

            if (responseTimeArray.Count > 0)
            {
                theStdDeviation = System.Math.Sqrt(theSum / responseTimeArray.Count - 1);
            }
            stdDevResponseTime = Math.Round(theStdDeviation, 2);

            return stdDevResponseTime;
        }


        /// <summary>
        /// Returnes a bool if any response time is above the threshold.
        /// </summary>
        /// <param name = "conCurDicReqResponse" ></ param >
        /// < param name="threshold"></param>
        /// <returns>Boolean</returns>
        public bool ResponseTimeThresholdViolation(double threshold, List<double> responseTimeArray = null, ConcurrentDictionary<int, SlimCatResponse> conCurDicReqResponse = null)
        {
            // use the passed -in parameters if supplied, else create new versions
            responseTimeArray ??= new List<double>();
            conCurDicReqResponse ??= new ConcurrentDictionary<int, SlimCatResponse>();

            if (responseTimeArray.Count == 0)
            {
                responseTimeArray = ConvertDictionaryToArray(conCurDicReqResponse);
            }

            bool violationYesNo = false;
            // flag if any request greater than the threshold
            for (int j = 2; j < responseTimeArray.Count - 1; j++)
            {
                if (responseTimeArray[j] > threshold)
                {
                    violationYesNo = true;
                }
            }
            return violationYesNo;
        }




        /// <summary>
        /// Returnes a bool if any response time is above the threshold.
        /// </summary>
        /// <param name = "conCurDicReqResponse" ></ param >
        /// < param name="threshold"></param>
        /// <returns>Boolean</returns>
        public double ExceptionThrown(ConcurrentDictionary<int, SlimCatResponse> conCurDicReqResponse = null)
        {
            double exceptionThrown = 0;
            // flag if any request has an exception.

            for (int j = 0; j < conCurDicReqResponse.Count - 1; j++)
            {
                if (conCurDicReqResponse[j].exceptionThrown == true)
                {
                    exceptionThrown++;
                }
            }
            return exceptionThrown;
        }



        /*
/ <summary>
/ Finds average response time from a load test result set. 
/ </summary>
/ <param name="conCurDicReqResponse">Dictionary object containing response times, error codes, and other 
/ performance information collected during a load test. 
/  Using coalescing operator to set a default collection if none supplied: 
/ See: https://stackoverflow.com/questions/6947470/c-how-to-use-empty-liststring-as-optional-parameter.
/ Uses optional parameters to allow the method to take either a concurrent dictionary of response times, or a list of response times. 
/ To do: This could be refactored using a base class: https://stackoverflow.com/questions/3361891/alternative-to-a-series-of-overloaded-methods
/ </param>
/ <returns></returns>
public virtual double Exceptions_Present(ConcurrentDictionary<int, RequestResponseEntity> conCurDicReqResponse = null, List<double> responseTimeArray = null, bool useTrim = false)
{
     use the passed-in parameters if supplied, else create new, blank, versions
    responseTimeArray = responseTimeArray ?? new List<double>();
    conCurDicReqResponse = conCurDicReqResponse ?? new ConcurrentDictionary<int, RequestResponseEntity>();

    if (responseTimeArray.Count == 0)
    {
        responseTimeArray = ConvertDictionaryToArray(conCurDicReqResponse);
    }
    return avgResponseTime;

}

*/




        /// <summary>
        /// This is useful for the Azure DevOps Log
        /// </summary>
        /// <param name = "reqResponseDic" ></ param >
        public void PrintResponseTimesToConsole(ConcurrentDictionary<int, SlimCatResponse> reqResponseDic)
        {
            for (int i = 0; i <= reqResponseDic.Count - 1; i++)
            {
                Console.WriteLine("Response time " + i + " = " + reqResponseDic[i].responseTtlb.ToString() + " , " + reqResponseDic[i].responseBody);
            }
        }


        /// <summary>
        /// Converts a dictionary full of response times to just a simple array of doubles
        /// containing the response time of each request.
        /// This makes it easier to iterate through the response times, and we can also leverage the array's min and max functions. 
        /// </summary>
        /// <param name = "conCurDicReqResponse" > Dictionary full of response times. </param>
        //    / <returns>A Collection of doubles representing all the response times in the current responses</returns>
        public List<double> ConvertDictionaryToArray(ConcurrentDictionary<int, SlimCatResponse> conCurDicReqResponse)
        {
            List<double> dblArrayOfResponseTimes = new List<double>();
            int countOfReceivedResponses = conCurDicReqResponse.Count(x => x.Value.responseTtlb != -99 || x.Value.exceptionThrown != true); //

            for (int j = 0; j < countOfReceivedResponses; j++)
            {
                dblArrayOfResponseTimes.Add(conCurDicReqResponse[j].responseTtlb);
            }
            return dblArrayOfResponseTimes;
        }

    } // End of PerformanceViolationChecker class



}
