using System;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using Rally.RestApi.Connection;
using Rally.RestApi.Json;
using Rally.RestApi.Auth;
using System.Collections.Specialized;
using Rally.RestApi.Exceptions;

namespace Rally.RestApi.Web
{
	internal class HttpService
	{
		const int MAX_RETRIES = 4;
		readonly CredentialCache credentials;
		readonly CookieContainer cookies = new CookieContainer();
		readonly ConnectionInfo connectionInfo;

		internal Uri Server { get; set; }
		private ApiAuthManager authManager;

		#region HttpService
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="authManager">The authorization manager to use.</param>
		/// <param name="connectionInfo">Connection Information</param>
		internal HttpService(ApiAuthManager authManager, ConnectionInfo connectionInfo)
		{
			if (authManager == null)
				throw new ArgumentNullException("authManager");

			if (connectionInfo == null)
				throw new ArgumentNullException("connectionInfo");

			this.authManager = authManager;
			this.connectionInfo = connectionInfo;

			if (connectionInfo.AuthType == AuthorizationType.Basic)
			{
				Server = connectionInfo.Server;
				credentials = new CredentialCache { { connectionInfo.Server, "Basic", new NetworkCredential(connectionInfo.UserName, connectionInfo.Password) } };
			}
			else if (connectionInfo.AuthType == AuthorizationType.ZSessionID)
			{
				Server = connectionInfo.Server;
				credentials = null;
			}
			else if (connectionInfo.AuthType == AuthorizationType.ApiKey)
			{
				Server = connectionInfo.Server;
				credentials = new CredentialCache { { connectionInfo.Server, "Basic", new NetworkCredential(connectionInfo.ApiKey, connectionInfo.ApiKey) } };
			}
		}
		#endregion

		#region GetWebClient
		WebClient GetWebClient(IEnumerable<KeyValuePair<string, string>> headers = null, bool isCacheable = false, bool isSsoCheck = false)
		{
			CookieAwareWebClient webClient;
			if (isCacheable)
				webClient = new CookieAwareCacheableWebClient(cookies);
			else if (isSsoCheck)
				webClient = new SsoWebClient(cookies);
			else
				webClient = new CookieAwareWebClient(cookies);

			if (connectionInfo.AuthType == AuthorizationType.ApiKey)
				webClient.Headers.Add("ZSESSIONID", connectionInfo.ApiKey);
			else if (connectionInfo.AuthType == AuthorizationType.ZSessionID)
				webClient.AddCookie(connectionInfo.Server, "ZSESSIONID", connectionInfo.ZSessionID);

			webClient.Encoding = Encoding.UTF8;
			if (headers != null)
			{
				foreach (var pairs in headers)
					webClient.Headers.Add(pairs.Key, pairs.Value);
			}

			if (credentials != null)
				webClient.Credentials = credentials;
			if (connectionInfo.Proxy != null)
				webClient.Proxy = connectionInfo.Proxy;
			return webClient;
		}
		#endregion

		#region Download
		internal byte[] Download(Uri target, IDictionary<string, string> headers = null)
		{
			byte[] response = null;
			DateTime startTime = DateTime.Now;
			String requestHeaders = "";
			String responseHeaders = "";
			String cookiesBefore = "";
			String cookiesAfter = "";
			try
			{
				using (var webClient = GetWebClient(headers))
				{
					if ((connectionInfo.AuthType == AuthorizationType.ZSessionID) &&
						(target.ToString().EndsWith(RallyRestApi.SECURITY_ENDPOINT)))
					{
						// Sending blank username
						string auth = string.Format(":{0}", connectionInfo.ZSessionID);
						string enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
						string cred = string.Format("{0} {1}", "Basic", enc);
						webClient.Headers.Add(HttpRequestHeader.Authorization, cred);
					}

					cookiesBefore = MakeDisplayableCookieString(cookies);
					requestHeaders = webClient.Headers.ToString();
					response = webClient.DownloadData(target);
					cookiesAfter = MakeDisplayableCookieString(cookies);
					responseHeaders = webClient.ResponseHeaders.ToString();
					return response;
				}
			}
			finally
			{
				Trace.TraceInformation("Get ({0}):\r\n{1}\r\nRequest Headers:\r\n{2}Cookies Before:\r\n{3}Response Headers:\r\n{4}Cookies After:\r\n{5}Response Data\r\n{6}",
															 DateTime.Now.Subtract(startTime).ToString(),
															 target.ToString(),
															 requestHeaders,
															 cookiesBefore,
															 responseHeaders,
															 cookiesAfter,
															 response);
			}
		}
		#endregion

