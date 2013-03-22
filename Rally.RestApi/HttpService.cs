using System;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace Rally.RestApi
{
    internal class HttpService
    {

        /// <summary>
        /// The character encoding that will be used for the requests.
        /// </summary>
        public Encoding Encoding { get; set; }

        readonly CredentialCache credentials;

        readonly CookieContainer cookies;

        /// <summary>
        /// The optional proxy configuration
        /// </summary>
        public WebProxy Proxy { get; set; }

        internal Uri Server { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username">Rally Username</param>
        /// <param name="password">Rally Password</param>
        /// <param name="serverUrl" > Server url defaults to <value>http://rally1.rallydev.com</value></param>
        /// <param name="proxy">Optional proxy config</param>
        public HttpService(string username, string password, string serverUrl = "http://rally1.rallydev.com",
            WebProxy proxy = null)
            : this(username, password, new Uri(serverUrl), proxy)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username">Rally Username</param>
        /// <param name="password">Rally Password</param>
        /// <param name="serverUrl" > Server url defaults to <value>http://rally1.rallydev.com</value></param>
        /// <param name="proxy">Optional proxy configuration</param> 
        public HttpService(string username, string password, Uri serverUrl, WebProxy proxy = null)
        {
            Server = serverUrl;
            Encoding = Encoding.UTF8;
            Proxy = proxy;
            credentials = new CredentialCache { { serverUrl, "Basic", new NetworkCredential(username, password) } };
            cookies = new CookieContainer();
        }

        WebClient GetWebClient(IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            var webClient = new CookieAwareWebClient(cookies);
            if (headers != null)
            {
                foreach (var pairs in headers)
                {
                    webClient.Headers.Add(pairs.Key, pairs.Value);
                }
            }
            webClient.Encoding = Encoding;
            webClient.Credentials = credentials;

            if (Proxy != null)
            {
                webClient.Proxy = Proxy;
            }
            return webClient;
        }

        public string Post(Uri target, string data, IDictionary<string, string> headers = null)
        {
            String response = "<No response>";
            DateTime startTime = DateTime.Now;
            String requestHeaders = "";
            String responseHeaders = "";
            try
            {
                using (var webClient = GetWebClient(headers))
                {
                    requestHeaders = webClient.Headers.ToString();
                    response = webClient.UploadString(target, data);
                    responseHeaders = webClient.ResponseHeaders.ToString();
                    return response;
                }
            }
            finally
            {
                Trace.TraceInformation("Post ({0}):\r\n{1}\r\nRequest Headers:\r\n{2}Request Data:\r\n{3}\r\nResponse Headers:\r\n{4}Response Data\r\n{5}",
                                       DateTime.Now.Subtract(startTime).ToString(),
                                       target.ToString(),
                                       requestHeaders,
                                       data,
                                       responseHeaders,
                                       response);
            }
        }

        public string Get(Uri target, IDictionary<string, string> headers = null)
        {
            String response = "<No response>";
            DateTime startTime = DateTime.Now;
            String requestHeaders = "";
            String responseHeaders = "";
            try
            {
                using (var webClient = GetWebClient(headers))
                {
                    requestHeaders = webClient.Headers.ToString();
                    response = webClient.DownloadString(target);
                    responseHeaders = webClient.ResponseHeaders.ToString();
                    return response;
                }
            }
            finally
            {
                Trace.TraceInformation("Get ({0}):\r\n{1}\r\nRequest Headers:\r\n{2}Response Headers:\r\n{3}Response Data\r\n{4}",
                                       DateTime.Now.Subtract(startTime).ToString(),  
                                       target.ToString(), 
                                       requestHeaders, 
                                       responseHeaders,
                                       response);
            }
        }

        public string Delete(Uri target, IDictionary<string, string> headers = null)
        {
            String response = "<No response>";
            DateTime startTime = DateTime.Now;
            String requestHeaders = "";
            String responseHeaders = "";
            try
            {
                var request = WebRequest.Create(target);
                request.Method = "DELETE";
                request.Credentials = credentials;
                if (headers != null)
                {
                    foreach (var pairs in headers)
                    {
                        request.Headers.Add(pairs.Key, pairs.Value);
                    }
                }
                requestHeaders = request.Headers.ToString();
                var httpResponse = (HttpWebResponse)request.GetResponse();
                responseHeaders = httpResponse.Headers.ToString();
                httpResponse.StatusCode.ToString();
                var enc = Encoding.ASCII;
                var responseStream = new StreamReader(httpResponse.GetResponseStream(), enc);
                response = responseStream.ReadToEnd();
                responseStream.Close();
                return response;
            }
            finally
            {
                Trace.TraceInformation("Delete ({0}):\r\n{1}\r\nRequest Headers:\r\n{2}Response Headers:\r\n{3}Response Data\r\n{4}",
                                       DateTime.Now.Subtract(startTime).ToString(),
                                       target.ToString(),
                                       requestHeaders,
                                       responseHeaders,
                                       response);
            }
        }
    }
}
