using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Rally.RestApi;

namespace Rally.RestApi
{
    public class ConnectionInfo
    {
        public AuthorizationType authType;
        public Uri server;
        public String username;
        public String password;
        public WebProxy proxy;
        public String wsapiVersion;
        public Cookie authCookie;
    }
}
