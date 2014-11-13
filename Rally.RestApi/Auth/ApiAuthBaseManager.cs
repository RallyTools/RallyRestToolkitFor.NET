using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;

namespace Rally.RestApi.Auth
{
	/// <summary>
	/// A authentication manager for a manually implmemented authentication.
	/// </summary>
	public abstract class ApiAuthBaseManager
	{
		/// <summary>
		/// The API that is linked to this authorization manager.
		/// </summary>
		public RallyRestApi Api { get; private set; }
		/// <summary>
		/// Is the UI supported?
		/// </summary>
		public bool IsUiSupported { get; private set; }
		/// <summary>
		/// Notifies that SSO authentication has completed.
		/// </summary>
		protected SsoAuthenticationComplete SsoAuthenticationComplete;

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public ApiAuthBaseManager(string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
		{
			IsUiSupported = false;
			Api = new RallyRestApi(this, webServiceVersion: webServiceVersion);
		}
		/// <summary>
		/// Constructor
		/// </summary>
		protected ApiAuthBaseManager(bool isUiSupported, string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
		{
			IsUiSupported = isUiSupported;
			Api = new RallyRestApi(this, webServiceVersion: webServiceVersion);
		}
		#endregion

		/// <summary>
		/// Authenticates the user against Rally.
		/// </summary>
		public virtual void ShowUserLoginWindow(AuthenticationComplete authenticationComplete,
			SsoAuthenticationComplete ssoAuthenticationComplete)
		{
			SsoAuthenticationComplete = ssoAuthenticationComplete;
			ShowUserLoginWindowInternal(authenticationComplete);
		}
		/// <summary>
		/// Opens the window that displays the SSO URL to the user.
		/// </summary>
		protected abstract void ShowUserLoginWindowInternal(AuthenticationComplete authenticationComplete);

		#region OpenSsoPage
		/// <summary>
		/// Opens the specified SSO URL to the user.
		/// </summary>
		/// <param name="ssoUrl">The Uri that the user was redirected to in order to perform their SSO authentication.</param>
		public void OpenSsoPage(Uri ssoUrl)
		{
			if (ssoUrl == null)
				throw new ArgumentNullException("ssoUrl", "You must provide a URL for completing SSO authentication.");

			OpenSsoPageInternal(ssoUrl);
		}
		/// <summary>
		/// Opens the window that displays the SSO URL to the user.
		/// </summary>
		protected abstract void OpenSsoPageInternal(Uri ssoUrl);
		#endregion

		#region ReportSsoResults
		/// <summary>
		/// Reports the results of an SSO action.
		/// </summary>
		/// <param name="success">Was SSO authentication completed successfully?</param>
		/// <param name="zSessionID">The zSessionID that was returned from Rally.</param>
		public void ReportSsoResults(bool success, string zSessionID)
		{
			if (SsoAuthenticationComplete != null)
			{
				if (success)
				{
					RallyRestApi.AuthenticationResult authResult =
						Api.AuthenticateWithZSessionID(Api.ConnectionInfo.UserName, zSessionID);

					if (authResult == RallyRestApi.AuthenticationResult.Authenticated)
					{
						NotifyLoginWindowSsoComplete(authResult, Api);
						SsoAuthenticationComplete.Invoke(authResult, Api);
						return;
					}
				}

				Api.Logout();
				NotifyLoginWindowSsoComplete(RallyRestApi.AuthenticationResult.NotAuthorized, null);
				SsoAuthenticationComplete.Invoke(RallyRestApi.AuthenticationResult.NotAuthorized, null);
			}
		}
		/// <summary>
		/// Notifies the login window that SSO has been completed.
		/// </summary>
		protected abstract void NotifyLoginWindowSsoComplete(
			RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api);
		#endregion

		#region TrustAllCertificates
		/// <summary>
		/// Change SSL checks so that all checks pass (self signed, etc.)
		/// </summary>
		public static void TrustAllCertificates()
		{
			ServicePointManager.ServerCertificateValidationCallback =
					new RemoteCertificateValidationCallback(delegate { return true; });
		}
		#endregion
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