		#region Post
		internal string Post(Uri target, string data, IDictionary<string, string> headers = null)
		{
			String response = "<No response>";
			DateTime startTime = DateTime.Now;
			String requestHeaders = "";
			String responseHeaders = "";
			String cookiesBefore = "";
			String cookiesAfter = "";

			try
			{
				using (var webClient = GetWebClient(headers))
				{
					cookiesBefore = MakeDisplayableCookieString(cookies);
					requestHeaders = webClient.Headers.ToString();
					response = webClient.UploadString(target, data);
					cookiesAfter = MakeDisplayableCookieString(cookies);
					responseHeaders = webClient.ResponseHeaders.ToString();
					return response;
				}
			}
			finally
			{
				Trace.TraceInformation("Post ({0}):\r\n{1}\r\nRequest Headers:\r\n{2}Cookies Before:\r\n{3}Request Data:\r\n{4}\r\nResponse Headers:\r\n{5}Cookies After:\r\n{6}Response Data\r\n{7}",
															 DateTime.Now.Subtract(startTime).ToString(),
															 target.ToString(),
															 requestHeaders,
															 cookiesBefore,
															 data,
															 responseHeaders,
															 cookiesAfter,
															 response);
			}
		}
		#endregion

		#region GetAsPost
		internal string GetAsPost(Uri target, IDictionary<string, string> data, IDictionary<string, string> headers = null)
		{
			String response = "<No response>";
			DateTime startTime = DateTime.Now;
			String requestHeaders = "";
			String responseHeaders = "";
			String cookiesBefore = "";
			String cookiesAfter = "";
			try
			{
				using (var webClient = GetWebClient(headers))
				{
					if ((connectionInfo.AuthType == AuthorizationType.ZSessionID) &&
						(target.ToString().EndsWith(RallyRestApi.SECURITY_ENDPOINT)))
					{
						// Sending blank username
						string auth = string.Format(":{0}", connectionInfo.ZSessionID);
						string enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
						string cred = string.Format("{0} {1}", "Basic", enc);
						webClient.Headers.Add(HttpRequestHeader.Authorization, cred);
					}

					NameValueCollection requestParams = new NameValueCollection();
					requestParams.Add("_method", "GET");
					foreach (string key in data.Keys)
						requestParams.Add(key, data[key]);

					cookiesBefore = MakeDisplayableCookieString(cookies);
					requestHeaders = webClient.Headers.ToString();
					byte[] responsebytes = webClient.UploadValues(target, "POST", requestParams);
					cookiesAfter = MakeDisplayableCookieString(cookies);
					responseHeaders = webClient.ResponseHeaders.ToString();
					response = Encoding.UTF8.GetString(responsebytes);
					return response;
				}
			}
			finally
			{
				Trace.TraceInformation("Post ({0}):\r\n{1}\r\nRequest Headers:\r\n{2}Cookies Before:\r\n{3}Request Data:\r\n{4}\r\nResponse Headers:\r\n{5}Cookies After:\r\n{6}Response Data\r\n{7}",
															 DateTime.Now.Subtract(startTime).ToString(),
															 target.ToString(),
															 requestHeaders,
															 cookiesBefore,
															 data,
															 responseHeaders,
															 cookiesAfter,
															 response);
			}
		}
		#endregion

