using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Auth
{
	/// <summary>
	/// A authentication manager for a manually implmemented authentication.
	/// </summary>
	internal class ConsoleAuthenticationManager : IAuthorizationManager
	{
		/// <summary>
		/// The API that is linked to this authorization manager.
		/// </summary>
		public RallyRestApi Api { get; set; }
		/// <summary>
		/// Authenticates the user against Rally.
		/// </summary>
		public void ShowUserLoginWindow(AuthenticationComplete authenticationComplete,
			SsoAuthenticationComplete ssoAuthenticationComplete)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Is SSO authorized?
		/// </summary>
		public bool IsSsoAuthorized { get { return false; } }
		/// <summary>
		/// Opens the specified SSO URL to the user.
		/// </summary>
		/// <param name="ssoUrl">The URL that the user was redirected to in order to perform their SSO authentication.</param>
		public void OpenSsoPage(Uri ssoUrl)
		{
			throw new NotImplementedException();
		}
	}
}
