using SlimCAT;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SlimCAT_UnitTests.Scripts
{

    // has a good client
    // https://stackoverflow.com/questions/37214345/rapid-web-requests-to-many-different-websites-using-httpclient-c-sharp

    // the slimCatREquest is a wrapper around httpMessageRequest or a restSharpRequest 
    class S04_PublicSOAPExample : Script
    {
        public S04_PublicSOAPExample()
        {
            // Usage Notes:
            // Register correlations by adding another element to this dictionary.
            // This dictionary is inherted from class "Script". 
            // ToDo: This is not completely kosher that we build the dictionary in the constructor.
            scriptName = GetType().Name;
            correlationsDict.Add("SessionStateId", "Correlated Value Not Initialized");

            BuildRequestList(); 
        }


        string requestContentStr;
        StringContent reqContent;

        // Question - 10/1/2021 - : Why don't I build the HttpRequestMessage first? Then wrap the SlimCat stuff around it?
        // This would be cleaner. 

        public List<SlimCatReq> BuildRequestList()
        {
            slimCatRequestList = new List<SlimCatReq>();
            var rootUri = new UriBuilder("https://www.dataaccess.com/webservicesserver/NumberConversion.wso");
            string host = "https://www.dataaccess.com";
            string api = "http://ensenta.com/ECPartnerDepositRequest";
            var dateTimeString = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss").ToString();


            //1. Signon request
            var httpReqMsg = new HttpRequestMessage(HttpMethod.Post, host + "/webservicesserver/NumberConversion.wso");
            httpReqMsg.Headers.Add("SOAPAction", api + "/SingleSignon/PartnerSSORequest/IPartnerSSORequest/StartSession");
            httpReqMsg = AddGlobalRequestHeaders(httpReqMsg);

            requestContentStr = @"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                  <soap:Body>
                                    <NumberToWords xmlns=""http://www.dataaccess.com/webservicesserver/"">
                                      <ubiNum>500</ubiNum>
                                    </NumberToWords>
                                  </soap:Body>
                                </soap:Envelope>";

            reqContent = new StringContent(requestContentStr, Encoding.UTF8, "text/xml");
            httpReqMsg.Content = reqContent;

            // Add SlimCat Extras and wrap httpReqMsg message with SlimCatReq
            SlimCatReq slimCatReq = new SlimCatReq()
            {
                //extractText = true,
                //nameForCorrelatedVariable = "SessionStateId",
                //regExPattern = "(?<=SessionStateId>)(.*?)(?=<)",
                uri = httpReqMsg.RequestUri.ToString(),
                httpReqMsg = httpReqMsg
            };

            slimCatRequestList.Add(slimCatReq);



            // 2. Upload front image


            return slimCatRequestList; 


        }



        private HttpRequestMessage AddGlobalRequestHeaders(HttpRequestMessage httpReqMsg)
        {
            httpReqMsg.Headers.Add("Accept", "*/*");
            httpReqMsg.Headers.Add("Accept-Language", "en-US");
            httpReqMsg.Headers.Add("Accept-Encoding", "GZIP");
            httpReqMsg.Headers.Add("Expect", "100-continue");
            httpReqMsg.Headers.Add("Connection", "Keep-Alive");

            return httpReqMsg;
        }



    }
}
