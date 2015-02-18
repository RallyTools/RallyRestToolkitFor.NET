using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Auth
{
	/// <summary>
	/// A authentication manager for a manually implemented authentication.
	/// </summary>
	public class ApiConsoleAuthManager : ApiAuthManager
	{
		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="webServiceVersion">The version of the WSAPI API to use.</param>
		public ApiConsoleAuthManager(string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
			: base(false, null, null, null, webServiceVersion)
		{
		}
		#endregion

		#region ShowUserLoginWindowInternal
		/// <summary>
		/// Opens the window that displays the SSO URL to the user.
		/// </summary>
		/// <exception cref="NotImplementedException">This method is not supported for this authentication manager.</exception>
		protected override void ShowUserLoginWindowInternal()
		{
			throw new NotImplementedException("This authorization manager does not support UI elements.");
		}
		#endregion

		#region OpenSsoPageInternal
		/// <summary>
		/// Opens the window that displays the SSO URL to the user.
		/// </summary>
		/// <param name="ssoUrl">The URL for the SSO page to be opened.</param>
		/// <exception cref="NotImplementedException">This method is not supported for this authentication manager.</exception>
		protected override void OpenSsoPageInternal(Uri ssoUrl)
		{
			throw new NotImplementedException("This authorization manager does not support UI elements.");
		}
		#endregion

		#region NotifyLoginWindowSsoComplete
		/// <summary>
		/// Notifies the login window that SSO has been completed.
		/// </summary>
		/// <param name="authenticationResult">The current state of the authentication process. <see cref="RallyRestApi.AuthenticationResult"/></param>
		/// <param name="api">The API that was authenticated against.</param>
		/// <exception cref="NotImplementedException">This method is not supported for this authentication manager.</exception>
		protected override void NotifyLoginWindowSsoComplete(
			RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			throw new NotImplementedException("This authorization manager does not support UI elements.");
		}
		#endregion
	}
}
