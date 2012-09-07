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
            Trace.TraceInformation("Rally.RestApi Post: {0}\r\n\tPost Data: {1}", target.ToString(), data);
            using (var webClient = GetWebClient(headers))
            {
                string response = webClient.UploadString(target, data);
                Trace.TraceInformation("Rally.RestApi Post Response: {0}\r\n", response);
                return response;
            }
        }

        public string Get(Uri target, IDictionary<string, string> headers = null)
        {
            Trace.TraceInformation("Rally.RestApi Get: {0}", target.ToString());
            using (var webClient = GetWebClient(headers))
            {
                string response = webClient.DownloadString(target);
                Trace.TraceInformation("Rally.RestApi Get Response: {0}\r\n", response);
                return response;
            }
        }

        public string Delete(Uri target, IDictionary<string, string> headers = null)
        {
            Trace.TraceInformation("Rally.RestApi Delete: {0}", target.ToString());
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
            var httpResponse = (HttpWebResponse)request.GetResponse();
            httpResponse.StatusCode.ToString();
            var enc = Encoding.ASCII;
            var responseStream = new StreamReader(httpResponse.GetResponseStream(), enc);
            var response = responseStream.ReadToEnd();
            responseStream.Close();
            Trace.TraceInformation("Rally.RestApi Delete Response: {0}\r\n", response);
            return response;
        }


    }
}
