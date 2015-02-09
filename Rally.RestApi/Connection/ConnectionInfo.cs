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
		/// The API Key for this connection.
		/// </summary>
		public string ApiKey { get; set; }
		/// <summary>
		/// The ZSessionID for this connection.
		/// </summary>
		public string ZSessionID { get; set; }
		/// <summary>
		/// The server this connection is to.
		/// </summary>
		public virtual Uri Server { get; set; }
		/// <summary>
		/// The IDP server to use for authentication.
		/// </summary>
		public virtual Uri IdpServer { get; set; }
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
			AuthType = AuthorizationType.Basic;
			ApiKey = String.Empty;
			UserName = String.Empty;
			Password = String.Empty;
			IdpServer = null;
			Server = null;
			Proxy = null;
		}
		#endregion
	}
}
