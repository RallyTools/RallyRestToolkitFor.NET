﻿using Rally.RestApi.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Rally.RestApi.Web
{
	[System.ComponentModel.DesignerCategory("")]
	internal class CookieAwareCacheableWebClient : CookieAwareWebClient
	{
		private readonly DynamicJsonSerializer serializer = new DynamicJsonSerializer();
		private class CachedResult
		{
			public string Url { get; set; }
			public DynamicJsonObject ResponseData { get; set; }

			public CachedResult(string redirectUrl, DynamicJsonObject responseData)
			{
				Url = redirectUrl;
				ResponseData = responseData;
			}
		}

		// Tracking Key: Username|SourceUrl
		private static Dictionary<string, CachedResult> cachedResults;
		private static object dataLock;
		private CachedResult returnValue = null;
		private bool isCachedResult = false;

		static CookieAwareCacheableWebClient()
		{
			cachedResults = new Dictionary<string, CachedResult>();
			dataLock = new object();
		}

		public CookieAwareCacheableWebClient(CookieContainer cookies = null)
			: base(cookies)
		{
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);
			HttpWebRequest webRequest = request as HttpWebRequest;
			if (webRequest != null)
			{
				// IMPORTANT: 
				// Don't allow redirect as it prevents us from disovering if we 
				// are using a cached endpoint, or have a new result set.
				webRequest.AllowAutoRedirect = false;
			}

			return request;
		}

		/// <summary>
		/// Downloads the requested resource as a System.String. The resource to download is 
		/// specified as a System.String containing the URI.
		/// </summary>
		/// <param name="address">A System.String containing the URI to download.</param>
		/// <param name="isCachedResult">If the returned result was a cached result.</param>
		/// <returns>A System.String containing the requested resource.</returns>
		public DynamicJsonObject DownloadCacheableResult(string address, out bool isCachedResult)
		{
			return DownloadCacheableResult(new Uri(address), out isCachedResult);
		}

		/// <summary>
		/// Downloads the requested resource as a System.String. The resource to 
		/// download is specified as a System.Uri.
		/// </summary>
		/// <param name="address">A System.Uri object containing the URI to download.</param>
		/// <param name="isCachedResult">If the returned result was a cached result.</param>
		/// <returns>A System.String containing the requested resource.</returns>
		public DynamicJsonObject DownloadCacheableResult(Uri address, out bool isCachedResult)
		{
			string results = DownloadString(address);

			isCachedResult = this.isCachedResult;
			if (returnValue != null)
				return returnValue.ResponseData;

			return serializer.Deserialize(results);
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			WebResponse response = base.GetWebResponse(request);
			HttpWebResponse webResponse = response as HttpWebResponse;
			if (webResponse != null)
			{
				// Check to see if it's a redirect
				if ((int)webResponse.StatusCode >= 300 && (int)webResponse.StatusCode <= 399)
				{
					string uriString = webResponse.Headers["Location"];
					string cachingKey = String.Empty;
					if (request.Credentials != null)
					{
						NetworkCredential credential = request.Credentials.GetCredential(request.RequestUri, "Basic");
						cachingKey = credential.UserName;
					}
					else
					{
						CookieCollection cookieCollection = Cookies.GetCookies(request.RequestUri);
						foreach (Cookie currentCookie in cookieCollection)
						{
							if (currentCookie.Name.Equals(RallyRestApi.ZSessionID, StringComparison.InvariantCultureIgnoreCase))
							{
								cachingKey = currentCookie.Value;
								break;
							}
						}
					}

					returnValue = GetCachedResult(cachingKey, request.RequestUri.ToString());
					if ((returnValue == null) || (!returnValue.Url.Equals(uriString)))
					{
						if (returnValue != null)
						{
							ClearCacheResult(cachingKey, request.RequestUri.ToString());
							returnValue = null;
						}

						CookieAwareWebClient webClient = GetWebClient();

						string cacheableDataValue = webClient.DownloadString(uriString);
						returnValue = CacheResult(cachingKey, request.RequestUri.ToString(), uriString, serializer.Deserialize(cacheableDataValue));
					}
					else
						isCachedResult = true;
				}
			}

			return response;
		}

		private CookieAwareWebClient GetWebClient()
		{
			CookieAwareWebClient webClient = new CookieAwareWebClient(Cookies);

			foreach (string key in webClient.Headers.Keys)
				webClient.Headers.Add(key, webClient.Headers[key]);

			webClient.Encoding = Encoding;
			webClient.Credentials = Credentials;

			if (Proxy != null)
			{
				webClient.Proxy = Proxy;
			}

			return webClient;
		}

		private CachedResult CacheResult(string userName, string sourceUrl, string redirectUrl, DynamicJsonObject responseData)
		{
			string cacheKey = GetCacheKey(userName, sourceUrl);
			CachedResult cachedResult = new CachedResult(redirectUrl, responseData);
			lock (dataLock)
			{
				if (cachedResults.ContainsKey(cacheKey))
					cachedResults[cacheKey] = cachedResult;
				else
					cachedResults.Add(cacheKey, cachedResult);
			}

			return cachedResult;
		}

		private CachedResult GetCachedResult(string userName, string sourceUrl)
		{
			string cacheKey = GetCacheKey(userName, sourceUrl);
			lock (dataLock)
			{
				if (cachedResults.ContainsKey(cacheKey))
					return cachedResults[cacheKey];
				else
					return null;
			}
		}

		private void ClearCacheResult(string userName, string sourceUrl)
		{
			string cacheKey = GetCacheKey(userName, sourceUrl);
			lock (dataLock)
			{
				if (cachedResults.ContainsKey(cacheKey))
					cachedResults.Remove(cacheKey);
			}
		}

		private string GetCacheKey(string userName, string sourceUrl)
		{
			return String.Format("{0}|{1}", userName, sourceUrl);
		}
	}
}
