using Rally.RestApi.Auth;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rally.RestApi.UiForWinforms
{
	/// <summary>
	/// A Winforms based authentication manager.
	/// </summary>
	public class RestApiAuthMgrWinforms : ApiAuthManager
	{
		static Image logoForUi = null;
		LoginWindow loginControl = null;
		internal SsoAuthenticationComplete LoginWindowSsoAuthenticationComplete;

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public RestApiAuthMgrWinforms(string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
			: base(true, webServiceVersion)
		{
		}
		#endregion

		#region SetLogo
		/// <summary>
		/// Sets the logo used in the user controls.
		/// </summary>
		public static void SetLogo(Image logo)
		{
			logoForUi = logo;
		}
		#endregion

		#region ShowUserLoginWindowInternal
		/// <summary>
		/// Opens the window that displays the SSO URL to the user.
		/// </summary>
		protected override void ShowUserLoginWindowInternal()
		{
			// If the login control exists, don't open a new one.
			if (loginControl == null)
			{
				loginControl = new LoginWindow();
				loginControl.BuildLayout(this);
				loginControl.Closed += loginControl_Closed;
				LoginWindowSsoAuthenticationComplete += loginControl.SsoAuthenticationComplete;
			}

			loginControl.SetLogo(logoForUi);
			loginControl.SetFields();
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
			SsoWindow window = new SsoWindow();
			window.ShowSsoPage(this, ssoUrl);
		}
		#endregion

		#region OpenIdpBasedSsoPage
		/// <summary>
		/// Opens the specified SSO URL to the user.
		/// </summary>
		/// <param name="idpBasedSsoUrl">The Uri that the user was redirected to in order to perform their SSO authentication.</param>
		protected override void OpenIdpBasedSsoPageInternal(Uri idpBasedSsoUrl)
		{
			SsoWindow window = new SsoWindow();
			window.ShowSsoPage(this, idpBasedSsoUrl);
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

		#region PerformAuthenticationCheck
		/// <summary>
		/// Performs an authentication check against Rally with the specified credentials
		/// </summary>
		internal RallyRestApi.AuthenticationResult PerformAuthenticationCheck(string username, string password, string rallyServer,
			string proxyServer, string proxyUser, string proxyPassword, out string errorMessage)
		{
			return PerformAuthenticationCheckAgainstRally(username, password, rallyServer,
				proxyServer, proxyUser, proxyPassword, out errorMessage);
		}
		#endregion

		#region PerformLogout
		/// <summary>
		/// Performs an logout from Rally.
		/// </summary>
		internal void PerformLogout()
		{
			base.PerformLogoutFromRally();
		}
		#endregion
	}
}