		#region Get
		internal string Get(Uri target, IDictionary<string, string> headers = null)
		{
			String response = "<No response>";
			DateTime startTime = DateTime.Now;
			String requestHeaders = "";
			String responseHeaders = "";
			String cookiesBefore = "";
			String cookiesAfter = "";

			try
			{
				using (var webClient = GetWebClient(headers))
				{
					if (connectionInfo.AuthType == AuthorizationType.ZSessionID)
					{
						// Sending blank user name
						string auth = string.Format(":{0}", connectionInfo.ZSessionID);
						string enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
						string cred = string.Format("{0} {1}", "Basic", enc);
						webClient.Headers.Add(HttpRequestHeader.Authorization, cred);
					}

					cookiesBefore = MakeDisplayableCookieString(cookies);
					requestHeaders = webClient.Headers.ToString();
					response = webClient.DownloadString(target);
					cookiesAfter = MakeDisplayableCookieString(cookies);
					responseHeaders = webClient.ResponseHeaders.ToString();
					return response;
				}
			}
			finally
			{
				Trace.TraceInformation("Get ({0}):\r\n{1}\r\nRequest Headers:\r\n{2}Cookies Before:\r\n{3}Response Headers:\r\n{4}Cookies After:\r\n{5}Response Data\r\n{6}",
															 DateTime.Now.Subtract(startTime).ToString(),
															 target.ToString(),
															 requestHeaders,
															 cookiesBefore,
															 responseHeaders,
															 cookiesAfter,
															 response);
			}
		}
		#endregion

