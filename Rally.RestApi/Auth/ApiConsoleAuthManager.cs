using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Auth
{
	/// <summary>
	/// A authentication manager for a manually implmemented authentication.
	/// </summary>
	public class ApiConsoleAuthManager : ApiAuthManager
	{
		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public ApiConsoleAuthManager(string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
			: base()
		{
		}
		#endregion

		#region ShowUserLoginWindowInternal
		/// <summary>
		/// Opens the window that displays the SSO URL to the user.
		/// </summary>
		protected override void ShowUserLoginWindowInternal()
		{
			throw new NotImplementedException("This authorization manager does not support UI elements.");
		}
		#endregion

		#region OpenSsoPageInternal
		/// <summary>
		/// Opens the window that displays the SSO URL to the user.
		/// </summary>
		protected override void OpenSsoPageInternal(Uri ssoUrl)
		{
			throw new NotImplementedException("This authorization manager does not support UI elements.");
		}
		#endregion

		#region OpenIdpBasedSsoPageInternal
		/// <summary>
		/// Opens the window that displays the SSO URL to the user.
		/// </summary>
		protected override void OpenIdpBasedSsoPageInternal(Uri ssoUrl)
		{
			throw new NotImplementedException("This authorization manager does not support UI elements.");
		}
		#endregion

		#region NotifyLoginWindowSsoComplete
		/// <summary>
		/// Notifies the login window that SSO has been completed.
		/// </summary>
		protected override void NotifyLoginWindowSsoComplete(
			RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			throw new NotImplementedException("This authorization manager does not support UI elements.");
		}
		#endregion
	}
}
