using Rally.RestApi.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Rally.RestApi.Web
{
	[System.ComponentModel.DesignerCategory("")]
	internal class SsoWebClient : CookieAwareWebClient
	{
		private readonly DynamicJsonSerializer serializer = new DynamicJsonSerializer();
		public string RedirectTo { get; private set; }

		public SsoWebClient(CookieContainer cookies = null)
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

		internal bool CheckIfRedirect(Uri address, string userName)
		{
			UploadString(address, "PUT", String.Format("{{\"username\": \"{0}\", \"password\": \"\"}}", userName));

			return (!String.IsNullOrWhiteSpace(RedirectTo));
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			request.ContentType = "application/json";

			WebResponse response = base.GetWebResponse(request);
			HttpWebResponse webResponse = response as HttpWebResponse;
			if (webResponse != null)
			{
				// Check to see if it's a redirect
				if ((int)webResponse.StatusCode >= 300 && (int)webResponse.StatusCode <= 399)
				{
					RedirectTo = webResponse.Headers["Location"];
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
	}
}
