# SlimCAT - A DevOps CI/CD Performance Test Tool

<img src="https://github.com/charlesdwmorrison/SlimCAT/blob/master/SlimCAT.jpg?raw=true" align="left" alt="drawing" width="250" height="300"/>

- SlimCAT is a C# (.Net 5.0 /.Net Core) class library implementing features typically found in load tools:   
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; - launch varying amounts of multiple threads (users) and run them for a specific duration or a specified number of requests.    
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; - correlate data from one request to another so user "flows" or scenarios can be contructed from multiple requests (such as: logon, do transaction, log out).    
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; - measurement of response time, throughput    
- SlimCAT can be used in CI/CD pipelines or as a standalone desktop tool.
- SlimCAT makes writing multi-threaded tests as easy as writing any functional, MSTest or NUnit test. 

![SlimCAT SignalR Chart.js](https://github.com/charlesdwmorrison/SlimCAT/blob/master/SlimCAT_Gif.gif)

- SlimCAT uses NUnit to assert against response time metrics; pipelines can then make decsions based on this pass/fail result. 
- SlimCAT has a simple design. If you can write some C# code, you can follow the examples and load test your own applications.
- SlimCAT follows principles and architecture of well-known load tools, with scripts, correlations, and data sources.
- Tests can be as short as 10-13 seconds (ideal for pipeline use), or as long as several hours. 

## Features/Components   
Advancements over vernerable tools such as [Netling](https://github.com/hallatore/Netling) and [K6](https://medium.com/swlh/beginners-guide-to-load-testing-with-k6-ff155885b6db) include:
- .Net 5.0 (.Net Core). SlimCAT can be executed on Linux or a Mac.
- Correlation so that you can add data and vary the body or URI.
- Ability to test more than one URL or endpoint per test test. 
- Test user scenarios,not just one URI; just like LoadRunner or Visual Studio load tests.   
- User flows ("scripts") composed in familiar C# code, not JavaScript.
- Use NUnit or MSTest asserts to generate pass/fail results for any URL or for the test as a whole.
- Ramp up users gradually, not just "slam testing."    
- Logging class logs response time, throughput and other metrics to a CSV file.
- Perf Metrics class calculates response time average, percentiles and throughput.
- Stay in the Visual Studio IDE to create scripts. 
- Easier to understand script syntax compared to [K6](https://medium.com/swlh/beginners-guide-to-load-testing-with-k6-ff155885b6db) or some other tools. 
- Open Source and easily customizable.
- Use standard DevOps functional test agent machines instead of dedicated load generators.


SlimCAT takes its name from the old Microsoft [WCAT - Web Capacity Analysis Tool](https://docs.microsoft.com/en-us/previous-versions/ms951774)
which [Fiddler can still export](http://www.tiernok.com/posts/implementing-wcat-for-load-testing.html). See also [here](https://blogs.iis.net/thomad/using-the-wcat-fiddler-extension-for-web-server-performance-tests)


## Usage
### Scripts (User Flows)
Scripts consist of a collection of requests. E.g., 

```
List<Req>
```

Scripts in SlimCAT are classes with one method, BuildRequest(). BuildRequest() returns a list of the requests you want to execute.  
The following example should look somewhat familiar if you have done any LoadRunner scripting:

```
public class S02_OnlineRestExampleScript : Script
    {
        public  List<Req> BuildRequestList()
        {
            requestList = new List<Req>()
              {
                new Req()
                {
                    uri = urlPrefix + "/employees",
                    method = Method.GET,
                    extractText = true,
                    nameForCorrelatedVariable = "empId",           
                    regExPattern = "(?<={\"id\":\")(.*?)(?=\",\"employee_name)"
                },
                new Req()
                {
                    method = Method.GET,
                    // Example of using correlated value from above
                    useExtractedText = true, // instructs SendRequest() to use a correlated value
                    nameForCorrelatedVariable = "empId",
                    uri = urlPrefix + "/employee/" + correlationsDict["empId"]
                },
                new Req
                {
                    uri = urlPrefix + "/create",
                    method = Method.POST,
                    body = "{\"name\":\"test\",\"salary\":\"123\",\"age\":\"23\"}"
                },
                 new Req
                {
                    uri = urlPrefix + "/update/1",
                    method = Method.PUT,
                    body = "{\"name\":\"test\",\"salary\":\"123\",\"age\":\"23\"}"
                },
                new Req
                {
                    uri = urlPrefix + "/delete/2",
                    method = Method.DELETE,
                }
            };

            return requestList;
        }
    }
```

Correlation (using the response of one request as data for the next) is accomplished by means of regular expressions:   

Correlation RegEx Syntax:  
```
(?<=  <left boundary> )(.*?)(?=   < right boundary> )
```

### Users (Threads)
To create multiple users, a script object is passed to a user controller class, which launches threads (in the form of C# tasks).
The threads execute the requests defined in the List<Req> collection.    
The method "AddUsersByRampUp()" takes any collection of requests, creates a new instance of it every X number of seconds, and passes it to a SendRequest() method:

```
public async Task AddUsersByRampUp(Script script = null, int newUserEvery = 2000, int maxUsers = 2, long testDurationSecs = 360)
{
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
```
### Test Execution
To make a multi-user test, you just put the above two components together:   
- call the BuildRequestList() method of the script   
- pass the script object to the user controller  
- finally, add an assertion:  

```
[Test]
public async Task S02_OnlineRest_3_Users()
{
    // arrange
    S02_OnlineRestExampleScript onlineRestExampleScript = new S02_OnlineRestExampleScript();
    UserController uc = new UserController();
    PerfMetrics pm = new PerfMetrics();

    // act
    List<Req> requestList = onlineRestExampleScript.BuildRequestList();    
    await Task.Run(() => uc.AddUsersByRampUp(script: onlineRestExampleScript, newUserEvery: 10000, maxUsers: 3, testDurationSecs: 30));
    Dictionary<string,double> perfMetrics = pm.CalcualteAllMetrics(ResponseDb.conCurResponseDict);

    // assert
    Assert.IsTrue(perfMetrics["avgResponseTime"] < 3, "Expected:Avg. Response time < 3 seconds");
}
```

As you can see from the above, SlimCAT also has a PerfMetrics class which performs calculations on the results and which you can assert against to determine if the test passed or failed.

The SendRequest() class currently consists of a static .Net HttpClient which sends the requests. The class also performs the correlations defined in the script, and starts and stops a timer measuring response time.


## Planned Enhancements
- GUI with a chart to show response time and throughput. This will be done as [Blazor WebAssembly Progressive Web App](https://devblogs.microsoft.com/visualstudio/building-a-progressive-web-app-with-blazor).
- Build as a kubernetes app.
- Examples showing how to use a CSV file as data input.
- Examples of how to assert against a particular URL within a script.
- Create scripts programmatically by importing .har files. 
- Run multiple scripts at the same to create a load scenario.
- Create a console version in order to execute from the command line. (I'm not 100% sure this is a goal.)
- Test Agents to use more than one machine (distant goal).

## Suggestions/FAQ
- Leverage Application Insights, AppDynamics, Splunk or other monitoring tool to visualize the load test.
- If you do that, you do not need a GUI.     
- A SignlR GUI is provided for cases when a monitoring tool is not available. 


Illustration 112514351 � Martin Malchev | Dreamstime.com
Keywords: Performance, Load, Load Test, Test, chart.js, Threads, Multi-Threaded, Tasks, Stress, Capacity, Capacity Testing, WCAT, Capacity Analysis Tool



