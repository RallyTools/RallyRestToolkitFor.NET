using Rally.RestApi.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rally.RestApi.UiForWinforms
{
	/// <summary>
	/// A Winforms based authentication manager.
	/// </summary>
	public class RestApiAuthMgrWinforms : ApiAuthBaseManager
	{
		LoginWindow loginControl = null;
		internal SsoAuthenticationComplete LoginWindowSsoAuthenticationComplete;
		private static bool isConfigured = false;

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public RestApiAuthMgrWinforms(string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
			: base(true, webServiceVersion)
		{
		}
		#endregion

		#region ConfigureUserInterface
		/// <summary>
		/// Configures the authorization manger. This must be called before any other method.
		/// </summary>
		public static void ConfigureUserInterface(bool trustAllSslCertificates, object loginWindowLogo,
			string loginWindowHeaderLabelText,
			string loginWindowCredentialsTabText = null,
			string loginWindowUserNameLabelText = null, string loginWindowPwdLabelText = null,
			string loginWindowServerTabText = null, string loginWindowServerLabelText = null,
			Uri loginWindowDefaultServer = null,
			string loginWindowProxyServerTabText = null,
			string loginWindowProxyServerLabelText = null, string loginWindowProxyUserNameLabelText = null,
			string loginWindowProxyPwdLabelText = null, Uri loginWindowDefaultProxyServer = null,
			string loginWindowSsoInProgressText = null,
			string loginWindowLoginButtonText = null, string loginWindowLogoutButtonText = null,
			string loginWindowCancelButtonText = null)
		{
			if (trustAllSslCertificates)
				ApiAuthBaseManager.TrustAllCertificates();

			LoginWindow.Configure(loginWindowLogo,
				loginWindowHeaderLabelText,
				loginWindowDefaultServer,
				loginWindowDefaultProxyServer,
				loginWindowCredentialsTabText,
				loginWindowUserNameLabelText,
				loginWindowPwdLabelText,
				loginWindowServerTabText,
				loginWindowServerLabelText,
				loginWindowProxyServerTabText,
				loginWindowProxyServerLabelText,
				loginWindowProxyUserNameLabelText,
				loginWindowProxyPwdLabelText,
				loginWindowSsoInProgressText,
				loginWindowLoginButtonText,
				loginWindowLogoutButtonText,
				loginWindowCancelButtonText);

			isConfigured = true;
		}
		#endregion

		#region ShowUserLoginWindowInternal
		/// <summary>
		/// Opens the window that displays the SSO URL to the user.
		/// </summary>
		protected override void ShowUserLoginWindowInternal(AuthenticationComplete authenticationComplete)
		{
			if (!isConfigured)
				throw new InvalidOperationException("You must call Configure prior to calling this method.");

			// If the login control exists, don't open a new one.
			if (loginControl == null)
			{
				loginControl = new LoginWindow();
				loginControl.BuildLayout(this);
				loginControl.AuthenticationComplete += authenticationComplete;
				loginControl.Closed += loginControl_Closed;
				LoginWindowSsoAuthenticationComplete += loginControl.SsoAuthenticationComplete;
			}

			loginControl.UpdateLoginState();
			loginControl.Show();
			loginControl.Focus();
		}
		#endregion

		#region OpenSsoPageInternal
		/// <summary>
		/// Opens the specified SSO URL to the user.
		/// </summary>
		/// <param name="ssoUrl">The Uri that the user was redirected to in order to perform their SSO authentication.</param>
		protected override void OpenSsoPageInternal(Uri ssoUrl)
		{
			if (!isConfigured)
				throw new InvalidOperationException("You must call Configure prior to calling this method.");

			SsoWindow window = new SsoWindow();
			window.ShowSsoPage(this, ssoUrl);
		}
		#endregion

		#region NotifyLoginWindowSsoComplete
		/// <summary>
		/// Notifies the login window that SSO has been completed.
		/// </summary>
		protected override void NotifyLoginWindowSsoComplete(
			RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			if (LoginWindowSsoAuthenticationComplete != null)
				LoginWindowSsoAuthenticationComplete.Invoke(authenticationResult, api);
		}
		#endregion

		#region loginControl_Closed
		void loginControl_Closed(object sender, EventArgs e)
		{
			LoginWindowSsoAuthenticationComplete = null;
			loginControl = null;
		}
		#endregion
	}
}
