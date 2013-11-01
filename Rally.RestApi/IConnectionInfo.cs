using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Rally.RestApi
{
    public interface IConnectionInfo
    {
        AuthorizationType authType { get; }
        Uri server { get; }
        String username { get; }
        String password { get; }
        WebProxy proxy { get; }
        String wsapiVersion { get; }
        Cookie authCookie { get; set; }
        int port { get; set; }

        void doSSOAuth();
    }
}
