using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Rally.RestApi;
using System.Text.RegularExpressions;

namespace Rally.RestApi.Connection
{
	/// <summary>
	/// An object for tracking connection information.
	/// </summary>
	public class ConnectionInfo
	{
		#region Properties
		/// <summary>
		/// The authorization type for this connection.
		/// </summary>
		public virtual AuthorizationType AuthType { get; set; }
		/// <summary>
		/// The security token for this connection.
		/// </summary>
		public string SecurityToken { get; set; }
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
		/// Is this connection using WSAPI 2?
		/// </summary>
		internal bool IsWsapi2 { get { return !new Regex("^1[.]\\d+").IsMatch(WsapiVersion); } }
		/// <summary>
		/// The authorization cookie for this connection.
		/// </summary>
		public virtual Cookie AuthCookie { get; set; }
		/// <summary>
		/// The port we are connecting to.
		/// </summary>
		public virtual int Port { get; set; }
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public ConnectionInfo()
		{
			Port = 0;
		}
		#endregion

		#region DoSSOAuth
		/// <summary>
		/// Perform SSO authorization for this connection.
		/// </summary>
		public virtual void DoSSOAuth()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region ParseSSOLandingPage
		/// <summary>
		/// Parses an SSO landing page to retreive the Cookie that is embedded for SSO.
		/// </summary>
		protected Cookie ParseSSOLandingPage(String ssoLandingPage)
		{
			return SSOHelper.ParseSSOLandingPage(ssoLandingPage);
		}
		#endregion
	}
}
