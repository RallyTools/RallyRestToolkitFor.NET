using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Connection
{
	/// <summary>
	/// The types of authorization that are available.
	/// </summary>
	public enum AuthorizationType
	{
		/// <summary>
		/// Basic authorization where the user provides a username and password.
		/// </summary>
		Basic,
		/// <summary>
		/// SSO authorization.
		/// </summary>
		SSO
	}
}
