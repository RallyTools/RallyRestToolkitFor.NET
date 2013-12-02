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
        public static Cookie parsSSOLandingPage(String htmlPage)
        {
            Cookie authCookie = null;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlPage);
            HtmlNodeCollection inputs = doc.DocumentNode.SelectNodes("//input");
            if (inputs != null && inputs.Count > 0)
            {
                authCookie = new Cookie();

                foreach (HtmlNode inputNode in inputs)
                {
                    switch (inputNode.GetAttributeValue("name", ""))
                    {
                        case "authCookieName":
                            authCookie.Name = inputNode.GetAttributeValue("value", "");
                            continue;
                        case "authCookieValue":
                            authCookie.Value = inputNode.GetAttributeValue("value", "");
                            continue;
                        case "authCookieDomain":
                            String domain = inputNode.GetAttributeValue("value", "");
                            authCookie.Domain = domain == null || domain == "null" ?  "" : domain;
                            continue;
                        case "authCookiePath":
                            String path = inputNode.GetAttributeValue("value", "");
                            authCookie.Path = path == null || path == "null" ? "" : path;
                            continue;
                    }
                }

                if (String.IsNullOrWhiteSpace(authCookie.Name) ||
                    String.IsNullOrWhiteSpace(authCookie.Value))
                {
                    authCookie = null;
                }
            }

            if (authCookie == null)
            {
                Trace.TraceWarning("Unable to parse SSO landing page.\n{0}",htmlPage);
                throw new Exception("Unable to parse SSO landing page");
            }
            Trace.TraceInformation("Sucessfully parsed SSO token page.\n{0}",htmlPage);
            return authCookie;
        }
    }
}
