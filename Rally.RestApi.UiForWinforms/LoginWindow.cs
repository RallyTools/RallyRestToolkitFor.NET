using Rally.RestApi.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rally.RestApi.UiForWinforms
{
	internal partial class LoginWindow : Form
	{
		#region Enum: TabType
		private enum TabType
		{
			Credentials,
			Rally,
			Proxy,
		}
		#endregion

		internal RestApiAuthMgrWinforms AuthMgr { get; set; }

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public LoginWindow()
		{
			InitializeComponent();

			ShowMessage();
		}
		#endregion

		#region SetLogo
		internal void SetLogo(Image logo)
		{
			logoIcon.Image = logo;
		}
		#endregion

		/// <summary>
		/// Updates the fields to show what is in the API.
		/// </summary>
		internal void SetFields()
		{
			if (AuthMgr.Api.AuthenticationState == RallyRestApi.AuthenticationResult.NotAuthorized)
			{
				usernameInput.Text = String.Empty;
				passwordInput.Text = String.Empty;
				rallyServerInput.Text = ApiAuthManager.LoginWindowDefaultServer.ToString();
				if (ApiAuthManager.LoginWindowDefaultProxyServer != null)
					proxyServerInput.Text = ApiAuthManager.LoginWindowDefaultProxyServer.ToString();

				proxyUserNameInput.Text = String.Empty;
				proxyPasswordInput.Text = String.Empty;
			}
		}

		#region UpdateLoginState
		/// <summary>
		/// Updates the login state to show the correct buttons and field visibility.
		/// </summary>
		internal void UpdateLoginState()
		{
			bool isReadOnly = true;

			switch (AuthMgr.Api.AuthenticationState)
			{
				case RallyRestApi.AuthenticationResult.Authenticated:
					loginBtn.Visible = false;
					logoutBtn.Visible = true;
					ShowMessage();
					break;
				case RallyRestApi.AuthenticationResult.PendingSSO:
					loginBtn.Visible = false;
					logoutBtn.Visible = false;
					ShowMessage(ApiAuthManager.LoginWindowSsoInProgressText);
					break;
				case RallyRestApi.AuthenticationResult.NotAuthorized:
					loginBtn.Visible = true;
					logoutBtn.Visible = false;
					isReadOnly = false;
					break;
				default:
					throw new InvalidProgramException("Unknown authentication state.");
			}

			SetReadOnlyStateForEditors(isReadOnly);
		}
		#endregion

		#region BuildLayout
		internal void BuildLayout(RestApiAuthMgrWinforms authMgr)
		{
			Text = ApiAuthManager.LoginWindowTitle;
			AuthMgr = authMgr;

			tabCredentials.Text = ApiAuthManager.LoginWindowCredentialsTabText;
			userNameLabel.Text = ApiAuthManager.LoginWindowUserNameLabelText;
			passwordLabel.Text = ApiAuthManager.LoginWindowPwdLabelText;

			tabServer.Text = ApiAuthManager.LoginWindowRallyServerTabText;
			rallyServerLabel.Text = ApiAuthManager.LoginWindowServerLabelText;

			tabProxy.Text = ApiAuthManager.LoginWindowProxyServerTabText;
			proxyServerLabel.Text = ApiAuthManager.LoginWindowProxyServerLabelText;
			proxyUserNameLabel.Text = ApiAuthManager.LoginWindowProxyUserNameLabelText;
			proxyPasswordLabel.Text = ApiAuthManager.LoginWindowProxyPwdLabelText;

			loginBtn.Text = ApiAuthManager.LoginWindowLoginText;
			logoutBtn.Text = ApiAuthManager.LoginWindowLogoutText;
			cancelBtn.Text = ApiAuthManager.LoginWindowCancelText;

			if ((authMgr.Api != null) &&
				(authMgr.Api.ConnectionInfo != null) &&
				(authMgr.Api.ConnectionInfo.Server != null))
			{
				rallyServerInput.Text = authMgr.Api.ConnectionInfo.Server.ToString();
			}
			else if (ApiAuthManager.LoginWindowDefaultServer != null)
				rallyServerInput.Text = ApiAuthManager.LoginWindowDefaultServer.ToString();
			else
				rallyServerInput.Text = RallyRestApi.DEFAULT_SERVER;

			if ((authMgr.Api != null) &&
				(authMgr.Api.ConnectionInfo != null) &&
				(authMgr.Api.ConnectionInfo.Proxy != null) &&
				(authMgr.Api.ConnectionInfo.Proxy.Address != null))
			{
				proxyServerInput.Text = authMgr.Api.ConnectionInfo.Proxy.Address.ToString();
			}
			else if (ApiAuthManager.LoginWindowDefaultProxyServer != null)
				proxyServerInput.Text = ApiAuthManager.LoginWindowDefaultProxyServer.ToString();
			else
				proxyServerInput.Text = String.Empty;

			if (authMgr.Api.AuthenticationState != RallyRestApi.AuthenticationResult.NotAuthorized)
			{
				usernameInput.Text = AuthMgr.Api.ConnectionInfo.UserName;
				passwordInput.Text = String.Empty;
				rallyServerInput.Text = AuthMgr.Api.ConnectionInfo.Server.ToString();
				if (AuthMgr.Api.ConnectionInfo.Proxy != null)
					proxyServerInput.Text = AuthMgr.Api.ConnectionInfo.Proxy.Address.ToString();
			}
		}
		#endregion

		#region SetReadOnlyStateForEditor
		private void SetReadOnlyStateForEditors(bool isReadOnly)
		{
			usernameInput.ReadOnly = isReadOnly;
			passwordInput.ReadOnly = isReadOnly;

			rallyServerInput.ReadOnly = isReadOnly;

			proxyServerInput.ReadOnly = isReadOnly;
			proxyUserNameInput.ReadOnly = isReadOnly;
			proxyPasswordInput.ReadOnly = isReadOnly;
		}
		#endregion

		#region loginBtn_Click

		private void loginBtn_Click(object sender, EventArgs e)
		{
			string errorMessage;
			AuthMgr.LoginDetails.ConnectionType = Connection.ConnectionType.SpBasedSso;
			AuthMgr.LoginDetails.Username = usernameInput.Text;
			AuthMgr.LoginDetails.SetPassword(passwordInput.Text);
			AuthMgr.LoginDetails.RallyServer = rallyServerInput.Text;
			AuthMgr.LoginDetails.ProxyServer = proxyServerInput.Text;
			AuthMgr.LoginDetails.ProxyUsername = proxyUserNameInput.Text;
			AuthMgr.LoginDetails.SetProxyPassword(proxyPasswordInput.Text);

			AuthMgr.PerformAuthenticationCheck(out errorMessage);
			ShowMessage(errorMessage);

			UpdateLoginState();
			if (AuthMgr.Api.AuthenticationState == RallyRestApi.AuthenticationResult.Authenticated)
				Close();
		}
		#endregion

		#region logoutBtn_Click
		private void logoutBtn_Click(object sender, EventArgs e)
		{
			AuthMgr.PerformLogout();
			SetFields();
			UpdateLoginState();
		}
		#endregion

		#region cancelBtn_Click
		private void cancelBtn_Click(object sender, EventArgs e)
		{
			Close();
		}
		#endregion

		#region SsoAuthenticationComplete
		internal void SsoAuthenticationComplete(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			if (authenticationResult == RallyRestApi.AuthenticationResult.Authenticated)
				Close();
			else
			{
				UpdateLoginState();
			}
		}
		#endregion

		#region ShowMessage
		private void ShowMessage(string message = "")
		{
			userMessageLabel.Text = message;
		}
		#endregion

		#region OnClosing
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			AuthMgr.LoginWindowSsoAuthenticationComplete = null;
			base.OnClosing(e);
		}
		#endregion
	}
}
