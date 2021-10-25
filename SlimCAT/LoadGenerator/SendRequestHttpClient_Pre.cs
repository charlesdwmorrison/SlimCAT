using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace SlimCAT
{
    public partial class SendHttpClientRequest
    {
        bool logFullReqAndReponse = Convert.ToBoolean(Extensions.GetConfiguration("logFullReqAndReponse"));

        public void PreRequestActions(Script scriptInstance, SlimCatReq slimCatReq)
        {
            // even though we create a new scriptInstance, a user iterates over this instance, so we still must overcome "message has alreay been sent" limitation.
            // clone request in order to get around the HttpRequestMessage "unable to resend" limitation.
            // This also prevents threads from modifying each other's content.
            HttpRequestMessage httpReqMsgClone = Clone(slimCatReq.httpReqMsg);
            slimCatReq.httpReqMsg = httpReqMsgClone;

            // position of request in the list of requests.    
            // This is currently used only for charting.
            slimCatReq.uriIdx = scriptInstance.slimCatRequestList.IndexOf(slimCatReq);
            int taskOrThreadId = UserController.curNumThreads;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Modifiy request with values we want to correlate
            RebuildRequestWithCorrelatedValues(slimCatReq, scriptInstance);

            // modify request based on correlated value
            if (slimCatReq.useExtractedUriText == true)
            {
                string keyName = slimCatReq.nameForCorrelatedVariable;
                // ToDo:Fix this like the body correlation. 
                slimCatReq.uri = slimCatReq.uri.Replace("Correlated Value Not Initialized", scriptInstance.correlationsDict[keyName]);
            }

            SlimCatResponse slimCatResponse = new SlimCatResponse();
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

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

            Tuple<Script, int, SlimCatReq> slimCatReqInfo =
                        new Tuple<Script, int, SlimCatReq>(scriptInstance, taskOrThreadId, slimCatReq);            

            SendRequest(slimCatReqInfo);
        }

        private static string NormalizeBaseUrl(string url)
        {
            return url.EndsWith("/") ? url : url + "/";
        }


        private const string SEND_STATUS_FIELD_NAME = "_sendStatus";
        private void ResetSendStatus(HttpRequestMessage request)
        {
            TypeInfo requestType = request.GetType().GetTypeInfo();
            FieldInfo sendStatusField = requestType.GetField(SEND_STATUS_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
            if (sendStatusField != null)
                sendStatusField.SetValue(request, 0);
            else
                throw new Exception($"Failed to hack HttpRequestMessage, {SEND_STATUS_FIELD_NAME} doesn't exist.");
        }


        // https://stackoverflow.com/questions/18000583/re-send-httprequestmessage-exception/43512592
        // https://stackoverflow.com/questions/25044166/how-to-clone-a-httprequestmessage-when-the-original-request-has-content?noredirect=1&lq=1
        public HttpRequestMessage Clone(HttpRequestMessage httpRequestMessage)
        {
            HttpRequestMessage httpRequestMessageClone = new HttpRequestMessage(httpRequestMessage.Method, httpRequestMessage.RequestUri);

            var debug1 = UserController.numThreads;
            if (httpRequestMessage.Content != null)
            {
                var ms = new MemoryStream();
                httpRequestMessage.Content.CopyToAsync(ms);
                ms.Position = 0;
                httpRequestMessageClone.Content = new StreamContent(ms);

                httpRequestMessage.Content.Headers?.ToList().ForEach(header => httpRequestMessageClone.Content.Headers.Add(header.Key, header.Value));
            }

            var debug2 = UserController.numThreads;

            httpRequestMessageClone.Version = httpRequestMessage.Version;

            httpRequestMessage.Properties.ToList().ForEach(props => httpRequestMessageClone.Properties.Add(props));
            httpRequestMessage.Headers.ToList().ForEach(header => httpRequestMessageClone.Headers.TryAddWithoutValidation(header.Key, header.Value));

            var debug = UserController.numThreads;

            return httpRequestMessageClone;
        }

    }
}