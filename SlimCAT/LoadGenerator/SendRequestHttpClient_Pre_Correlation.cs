using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace SlimCAT
{
    public partial class SendHttpClientRequest
    {
        // This is a "post request" operation, but we must extract a value from a  request first before we can use the value. 
        // need to:
        // get the response
        // do extraction on it. 
        // hold this for upcoming request in the correlation dictionary
        public void ExtractText(SlimCatReq slimCatReq, Script scriptInstance, string responseBody)
        {
            // Regex for correlation is in the script. 
            // Correlation Hints:
            // 1. Copy the resultBody into https://rubular.com/
            // 2. left and right boundary basic format: (?<=  <left str>    )(.*?)(?=  < rt string> )
            // 3. Use https://onlinestringtools.com/escape-string to escape what you build in Rubular. 
            // 4. Remember that Visual Studio might display the response with backslashes. However, copying the response
            //    to notepad or rubular will shows that it does not actually contain back slashes. 
            Regex regEx = new Regex(slimCatReq.regExPattern);
            string extractedValue = regEx.Match(responseBody).Value;

            // add or update value in the correlation dictionary
            scriptInstance.correlationsDict[slimCatReq.correlatedVariableKeyName] = extractedValue;        

        }


        // check if the request needs to use a correlated value.
        // get value from correlations dictionary
        // modify request based on correlated value
        // ToDo: This could be simplifed for script side: If key is null don't go in here. 
        public SlimCatReq RebuildRequestWithCorrelatedValues(SlimCatReq slimCatReq, Script scriptInstance)
        {

            string reqContentStr;

            // Body Correlation - updating the body 
            if (slimCatReq.useExtractedBodyText == true)
            {
                string keyName = slimCatReq.correlatedVariableKeyName;
                reqContentStr = slimCatReq.httpReqMsg.Content.ReadAsStringAsync().Result;
                
                reqContentStr = Regex.Replace(reqContentStr, slimCatReq.regExPattern, scriptInstance.correlationsDict[keyName]);
                slimCatReq.httpReqMsg.Content = new StringContent(reqContentStr, Encoding.UTF8, "text/xml");

                var requestDebug = slimCatReq.httpReqMsg.Content.ReadAsStringAsync().Result;

                var newContent = new StringContent(requestDebug); // insert altered string back into request

                // I am not 100% sure we need this. Couldn't it be:
                // Content = new StringContent("", Encoding.UTF8, "application/json")
                // also here we are changing the Media type. We should be taking that from the script.
                // need something to test this on. 
                newContent.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                {
                    CharSet = Encoding.UTF8.WebName
                };
                slimCatReq.httpReqMsg.Content = newContent;
            }

            // URI correlation - updating the key
            if (slimCatReq.useExtractedUriText == true)
            {               
                string keyName = slimCatReq.correlatedVariableKeyName;
                string uriStr = slimCatReq.httpReqMsg.RequestUri.ToString();
                uriStr = uriStr.Replace("Correlated Value Not Initialized", scriptInstance.correlationsDict[keyName]);
                UriBuilder uriWithCorrelation = new UriBuilder(uriStr);
                slimCatReq.httpReqMsg.RequestUri = uriWithCorrelation.Uri; 
            }

            // ToDo: Header Correlation
            if (slimCatReq.useExtractedHeaderText == true)
            {

            }

                return slimCatReq; 
        }
    }
}
