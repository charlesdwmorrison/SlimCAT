using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace SlimCAT
{
    /// <summary>
    /// // https://stackoverflow.com/questions/37214345/rapid-web-requests-to-many-different-websites-using-httpclient-c-sharp
    /// </summary>
    public partial class SendHttpClientRequest: IDisposable
    {
        private  TimeSpan _timeout;
        private HttpClientHandler _httpClientHandler;
        private readonly string _baseUrl;
        private const string ClientUserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
        
        public void Dispose()
        {
            _httpClientHandler?.Dispose();
            _httpClient?.Dispose();
        }

        public void CreateHttpClient(TimeSpan? timeout = null)
        {
            //_baseUrl = NormalizeBaseUrl(baseUrl);
            _timeout = timeout ?? TimeSpan.FromSeconds(90);
            var debug0 = UserController.numThreads;

            //ToDo: Put back in
            //AddCertificates addCerts = new AddCertificates();
            //    X509Certificate2Collection x509Cert2coll = addCerts.LoadCertificates();

           var  debug1 = UserController.numThreads;
            _httpClientHandler = new HttpClientHandler
            {
                //AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip

            };
            //_httpClientHandler.ClientCertificates.AddRange(x509Cert2coll);

            _httpClient = new HttpClient(_httpClientHandler, false)
            {
                 Timeout = _timeout
            };

            // 10/12/2021 - We can add and remove headers more easily if we do it at the request level.
            //_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ClientUserAgent);

            if (!string.IsNullOrWhiteSpace(_baseUrl))
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }

            var debug = UserController.numThreads;

        }

    }
}
