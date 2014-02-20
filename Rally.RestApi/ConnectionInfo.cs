using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Rally.RestApi;

namespace Rally.RestApi
{
	/// <summary>
	/// An object for tracking connection information.
	/// </summary>
	public class ConnectionInfo : IConnectionInfo
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ConnectionInfo()
		{
			Port = 0;
		}

		/// <summary>
		/// The authorization type for this connection.
		/// </summary>
		public virtual AuthorizationType AuthType { get; set; }
		/// <summary>
		/// The server this connection is to.
		/// </summary>
		public virtual Uri Server { get; set; }
		/// <summary>
		/// The username for this connection.
		/// </summary>
		public virtual String UserName { get; set; }
		/// <summary>
		/// The password for this connection.
		/// </summary>
		public virtual String Password { get; set; }
		/// <summary>
		/// The proxy server to use for this connection.
		/// </summary>
		public virtual WebProxy Proxy { get; set; }
		/// <summary>
		/// The WSAPI version we are talking to.
		/// </summary>
		public virtual String WsapiVersion { get; set; }
		/// <summary>
		/// The authorization cookie for this connection.
		/// </summary>
		public virtual Cookie AuthCookie { get; set; }
		/// <summary>
		/// The port we are connecting to.
		/// </summary>
		public virtual int Port { get; set; }

		/// <summary>
		/// Perform SSO authorization for this connection.
		/// </summary>
		public virtual void DoSSOAuth()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Parses an SSO landing page to retreive the Cookie that is embedded for SSO.
		/// </summary>
		protected Cookie ParseSSOLandingPage(String ssoLandingPage)
		{
			return SSOHelper.ParseSSOLandingPage(ssoLandingPage);
		}
	}
}
