using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Connection
{
	/// <summary>
	/// 3 types of connection types
	/// </summary>
	public enum ConnectionType
	{
		/// <summary>
		/// Basic authorization where the user provides a username and password.
		/// </summary>
		BasicAuth,
		/// <summary>
		/// Service provider based connection.
		/// </summary>
		SpBasedSso,
		/// <summary>
		/// Identity provider based connection.
		/// </summary>
		IdpBasedSso,
	}
}
