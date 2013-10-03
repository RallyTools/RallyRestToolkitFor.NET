using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Rally.RestApi
{
    public interface IConnectionInfo
    {
        AuthorizationType authType {get;set;}
        Uri server { get; set; }
        String username { get; set; }
        String password { get; set; }
        WebProxy proxy { get; set; }
        String wsapiVersion { get; set; }
        Cookie authCookie { get; set; }
        String getFinalSSOHandshakeHtml();
    }
}