		#region GetCacheable
		/// <summary>
		/// Gets a cacheable response.
		/// </summary>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		internal DynamicJsonObject GetCacheable(Uri target, out bool isCachedResult, IDictionary<string, string> headers = null)
		{
			DynamicJsonObject response = null;
			DateTime startTime = DateTime.Now;
			String requestHeaders = "";
			String responseHeaders = "";
			try
			{
				using (var webClient = GetWebClient(headers, true))
				{
					requestHeaders = webClient.Headers.ToString();
					CookieAwareCacheableWebClient cacheableWeb = webClient as CookieAwareCacheableWebClient;
					if (cacheableWeb != null)
					{
						response = cacheableWeb.DownloadCacheableResult(target, out isCachedResult);
					}
					else
						throw new InvalidOperationException("GetWebClient failed to create a CookieAwareCacheableWebClient");

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
		#endregion

		#region Delete
		internal string Delete(Uri target, IDictionary<string, string> headers = null)
		{
			String response = "<No response>";
			DateTime startTime = DateTime.Now;
			String requestHeaders = "";
			String responseHeaders = "";
			String cookiesBefore = "";
			String cookiesAfter = "";

			try
			{
				var request = WebRequest.Create(target) as HttpWebRequest;
				request.Method = "DELETE";
				request.CookieContainer = cookies;
				request.Credentials = credentials;
				if (connectionInfo.AuthType == AuthorizationType.ApiKey)
					request.Headers.Add("ZSESSIONID", connectionInfo.ApiKey);

				if (headers != null)
				{
					foreach (var pairs in headers)
					{
						request.Headers.Add(pairs.Key, pairs.Value);
					}
				}
				cookiesBefore = MakeDisplayableCookieString(cookies);
				requestHeaders = request.Headers.ToString();
				var httpResponse = (HttpWebResponse)request.GetResponse();
				cookiesAfter = MakeDisplayableCookieString(cookies);
				responseHeaders = httpResponse.Headers.ToString();
				if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
				{
					throw new WebException("Unauthorized", null, WebExceptionStatus.TrustFailure, httpResponse);
				}
				var enc = Encoding.ASCII;
				var responseStream = new StreamReader(httpResponse.GetResponseStream(), enc);
				response = responseStream.ReadToEnd();
				responseStream.Close();
				return response;
			}
			finally
			{
				Trace.TraceInformation("Delete ({0}):\r\n{1}\r\nRequest Headers:\r\n{2}Cookies Before:\r\n{3}Response Headers:\r\n{4}Cookies After:\r\n{5}Response Data\r\n{6}",
															 DateTime.Now.Subtract(startTime).ToString(),
															 target.ToString(),
															 requestHeaders,
															 cookiesBefore,
															 responseHeaders,
															 cookiesAfter,
															 response);
			}
		}
		#endregion

		#region MakeDisplayableCookieString
		private String MakeDisplayableCookieString(CookieContainer cookieContainer)
		{
			StringBuilder sb = new StringBuilder();
			foreach (Cookie cookie in GetAllCookies(cookieContainer))
			{
				sb.AppendFormat("Name = {0}\nValue = {1}\nDomain = {2}\n\n",
						cookie.Name,
						cookie.Value,
						cookie.Domain);
			}
			return sb.ToString();
		}
		#endregion

		#region GetAllCookies
		private CookieCollection GetAllCookies(CookieContainer cookieJar)
		{
			CookieCollection cookieCollection = new CookieCollection();

			Hashtable table = (Hashtable)cookieJar.GetType().InvokeMember("m_domainTable",
																																			BindingFlags.NonPublic |
																																			BindingFlags.GetField |
																																			BindingFlags.Instance,
																																			null,
																																			cookieJar,
																																			new object[] { });

			foreach (var tableKey in table.Keys)
			{
				String str_tableKey = (string)tableKey;

				if (str_tableKey[0] == '.')
				{
					str_tableKey = str_tableKey.Substring(1);
				}

				SortedList list = (SortedList)table[tableKey].GetType().InvokeMember("m_list",
																																		BindingFlags.NonPublic |
																																		BindingFlags.GetField |
																																		BindingFlags.Instance,
																																		null,
																																		table[tableKey],
																																		new object[] { });

				foreach (var listKey in list.Keys)
				{
					String url = "https://" + str_tableKey + (string)listKey;
					cookieCollection.Add(cookieJar.GetCookies(new Uri(url)));
				}
			}

			return cookieCollection;
		}
		#endregion

		#region PerformSsoAuthentication
		/// <summary>
		/// Performs SSO Authentication
		/// </summary>
		/// <returns></returns>
		internal bool PerformSsoAuthentication()
		{
			if ((authManager == null) || (!authManager.IsUiSupported))
				return false;

			ConnectionInfo ssoConnection = new ConnectionInfo();
			ssoConnection.AuthType = AuthorizationType.Basic;
			ssoConnection.Server = new Uri(String.Format("{0}login/key.js", connectionInfo.Server.AbsoluteUri));
			ssoConnection.Port = connectionInfo.Port;
			ssoConnection.Proxy = connectionInfo.Proxy;
			ssoConnection.UserName = connectionInfo.UserName;

			HttpService ssoService = new HttpService(authManager, ssoConnection);
			Uri ssoRedirectUri = ssoConnection.Server;
			if (ssoService.PerformSsoCheck(out ssoRedirectUri))
			{
				authManager.OpenSsoPage(ssoRedirectUri);

				return true;
			}

			return false;
		}
		#endregion

		#region PerformSsoCheck
		internal bool PerformSsoCheck(out Uri ssoRedirectUri, IDictionary<string, string> headers = null)
		{
			using (var webClient = GetWebClient(headers, isSsoCheck: true))
			{
				SsoWebClient ssoWebClient = webClient as SsoWebClient;
				if (ssoWebClient != null)
				{
					try
					{
						if (ssoWebClient.CheckIfRedirect(connectionInfo.Server, connectionInfo.UserName))
						{
							string spacerChar = "?";
							if (ssoWebClient.RedirectTo.Contains("?"))
								spacerChar = "&";

							string portInfo = String.Empty;
							if (connectionInfo.Port > 0)
								portInfo = String.Format(":{0}", connectionInfo.Port);

							ssoRedirectUri = new Uri(String.Format("{0}{1}TargetResource={2}://{3}{4}/slm/j_sso_security_check?noRedirect=true",
								ssoWebClient.RedirectTo, spacerChar, connectionInfo.Server.Scheme, connectionInfo.Server.Host, portInfo));

							return true;
						}
					}
					catch (WebException we)
					{
						if ((we.Response != null) &&
							(((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.MethodNotAllowed))
						{
							ssoRedirectUri = null;
							return false;
						}

						throw;
					}

				}
				else
					throw new InvalidOperationException("GetWebClient failed to create a SsoWebClient");
			}

			ssoRedirectUri = null;
			return false;
		}
		#endregion
	}
}
