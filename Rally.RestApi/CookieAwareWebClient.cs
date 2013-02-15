using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Rally.RestApi
{
    internal class CookieAwareWebClient : WebClient
    {
        private readonly CookieContainer cookies;

        public CookieAwareWebClient(CookieContainer cookies = null)
            : base()
        {
            this.cookies = cookies ?? new CookieContainer();
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            HttpWebRequest webRequest = request as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.CookieContainer = cookies;
            }

            request.Timeout = 300000;

            return request;
        }
    }
}
