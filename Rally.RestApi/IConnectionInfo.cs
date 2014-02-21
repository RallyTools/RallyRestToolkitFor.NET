using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Rally.RestApi
{
	/// <summary>
	/// An interface for connection information.
	/// </summary>
	public interface IConnectionInfo
	{
		/// <summary>
		/// The authorization type for this connection.
		/// </summary>
		AuthorizationType AuthType { get; }
		/// <summary>
		/// The server this connection is to.
		/// </summary>
		Uri Server { get; }
		/// <summary>
		/// The username for this connection.
		/// </summary>
		String UserName { get; }
		/// <summary>
		/// The password for this connection.
		/// </summary>
		String Password { get; }
		/// <summary>
		/// The proxy server to use for this connection.
		/// </summary>
		WebProxy Proxy { get; }
		/// <summary>
		/// The WSAPI version we are talking to.
		/// </summary>
		String WsapiVersion { get; }
		/// <summary>
		/// The authorization cookie for this connection.
		/// </summary>
		Cookie AuthCookie { get; set; }
		/// <summary>
		/// The port we are connecting to.
		/// </summary>
		int Port { get; set; }
		/// <summary>
		/// Perform SSO authorization for this connection.
		/// </summary>
		void DoSSOAuth();
	}
}
