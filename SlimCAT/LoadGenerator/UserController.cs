using SlimCAT.Models;
using SlimCAT.SignalR;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SlimCAT
{
    public class UserController
    {
        readonly LogWriter writer = LogWriter.Instance;

        private static int numThreads = 0;
        private static Stopwatch testStopWatch = new Stopwatch(); // used when to stop a test when the test type is "ByDuration."
        private static SignalRClient sgnlR = new SignalRClient(); // might need to make only one of these, not one per script. Singleton?
        private static Dictionary<string, bool> chartLineStatusDict; // flag to determine if chartLines have been initialized for current script. 




        /// <summary>
        /// Adds another user (thread) to process a new instance of the script.
        /// </summary>
        /// <param name="newUserEvery">Time in milliseconds of the wait time before adding another thread. E.g., every three seconds.</param>
        /// <param name="script">An instance of a script class. (Currenty manaully built.)</param>
        /// <param name="newUserEvery">Spread out the start of a new user.</param>
        /// <param name="maxUsers">Total number of users desired in the test.</param>
        /// <param name="testDurationSecs">When to stop the test. Optional. Test can also stop after a certain number of iterations.</param>
        /// <returns></returns>
        public async Task AddUsersByRampUp(Script script = null, int newUserEvery = 2000, int maxUsers = 2, long testDurationSecs = 360)
        {
            ChartData chartData = new ChartData(script);
            chartData.InitializeChartLines(script);

            // AnalyzeScriptToBuildChart(script);
            var tasksInProgressLst = new List<Task>();

            testStopWatch.Start(); // start the timer so that we can stop at time specified in testDurationSecs.

            // This is a "while loop thread launcher." It launches a new thread every x seconds.
            // The while loop condition here controls the *launching* of threads. It only partially controls the duration of the test. 
            //   We stop *launching* threads at the two specified conditions.
            //   However, threads will contiue executing requests until they have completed the work specified in stop handler helper method below.
            //   This is significant in the case of long running requests.
            //   We had to separate these two things so that the threads can complete the work assigned to them.
            while ((testStopWatch.ElapsedMilliseconds < testDurationSecs * 1000) & (Interlocked.Increment(ref numThreads) <= maxUsers)) // loop as long as load test lasts. 
            {
                var t = Task.Run(() =>
                {
                    LaunchClientAndHandleStop(script, numThreads, testDurationSecs);
                });
                tasksInProgressLst.Add(t);
                Thread.Sleep(newUserEvery);
            }
            await Task.WhenAll(tasksInProgressLst);
        }



        /// <summary>
        ///  Helper method for AddUsersByRampUp().
        ///  Handles the stop condition for the test.
        ///  A test can stop in two ways:
        ///     1. When it has reached a certain duration.
        ///     2. When it has completed a certain number of total passes over the script (iterations).
        ///  A thread (or task) will continue to process the script requests until one of these two conditions is met.
        ///  We start one task per user. This task *synchronous*; it sends one request at a time and it waits for each request to complete.
        ///  The default stop approach is "ByDuration."
        /// </summary>
        /// <returns>A Task</returns>
        private void LaunchClientAndHandleStop(Script script, int clientId, long testDurationSecs = 360, string testType = "ByDuration") //alt "ByTestIterations"
        {
            // The logic here is:
            // 1. create a client (an instance of the request sender).
            // 2. create list of requests for a client to process by creating a script instance. The client will iterate over this list until the condtion is met. 
            // 3. pass the list to a task (thread) and let that one thread synchrously process the request. 
            // 4. Use while loops to keep looping over the script's request list until a stop condtion is met. 

            SendRequests sr = new SendRequests(clientId, 3);
            List<Req> requestList = script.requestList;

            if (testType == "ByTestIterations")  // ToDo: This needs another condition the count of script iterations. I think another while loop. 
            {
                foreach (Req r in requestList)
                {
                    sr.SendRequest(script, r);
                }
            }
            else
            {
                // TestType is "ByDuration". 
                do
                {
                    // Re-execute each request in the script until the target test duration is reached.
                    foreach (Req r in requestList)
                    {
                        sr.SendRequest(script, r); // Note that we only pass the script so that we know it's name and where to assign the request result in the database/conccurent dictioary. 
                    }
                } while (testStopWatch.ElapsedMilliseconds < testDurationSecs * 1000);// ToDo: The timer here might be redudant. However, in the above method, the timer stops the thread launching. The timer here allows the thread to complete the iteration. 
            }
        }







    }
}
