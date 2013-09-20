using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Collections.Specialized;
using System.Web;
using System.Collections;
using System.Reflection;

namespace Rally.RestApi
{
    class SSOHelper
    {
        private CookieContainer cookieContainer = new CookieContainer();
        public Cookie jsessionidCookie = null;
        
        public SSOHelper()
        {
        }

        private CookieCollection getAllCookies(CookieContainer cookieJar)
        {
            CookieCollection cookieCollection = new CookieCollection();

            Hashtable table = (Hashtable) cookieJar.GetType().InvokeMember("m_domainTable",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            cookieJar,
                                                                            new object[] {});

            foreach (var tableKey in table.Keys)
            {
                String str_tableKey = (string) tableKey;

                if (str_tableKey[0] == '.')
                {
                    str_tableKey = str_tableKey.Substring(1);
                }

                SortedList list = (SortedList) table[tableKey].GetType().InvokeMember("m_list",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            table[tableKey],
                                                                            new object[] { });

                foreach (var listKey in list.Keys)
                {
                    String url = "https://" + str_tableKey + (string) listKey;
                    cookieCollection.Add(cookieJar.GetCookies(new Uri(url)));
                }
            }

            return cookieCollection;
        }
        
        private String makeDisplayableCookieString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Cookie cookie in getAllCookies(cookieContainer))
            {
                sb.AppendFormat("Name = {0}\nValue = {1}\nDomain = {2}\n\n", 
                    cookie.Name,
                    cookie.Value,
                    cookie.Domain);
            }
            return sb.ToString();
        }

        private Cookie getJsessionidCookie()
        {
            foreach (Cookie cookie in getAllCookies(cookieContainer))
            {
                if (cookie.Name.Equals("jsessionid", StringComparison.CurrentCultureIgnoreCase))
                    return cookie;
            }
            return null;
        }

        private GetResults performGet(Uri uri, bool useDefaultCredentials) 
        {
            GetResults results = new GetResults();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            request.AllowAutoRedirect = true;

            if (useDefaultCredentials)
                request.UseDefaultCredentials = true;

            request.CookieContainer = cookieContainer;
            request.Method = "GET";

            String requestHeaders = request.Headers.ToString();
            String responseHeaders = "";
            String cookiesBefore = makeDisplayableCookieString();
            String cookiesAfter = "";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    //System.Windows.Forms.MessageBox.Show("Good GET request");
                    results.statusCode = response.StatusCode;
                    results.responseUri = response.ResponseUri;
                    responseHeaders = response.Headers.ToString();
                    cookiesAfter = makeDisplayableCookieString();

                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            results.body = reader.ReadToEnd();
                        }
                    }
                }
            }
            finally
            {
                Trace.TraceInformation("SSO Get ({0}):\r\n{1}\r\n({2})\r\nRequest Headers:\r\n{3}Cookies Before:\r\n{4}\r\nResponse Headers:\r\n{5}Cookies After:\r\n{6}\r\nResponse Data:\r\n{7}",
                    results.statusCode.ToString(),
                    uri, 
                    request.Address,
                    requestHeaders,
                    cookiesBefore,
                    responseHeaders,
                    cookiesAfter,
                    results.body);
            }

            return results;
        }

        private class HttpResults
        {
            public String body;
            public HttpStatusCode statusCode;
            public Uri responseUri;
        }
    
        private class PostResults : HttpResults
        {
        }
    
        private class GetResults : HttpResults
        {
        }

        public class PostParam
        {
            public String name;
            public String value;
            public bool showInLog;

            public PostParam(String name, String value, bool showInLog)
            {
                this.name = name;
                this.value = value;
                this.showInLog = showInLog;
            }
        }
    
        private PostResults performPost(
            Uri uri,
            List<PostParam> postParams
        )
        {
            var results = new PostResults();
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            request.AllowAutoRedirect = true;

            request.CookieContainer = cookieContainer;
            request.Method = "POST";
            //request.Referer = "";
            request.ContentType = "application/x-www-form-urlencoded";

            StringBuilder sb = new StringBuilder();
            StringBuilder postParamString = new StringBuilder();
            foreach (PostParam param in postParams)
            {
                postParamString.AppendFormat("{0} = {1}\r\n",param.name,param.showInLog?param.value:"Not shown");
                sb.AppendFormat("{0}{1}={2}",sb.Length==0?"":"&",param.name,HttpUtility.UrlEncode(param.value));
            }
            byte[] buffer = Encoding.Default.GetBytes(sb.ToString());
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(buffer, 0, buffer.Length);
            }

            String requestHeaders = request.Headers.ToString();
            String responseHeaders = "";
            String cookiesBefore = makeDisplayableCookieString();
            String cookiesAfter = "";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    //System.Windows.Forms.MessageBox.Show("Good POST request");
                    results.statusCode = response.StatusCode;
                    results.responseUri = response.ResponseUri;
                    //results.redirectLocation = response.GetResponseHeader("Location");
                    responseHeaders = response.Headers.ToString();
                    cookiesAfter = makeDisplayableCookieString();

                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            results.body = reader.ReadToEnd();
                        }
                    }
                }
            }
            finally
            {
                Trace.TraceInformation("SSO Post ({0}):\r\n{1}\r\n({2})\r\nRequest Headers:\r\n{3}Cookies Before:\r\n{4}\r\nPost Params(Unencoded):\r\n{5}Response Headers:\r\n{6}Cookies After:\r\n{7}\r\nResponse Data\r\n{8}",
                    results.statusCode.ToString(),  
                    uri,
                    request.Address,
                    requestHeaders,
                    cookiesBefore,
                    postParamString,
                    responseHeaders,
                    cookiesAfter,
                    results.body);
            }

            return results;            
        }

        public FormInfo getFirstFormInfo(String htmlPage)
        {
            FormInfo formInfo = null;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlPage);
            HtmlNode formNode = doc.DocumentNode.SelectSingleNode("//form");
            if (formNode != null)
            {
                formInfo = new FormInfo(formNode.GetAttributeValue("action", ""));
                HtmlNodeCollection inputs = formNode.SelectNodes("//input");
                if (inputs != null && inputs.Count > 0)
                {
                    foreach (HtmlNode inputNode in inputs)
                    {
                        formInfo.inputs.Add(new InputInfo(inputNode.GetAttributeValue("type", ""),
                                                          inputNode.GetAttributeValue("name", ""),
                                                          inputNode.GetAttributeValue("value", "")));
                    }
                }
            }
            return formInfo;
        }

        public class FormInfo
        {
            public String actionUrl;
            public List<InputInfo> inputs = new List<InputInfo>();

            public FormInfo(String actionUrl)
            {
                this.actionUrl = actionUrl;
            }

            public Uri getAbsoluteUri(Uri baseUri)
            {
                Uri absoluteUri;
                if (!Uri.TryCreate(actionUrl, UriKind.Absolute, out absoluteUri)) 
                    absoluteUri = new Uri(baseUri, actionUrl);
                return absoluteUri;
            }
            
            public bool isPasswordForm()
            {
                return inputs.Any(x => x.type.Equals("password",StringComparison.CurrentCultureIgnoreCase));
            }

            public bool isSamlForm()
            {
                return !isPasswordForm() && inputs.Any(x => x.type.Equals("hidden",StringComparison.CurrentCultureIgnoreCase) );
            }

            public List<PostParam> getPasswordPostParams(String username, String password)
            {
                var postParams = new List<PostParam>();
                foreach (InputInfo inputInfo in inputs)
                {
                    if (inputInfo.type.Equals("password",StringComparison.CurrentCultureIgnoreCase))
                        postParams.Add(new PostParam(inputInfo.name,password,false));
                    else if (inputInfo.type.Equals("text",StringComparison.CurrentCultureIgnoreCase) ||
                             inputInfo.type.Equals("", StringComparison.CurrentCultureIgnoreCase))
                        postParams.Add(new PostParam(inputInfo.name,username,true));
                }
                return postParams;
            }

            public List<PostParam> getSamlPostParams()
            {
                var postParams = new List<PostParam>();
                foreach (InputInfo inputInfo in inputs)
                {
                    if (inputInfo.type.Equals("hidden",StringComparison.CurrentCultureIgnoreCase))
                        postParams.Add(new PostParam(inputInfo.name, inputInfo.value, true));
                }
                return postParams;
            }
        }

        public class InputInfo
        {
            public String type;
            public String name;
            public String value;

            public InputInfo(String type, String name, String value)
            {
                this.type = type;
                this.name = name;
                this.value = value;
            }
        }
        
        public bool doHandshake(
            Uri uri,
            String username,
            String password
        )
        {
            // initiate the handshake by GET on SSO url
            Uri activeUri = uri;
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(username))
                performGet(activeUri, true);  // Try to get an authentication cookie using network credentials
            HttpResults httpResults = performGet(activeUri, false);
            //Fix = what if null or no data?
        
            do 
            {
                if (httpResults.body == null)
                {
                    Trace.TraceError("No response returned during SSO handshake.  There should have been one.");
                    break;
                }

                // look for a form that will provide the next URL in the handshake sequence, which will also provide fields (such
                // as the SAML request from the SP and the SAML response from the IdP) that need to be forwarded on to the next URL

                FormInfo formInfo = getFirstFormInfo(httpResults.body);
                
                if (formInfo == null) 
                {
                    Trace.TraceError("No form detected during SSO handshake.  There should have been one.\r\n\r\n{0}", httpResults.body);
                    break; // no form detected, so this is the end of the handshake
                }

                // determine the form fields to post to the next URL; special handling takes place in the case of the password
                // form where we step in and provide the user's name / password to the SP 
                
                List<PostParam> postParams;
                if (formInfo.isPasswordForm())
                {
                    postParams = formInfo.getPasswordPostParams(username, password);
                }
                else if (formInfo.isSamlForm())
                {
                    postParams = formInfo.getSamlPostParams();
                }
                else
                {
                    Trace.TraceError("Unknown form encountered during handshake");
                    break;
                }

                // invoke the next step in the handshake
                activeUri = formInfo.getAbsoluteUri(httpResults.responseUri);
                PostResults postResults = performPost(activeUri, postParams);

                if (postResults.statusCode != HttpStatusCode.OK)
                {
                    Trace.TraceError("Unexpected status code from POST: " + postResults.statusCode.ToString());
                    break;
                }

                httpResults = postResults;

            } while ((jsessionidCookie = getJsessionidCookie()) == null);
        
            return jsessionidCookie != null;
        }

        public bool doHandshake(Uri uri)
        {
            return doHandshake(uri, null, null);
        }
    }
}
