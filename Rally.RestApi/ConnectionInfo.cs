using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Rally.RestApi;

namespace Rally.RestApi
{
    public class ConnectionInfo : IConnectionInfo
    {
        public ConnectionInfo()
        {
            port = 0;
        }

        public virtual AuthorizationType authType {get;set;}
        public virtual Uri server { get; set; }
        public virtual String username { get; set; }
        public virtual String password { get; set; }
        public virtual WebProxy proxy { get; set; }
        public virtual String wsapiVersion { get; set; }
        public virtual Cookie authCookie { get; set; }
        public virtual int port { get; set; }

        public virtual void doSSOAuth()
        {
            if (authType == AuthorizationType.SSO)
            {
                doBrowserSSOAuth();
            }
            else if (authType == AuthorizationType.SSOWithoutCred)
            {
                doNetworkSSOAuth();
            }
        }

        protected virtual void doBrowserSSOAuth()
        {
            throw new NotImplementedException();
        }

        protected virtual void doNetworkSSOAuth()
        {
            throw new NotImplementedException();
        }

        protected Cookie parseSSOLandingPage(String ssoLandingPage)
        {
            return SSOHelper.parsSSOLandingPage(ssoLandingPage);
        }
    }
}
