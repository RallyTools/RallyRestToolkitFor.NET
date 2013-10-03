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
        public AuthorizationType authType {get;set;}
        public Uri server { get; set; }
        public String username { get; set; }
        public String password { get; set; }
        public WebProxy proxy { get; set; }
        public String wsapiVersion { get; set; }
        public Cookie authCookie { get; set; }
        public virtual String getFinalSSOHandshakeHtml()
        {
            throw new NotImplementedException();
        }
    }
}
