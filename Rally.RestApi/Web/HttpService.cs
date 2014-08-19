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
using Rally.RestApi.Sso;

namespace Rally.RestApi.Web
{
	internal class HttpService
	{
		const int MAX_RETRIES = 4;
		readonly CredentialCache credentials;
		readonly CookieContainer cookies = new CookieContainer();
		readonly ConnectionInfo connectionInfo;

		internal Uri Server { get; set; }
		/// <summary>
		/// An event that indicates changes to SSO authentication.
		/// </summary>
		internal event SsoResults SsoResults;
		private ISsoDriver ssoDriver;

		#region HttpService
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ssoDriver">The SSO driver to use.</param>
		/// <param name="connectionInfo">Connection Information</param>
		internal HttpService(ISsoDriver ssoDriver, ConnectionInfo connectionInfo)
		{
			if (ssoDriver == null)
				throw new ArgumentNullException("ssoDriver");

			if (connectionInfo == null)
				throw new ArgumentNullException("connectionInfo");

			this.ssoDriver = ssoDriver;
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
				webClient.AddCookie(connectionInfo.Server, "ZSESSIONID", connectionInfo.ApiKey);
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

		#region Post
		internal string Post(Uri target, string data, IDictionary<string, string> headers = null)
		{
			String response = "<No response>";
			DateTime startTime = DateTime.Now;
			String requestHeaders = "";
			String responseHeaders = "";
			String cookiesBefore = "";
			String cookiesAfter = "";
			int retries = 0;

			do
			{
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
				catch (WebException e)
				{
					if ((e.Response != null) &&
						(((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized) &&
						(connectionInfo.AuthType == AuthorizationType.ZSessionID))
					{
						if (retries > MAX_RETRIES)
						{
							Trace.TraceWarning("Got Unauthorized response code ({0}} more than {1} times in a row. Failing.", HttpStatusCode.Unauthorized, retries);
							throw;
						}

						Trace.TraceWarning("Got Unauthorized response code ({0}). Re-authorizing using SSO.", HttpStatusCode.Unauthorized);
						PerformSsoAuthentication();
						continue;
					}
					throw;
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
			} while (true);
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
			int retries = 0;

			do
			{
				try
				{
					using (var webClient = GetWebClient(headers))
					{
						cookiesBefore = MakeDisplayableCookieString(cookies);
						requestHeaders = webClient.Headers.ToString();
						response = webClient.DownloadString(target);
						cookiesAfter = MakeDisplayableCookieString(cookies);
						responseHeaders = webClient.ResponseHeaders.ToString();
						return response;
					}
				}
				catch (WebException e)
				{
					if ((e.Response != null) &&
						(((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized) &&
						(connectionInfo.AuthType == AuthorizationType.ZSessionID))
					{
						if (retries > MAX_RETRIES)
						{
							Trace.TraceWarning("Got Unauthorized response code ({0}} more than {1} times in a row. Failing.", HttpStatusCode.Unauthorized, retries);
							throw;
						}

						Trace.TraceWarning("Got Unauthorized response code ({0}). Re-authorizing using SSO.", HttpStatusCode.Unauthorized);
						PerformSsoAuthentication();
						continue;
					}
					throw;
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
			} while (true);
		}
		#endregion

		#region GetCacheable
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
			int retries = 0;

			do
			{
				try
				{
					var request = WebRequest.Create(target) as HttpWebRequest;
					request.Method = "DELETE";
					request.CookieContainer = cookies;
					request.Credentials = credentials;
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
						if (retries > MAX_RETRIES)
						{
							Trace.TraceWarning("Got Unauthorized response code ({0}} more than {1} times in a row. Failing.", HttpStatusCode.Unauthorized, retries);
							throw new WebException("Unauthorized", null, WebExceptionStatus.TrustFailure, httpResponse);
						}

						Trace.TraceWarning("Got Unauthorized response code ({0}). Re-authorizing using SSO.", HttpStatusCode.Unauthorized);
						PerformSsoAuthentication();
						continue;
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
			} while (true);
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
			if ((ssoDriver == null) || (!ssoDriver.IsSsoAuthorized))
				return false;

			ConnectionInfo ssoConnection = new ConnectionInfo();
			ssoConnection.AuthType = AuthorizationType.Basic;
			ssoConnection.Server = new Uri(String.Format("{0}login/key.js", connectionInfo.Server.AbsoluteUri));
			ssoConnection.Port = connectionInfo.Port;
			ssoConnection.Proxy = connectionInfo.Proxy;
			ssoConnection.UserName = connectionInfo.UserName;

			HttpService ssoService = new HttpService(ssoDriver, ssoConnection);
			Uri ssoRedirectUri = ssoConnection.Server;
			if (ssoService.PerformSsoCheck(out ssoRedirectUri))
			{
				ssoDriver.SsoResults += SsoCompleted;
				ssoDriver.ShowSsoPage(ssoRedirectUri);

				return true;
			}

			return false;
		}
		#endregion

		#region SsoCompleted
		private void SsoCompleted(bool success, string zSessionID)
		{
			if (SsoResults != null)
				SsoResults.Invoke(success, zSessionID);
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
