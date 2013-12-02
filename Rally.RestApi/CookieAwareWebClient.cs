using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Rally.RestApi
{
	[System.ComponentModel.DesignerCategory("")]
	internal class CookieAwareWebClient : WebClient
	{
		protected CookieContainer Cookies { get; private set; }

		public CookieAwareWebClient(CookieContainer cookies = null)
			: base()
		{
			this.Cookies = cookies ?? new CookieContainer();
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);

			HttpWebRequest webRequest = request as HttpWebRequest;
			if (webRequest != null)
			{
				webRequest.CookieContainer = Cookies;
			}

			request.Timeout = 300000;

			return request;
		}
	}
}
