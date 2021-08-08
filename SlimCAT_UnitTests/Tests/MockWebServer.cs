using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace APD_DAP.UnitTests.MockWebServer
{


    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        private static readonly Lazy<WebServer> lazy =
          new Lazy<WebServer>(() => new WebServer(SendResponse, "http://localhost:8080/test/"), LazyThreadSafetyMode.ExecutionAndPublication);

        private WebServer() { }

        public static WebServer Instance { get { return lazy.Value; } }

        /// <summary>
        /// The response string for our tests. 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string SendResponse(HttpListenerRequest request)
        {
            Thread.Sleep(5); // just to give a brief amount of latency, so we get something besides zero in the logs. 
            return string.Format("<HTML><BODY>My web page.<br>{0}</BODY></HTML>", DateTime.Now);
        }



        public WebServer(string[] prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example 
            // "http://localhost:8081/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (method == null)
                throw new ArgumentException("method");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = method;
            _listener.Start();
        }

        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
            : this(prefixes, method) { }


        public async void Run()
        {
            ThreadPool.SetMinThreads(200, 200);
            ThreadPool.SetMaxThreads(500, 500);

            ThreadPool.QueueUserWorkItem((o) =>
            {
                Debug.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(async (c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                string rstr = _responderMethod(ctx.Request);
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                await Task.Run(() => ctx.Response.OutputStream.Write(buf, 0, buf.Length));
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }


}
