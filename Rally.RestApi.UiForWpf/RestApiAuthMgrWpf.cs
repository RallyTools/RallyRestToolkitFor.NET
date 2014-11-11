using Rally.RestApi.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Rally.RestApi.UiForWpf
{
	/// <summary>
	/// A WPF based authentication manager.
	/// </summary>
	public class RestApiAuthMgrWpf : IAuthorizationManager
	{
		/// <summary>
		/// The API that is linked to this authorization manager.
		/// </summary>
		public RallyRestApi Api { get; set; }
		LoginWindow loginControl = null;
		private SsoAuthenticationComplete ssoAuthenticationComplete;
		private static bool isConfigured = false;
		/// <summary>
		/// Is SSO authorized?
		/// </summary>
		public bool IsSsoAuthorized { get { return true; } }

		#region Configure
		/// <summary>
		/// Configures the authorization manger. This must be called before any other method.
		/// </summary>
		public static void Configure(bool trustAllSslCertificates, ImageSource loginWindowLogo,
			string loginWindowHeaderLabelText, string loginWindowCredentialsTabText = null,
			string loginWindowUserNameLabelText = null, string loginWindowPwdLabelText = null,
			string loginWindowServerTabText = null, string loginWindowServerLabelText = null,
			string loginWindowProxyServerLabelText = null, string loginWindowProxyUserNameLabelText = null,
			string loginWindowProxyPwdLabelText = null, string loginWindowLoginButtonText = null,
			string loginWindowLogoutButtonText = null, string loginWindowCancelButtonText = null)
		{
			if (trustAllSslCertificates)
			{
				//Change SSL checks so that all checks pass
				ServicePointManager.ServerCertificateValidationCallback =
						new RemoteCertificateValidationCallback(delegate { return true; });
			}

			LoginWindow.Configure(loginWindowLogo,
				loginWindowHeaderLabelText, loginWindowCredentialsTabText,
				loginWindowUserNameLabelText, loginWindowPwdLabelText,
				loginWindowServerTabText, loginWindowServerLabelText,
				loginWindowProxyServerLabelText, loginWindowProxyUserNameLabelText,
				loginWindowProxyPwdLabelText, loginWindowLoginButtonText,
				loginWindowLogoutButtonText, loginWindowCancelButtonText);

			isConfigured = true;
		}
		#endregion

		#region ShowUserLoginWindow
		/// <summary>
		/// Authenticates the user against Rally.
		/// </summary>
		public void ShowUserLoginWindow(AuthenticationComplete authenticationComplete,
			SsoAuthenticationComplete ssoAuthenticationComplete)
		{
			if (!isConfigured)
				throw new InvalidOperationException("You must call Configure prior to calling this method.");

			// If the login control exists, don't open a new one.
			if (loginControl == null)
			{
				this.ssoAuthenticationComplete = ssoAuthenticationComplete;
				loginControl = new LoginWindow();
				loginControl.AuthMgr = this;
				loginControl.AuthenticationComplete += authenticationComplete;
				loginControl.Show();
				loginControl.Closed += loginControl_Closed;
			}
			else
			{
				loginControl.Show();
				loginControl.Focus();
			}
		}
		#endregion

		#region OpenSsoPage
		/// <summary>
		/// Shows the specified SSO URL to the user.
		/// </summary>
		/// <param name="ssoUrl">The URL that the user was redirected to in order to perform their SSO authentication.</param>
		public void OpenSsoPage(Uri ssoUrl)
		{
			if (!isConfigured)
				throw new InvalidOperationException("You must call Configure prior to calling this method.");

			if (ssoUrl == null)
				throw new ArgumentNullException("ssoUrl", "You must provide a URL for completing SSO authentication.");

			SsoWindow window = new SsoWindow();
			window.SsoResults += ReportSsoResults;
			window.ShowSsoPage(ssoUrl);
		}
		#endregion

		#region ReportSsoResults
		/// <summary>
		/// Reports the results of an SSO action.
		/// </summary>
		/// <param name="success">Was SSO authentication completed successfully?</param>
		/// <param name="zSessionID">The zSessionID that was returned from Rally.</param>
		private void ReportSsoResults(bool success, string zSessionID)
		{
			if (ssoAuthenticationComplete != null)
			{
				if (success)
				{
					// TODO: Remove need to reauthenticate with username in this case
					RallyRestApi.AuthenticationResult authResult =
						Api.AuthenticateWithZSessionID(Api.ConnectionInfo.UserName, zSessionID);

					if (authResult == RallyRestApi.AuthenticationResult.Authenticated)
						ssoAuthenticationComplete.Invoke(authResult, Api);
				}

				ssoAuthenticationComplete.Invoke(RallyRestApi.AuthenticationResult.NotAuthorized, null);
			}
		}
		#endregion

		#region loginControl_Closed
		void loginControl_Closed(object sender, EventArgs e)
		{
			loginControl = null;
		}
		#endregion
	}
}
