using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally.RestApi.Auth
{
	/// <summary>
	/// An interface that defines an authroization manager
	/// </summary>
	public interface IAuthorizationManager
	{
		/// <summary>
		/// The API that is linked to this authorization manager.
		/// </summary>
		RallyRestApi Api { get; set; }
		/// <summary>
		/// Authenticates the user against Rally.
		/// </summary>
		void ShowUserLoginWindow(AuthenticationComplete authenticationComplete,
			SsoAuthenticationComplete ssoAuthenticationComplete);
		/// <summary>
		/// Is SSO authorized?
		/// </summary>
		bool IsSsoAuthorized { get; }
		/// <summary>
		/// Opens the specified SSO URL to the user.
		/// </summary>
		/// <param name="ssoUrl">The Uri that the user was redirected to in order to perform their SSO authentication.</param>
		void OpenSsoPage(Uri ssoUrl);
	}

	/// <summary>
	/// A delegate to indicate that authenication has completed.
	/// </summary>
	/// <param name="authenticationResult">The status of authentication.</param>
	/// <param name="api">The authenticated API that can be used for the user who logged in.</param>
	public delegate void AuthenticationComplete(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api);

	/// <summary>
	/// A delegate to indicate that SSO authentication has completed.
	/// </summary>
	/// <param name="authenticationResult">The status of authentication.</param>
	/// <param name="api">The authenticated API that can be used for the user who logged in.</param>
	public delegate void SsoAuthenticationComplete(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api);
}
