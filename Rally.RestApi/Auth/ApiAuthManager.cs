using Rally.RestApi.Connection;
using Rally.RestApi.Exceptions;
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
	public abstract class ApiAuthManager
	{
		#region Static Values
		/// <summary>
		/// The text for the login window title.
		/// </summary>
		public static string LoginWindowTitle { get; private set; }
		/// <summary>
		/// The text for the header label in the login window.
		/// </summary>
		public static string LoginWindowHeaderLabelText { get; private set; }
		/// <summary>
		/// The text for the credentials tab in the login window.
		/// </summary>
		public static string LoginWindowCredentialsTabText { get; private set; }
		/// <summary>
		/// The text for the rally server tab in the login window.
		/// </summary>
		public static string LoginWindowRallyServerTabText { get; private set; }
		/// <summary>
		/// The text for the proxy tab in the login window.
		/// </summary>
		public static string LoginWindowProxyServerTabText { get; private set; }

		/// <summary>
		/// The text for the user name label in the login window.
		/// </summary>
		public static string LoginWindowUserNameLabelText { get; private set; }
		/// <summary>
		/// The text for the password label in the login window.
		/// </summary>
		public static string LoginWindowPwdLabelText { get; private set; }
		/// <summary>
		/// The text for the connection type label in the login window.
		/// </summary>
		public static string LoginWindowConnectionTypeText { get; private set; }
		/// <summary>
		/// The text for the server label in the login window.
		/// </summary>
		public static string LoginWindowServerLabelText { get; private set; }
		/// <summary>
		/// The text for the proxy server label in the login window.
		/// </summary>
		public static string LoginWindowProxyServerLabelText { get; private set; }
		/// <summary>
		/// The text for the proxy username label in the login window.
		/// </summary>
		public static string LoginWindowProxyUserNameLabelText { get; private set; }
		/// <summary>
		/// The text for the proxy password label in the login window.
		/// </summary>
		public static string LoginWindowProxyPwdLabelText { get; private set; }

		/// <summary>
		/// The text for the sso in progress label in the login window.
		/// </summary>
		public static string LoginWindowSsoInProgressText { get; private set; }
		/// <summary>
		/// The text for the login button in the login window.
		/// </summary>
		public static string LoginWindowLoginText { get; private set; }
		/// <summary>
		/// The text for the logout button in the login window.
		/// </summary>
		public static string LoginWindowLogoutText { get; private set; }
		/// <summary>
		/// The text for the cancel button in the login window.
		/// </summary>
		public static string LoginWindowCancelText { get; private set; }

		/// <summary>
		/// The default server for the login window.
		/// </summary>
		public static Uri LoginWindowDefaultServer { get; private set; }
		/// <summary>
		/// The default proxy server for the login window.
		/// </summary>
		public static Uri LoginWindowDefaultProxyServer { get; private set; }

		/// <summary>
		/// The error message to show when a login failure occured due to the server field being empty.
		/// </summary>
		public static String LoginFailureServerEmpty { get; private set; }
		/// <summary>
		/// The error message to show when a login failure occured due to the credentials being empty.
		/// </summary>
		public static String LoginFailureLoginEmpty { get; private set; }
		/// <summary>
		/// The error message to show when a login failure occured due to the server not being reachable.
		/// </summary>
		public static String LoginFailureBadServer { get; private set; }
		/// <summary>
		/// The error message to show when a login failure occured due to bad credentials.
		/// </summary>
		public static String LoginFailureCredentials { get; private set; }
		/// <summary>
		/// The error message to show when we failed to connect to a server or proxy.
		/// </summary>
		public static String LoginFailureBadConnection { get; private set; }
		/// <summary>
		/// The error message to show when a login failure occured due to bad proxy credentials.
		/// </summary>
		public static String LoginFailureProxyCredentials { get; private set; }
		/// <summary>
		/// The error message to show when an unknown login failure occured.
		/// </summary>
		public static String LoginFailureUnknown { get; private set; }

		#endregion

		#region Properties
		private bool trustAllCertificates;
		/// <summary>
		/// Should all certificates be trusted?
		/// </summary>
		public bool TrustAllCertificates
		{
			get { return trustAllCertificates; }
			set
			{
				trustAllCertificates = value;
				if (trustAllCertificates)
					ServicePointManager.ServerCertificateValidationCallback =
						new RemoteCertificateValidationCallback(delegate { return true; });
				else
					ServicePointManager.ServerCertificateValidationCallback = null;
			}
		}
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
		protected AuthenticationStateChange AuthenticationStateChange { get; private set; }
		/// <summary>
		/// Notifies that SSO authentication has completed.
		/// </summary>
		protected SsoAuthenticationComplete SsoAuthenticationComplete { get; private set; }
		#endregion

		#region Constructor
		static ApiAuthManager()
		{
			Configure(null);
		}
		/// <summary>
		/// Constructor
		/// </summary>
		public ApiAuthManager(string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
		{
			IsUiSupported = false;
			Api = new RallyRestApi(this, webServiceVersion: webServiceVersion);
		}
		/// <summary>
		/// Constructor
		/// </summary>
		protected ApiAuthManager(bool isUiSupported, string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
		{
			IsUiSupported = isUiSupported;
			Api = new RallyRestApi(this, webServiceVersion: webServiceVersion);
		}
		#endregion

		#region Configure
		/// <summary>
		/// Configures the authorization manger. This must be called before any other method.
		/// </summary>
		public static void Configure(string loginWindowTitle = null,
			string loginWindowHeaderLabelText = null,
			string loginWindowCredentialsTabText = null,
			string loginWindowUserNameLabelText = null, string loginWindowPwdLabelText = null,
			string loginWindowServerTabText = null, string loginWindowConnectionTypeText = null,
			string loginWindowServerLabelText = null,
			Uri loginWindowDefaultServer = null,
			string loginWindowProxyServerTabText = null,
			string loginWindowProxyServerLabelText = null, string loginWindowProxyUserNameLabelText = null,
			string loginWindowProxyPwdLabelText = null, Uri loginWindowDefaultProxyServer = null,
			string loginWindowSsoInProgressText = null,
			string loginWindowLoginButtonText = null, string loginWindowLogoutButtonText = null,
			string loginWindowCancelButtonText = null,
			string loginFailureBadServer = null,
			string loginFailureCredentials = null,
			string loginFailureLoginEmpty = null,
			string loginFailureServerEmpty = null,
			string loginFailureProxyCredentials = null,
			string loginFailureBadConnection = null,
			string loginFailureUnknown = null)
		{
			LoginWindowTitle = loginWindowTitle;
			if (String.IsNullOrWhiteSpace(LoginWindowTitle))
				LoginWindowTitle = "Login to Rally";

			LoginWindowHeaderLabelText = loginWindowHeaderLabelText;
			if (String.IsNullOrWhiteSpace(LoginWindowHeaderLabelText))
				LoginWindowHeaderLabelText = "Login to Rally";

			LoginWindowDefaultServer = loginWindowDefaultServer;
			if (LoginWindowDefaultServer == null)
				LoginWindowDefaultServer = new Uri(RallyRestApi.DEFAULT_SERVER);
			LoginWindowDefaultProxyServer = loginWindowDefaultProxyServer;

			#region Default Strings: Credentials
			LoginWindowCredentialsTabText = loginWindowCredentialsTabText;
			if (String.IsNullOrWhiteSpace(LoginWindowCredentialsTabText))
				LoginWindowCredentialsTabText = "Credentials";

			LoginWindowUserNameLabelText = loginWindowUserNameLabelText;
			if (String.IsNullOrWhiteSpace(LoginWindowUserNameLabelText))
				LoginWindowUserNameLabelText = "User Name";

			LoginWindowPwdLabelText = loginWindowPwdLabelText;
			if (String.IsNullOrWhiteSpace(LoginWindowPwdLabelText))
				LoginWindowPwdLabelText = "Password";
			#endregion

			#region Default Strings: Rally
			LoginWindowConnectionTypeText = loginWindowConnectionTypeText;
			if (String.IsNullOrWhiteSpace(LoginWindowConnectionTypeText))
				LoginWindowConnectionTypeText = "Connection Type";

			LoginWindowRallyServerTabText = loginWindowServerTabText;
			if (String.IsNullOrWhiteSpace(LoginWindowRallyServerTabText))
				LoginWindowRallyServerTabText = "Rally";

			LoginWindowServerLabelText = loginWindowServerLabelText;
			if (String.IsNullOrWhiteSpace(LoginWindowServerLabelText))
				LoginWindowServerLabelText = "Server";
			#endregion

			#region Default Strings: Proxy
			LoginWindowProxyServerTabText = loginWindowProxyServerTabText;
			if (String.IsNullOrWhiteSpace(LoginWindowProxyServerTabText))
				LoginWindowProxyServerTabText = "Proxy";

			LoginWindowProxyServerLabelText = loginWindowProxyServerLabelText;
			if (String.IsNullOrWhiteSpace(LoginWindowProxyServerLabelText))
				LoginWindowProxyServerLabelText = "Server";

			LoginWindowProxyUserNameLabelText = loginWindowProxyUserNameLabelText;
			if (String.IsNullOrWhiteSpace(LoginWindowProxyUserNameLabelText))
				LoginWindowProxyUserNameLabelText = "User Name";

			LoginWindowProxyPwdLabelText = loginWindowProxyPwdLabelText;
			if (String.IsNullOrWhiteSpace(LoginWindowProxyPwdLabelText))
				LoginWindowProxyPwdLabelText = "Password";
			#endregion

			#region Default Strings: Buttons
			LoginWindowSsoInProgressText = loginWindowSsoInProgressText;
			if (String.IsNullOrWhiteSpace(LoginWindowSsoInProgressText))
				LoginWindowSsoInProgressText = "SSO in Progress";

			LoginWindowLoginText = loginWindowLoginButtonText;
			if (String.IsNullOrWhiteSpace(LoginWindowLoginText))
				LoginWindowLoginText = "Login";

			LoginWindowLogoutText = loginWindowLogoutButtonText;
			if (String.IsNullOrWhiteSpace(LoginWindowLogoutText))
				LoginWindowLogoutText = "Logout";

			LoginWindowCancelText = loginWindowCancelButtonText;
			if (String.IsNullOrWhiteSpace(LoginWindowCancelText))
				LoginWindowCancelText = "Cancel";
			#endregion

			#region Default Strings: Error Messages
			LoginFailureBadServer = loginFailureBadServer;
			if (String.IsNullOrWhiteSpace(LoginFailureBadServer))
				LoginFailureBadServer = "Bad Server or Network Issues";

			LoginFailureProxyCredentials = loginFailureProxyCredentials;
			if (String.IsNullOrWhiteSpace(LoginFailureProxyCredentials))
				LoginFailureProxyCredentials = "Bad Proxy Credentials";

			LoginFailureCredentials = loginFailureCredentials;
			if (String.IsNullOrWhiteSpace(LoginFailureCredentials))
				LoginFailureCredentials = "Bad Credentials";

			LoginFailureLoginEmpty = loginFailureLoginEmpty;
			if (String.IsNullOrWhiteSpace(LoginFailureLoginEmpty))
				LoginFailureLoginEmpty = "Username is a required field.";

			LoginFailureServerEmpty = loginFailureServerEmpty;
			if (String.IsNullOrWhiteSpace(LoginFailureServerEmpty))
				LoginFailureServerEmpty = "Rally Server is a required field.";

			LoginFailureBadConnection = loginFailureBadConnection;
			if (String.IsNullOrWhiteSpace(LoginFailureBadConnection))
				LoginFailureBadConnection = "Failed to connect to the Rally server or proxy.";

			LoginFailureUnknown = loginFailureUnknown;
			if (String.IsNullOrWhiteSpace(LoginFailureUnknown))
				LoginFailureUnknown = "An unknown error occurred while attempting to log in. Please try again.";
			#endregion
		}
		#endregion

		#region ShowUserLoginWindow
		/// <summary>
		/// Authenticates the user against Rally.
		/// </summary>
		public virtual void ShowUserLoginWindow(AuthenticationStateChange authenticationStateChange,
			SsoAuthenticationComplete ssoAuthenticationComplete)
		{
			AuthenticationStateChange = authenticationStateChange;
			SsoAuthenticationComplete = ssoAuthenticationComplete;
			ShowUserLoginWindowInternal();
		}
		/// <summary>
		/// Opens the window that displays the SSO URL to the user.
		/// </summary>
		protected abstract void ShowUserLoginWindowInternal();
		#endregion

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
		public void ReportIdpBasedSsoResults(bool success, string zSessionID)
		{
		}
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

		#region PerformAuthenticationCheckAgainstRally
		/// <summary>
		/// Performs an authentication check against Rally with the specified credentials
		/// </summary>
		protected RallyRestApi.AuthenticationResult PerformAuthenticationCheckAgainstRally(
			string username, string password, string rallyServer,
			string proxyServer, string proxyUser, string proxyPassword, out string errorMessage)
		{
			RallyRestApi.AuthenticationResult authResult = RallyRestApi.AuthenticationResult.NotAuthorized;
			errorMessage = String.Empty;
			WebProxy proxy = null;
			if (!String.IsNullOrWhiteSpace(proxyServer))
			{
				try
				{
					proxy = new WebProxy(new Uri(proxyServer));
				}
				catch
				{
					errorMessage = "Bad URI format for Proxy Server";
					return RallyRestApi.AuthenticationResult.NotAuthorized;
				}

				if (!String.IsNullOrWhiteSpace(proxyUser))
					proxy.Credentials = new NetworkCredential(proxyUser, proxyPassword);
				else
					proxy.UseDefaultCredentials = true;
			}

			if (String.IsNullOrWhiteSpace(rallyServer))
				errorMessage = LoginFailureServerEmpty;
			else if (String.IsNullOrWhiteSpace(username))
				errorMessage = LoginFailureLoginEmpty;

			Uri serverUri = null;
			try
			{
				serverUri = new Uri(rallyServer);
			}
			catch
			{
				errorMessage = "Bad URI format for Rally Server";
			}

			try
			{
				if (String.IsNullOrWhiteSpace(errorMessage))
					authResult = Api.Authenticate(username, password, serverUri, proxy);
			}
			catch (RallyUnavailableException)
			{
				errorMessage = "Rally is currently unavailable.";
			}
			catch (WebException e)
			{
				if (e.Response is HttpWebResponse)
				{
					if ((((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.BadGateway) ||
						(((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.BadRequest))
					{
						errorMessage = LoginFailureBadServer;
					}
					else if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
					{
						errorMessage = LoginFailureProxyCredentials;
					}
					else if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
					{
						errorMessage = LoginFailureCredentials;
					}
					else
						errorMessage = LoginFailureUnknown;
				}
				else if ((e is WebException) &&
					(((WebException)e).Status == WebExceptionStatus.ConnectFailure))
				{
					errorMessage = LoginFailureBadConnection;
				}
				else
					errorMessage = LoginFailureUnknown;
			}

			if (AuthenticationStateChange != null)
			{
				switch (Api.AuthenticationState)
				{
					case RallyRestApi.AuthenticationResult.Authenticated:
						AuthenticationStateChange.Invoke(Api.AuthenticationState, Api);
						break;
					case RallyRestApi.AuthenticationResult.PendingSSO:
					case RallyRestApi.AuthenticationResult.NotAuthorized:
						AuthenticationStateChange.Invoke(Api.AuthenticationState, null);
						break;
					default:
						throw new NotImplementedException();
				}
			}
			return Api.AuthenticationState;
		}
		#endregion

		#region PerformAuthenticationCheckAgainstIdp
		/// <summary>
		/// Performs an authentication check against an identity provider (IDP Initiated).
		/// </summary>
		protected RallyRestApi.AuthenticationResult PerformAuthenticationCheckAgainstIdp(
			string idpUrl, string proxyServer, string proxyUser, string proxyPassword, out string errorMessage)
		{
			errorMessage = String.Empty;
			WebProxy proxy = null;
			if (!String.IsNullOrWhiteSpace(proxyServer))
			{
				try
				{
					proxy = new WebProxy(new Uri(proxyServer));
				}
				catch
				{
					errorMessage = "Bad URI format for Proxy Server";
					return RallyRestApi.AuthenticationResult.NotAuthorized;
				}

				if (!String.IsNullOrWhiteSpace(proxyUser))
					proxy.Credentials = new NetworkCredential(proxyUser, proxyPassword);
				else
					proxy.UseDefaultCredentials = true;
			}

			if (String.IsNullOrWhiteSpace(idpUrl))
				errorMessage = LoginFailureServerEmpty;

			Uri serverUri = null;
			try
			{
				serverUri = new Uri(idpUrl);
			}
			catch
			{
				errorMessage = "Bad URI format for Rally Server";
			}

			try
			{
				if (String.IsNullOrWhiteSpace(errorMessage))
				{
					if (String.IsNullOrWhiteSpace(errorMessage))
					{
						Api.CreateIdpAuthentication(serverUri, proxy);
						OpenSsoPage(Api.ConnectionInfo.IdpServer);
					}
					// TODO: Perform authentication
				}
			}
			catch (RallyUnavailableException)
			{
				errorMessage = "Rally is currently unavailable.";
			}
			catch (WebException e)
			{
				if (e.Response is HttpWebResponse)
				{
					if ((((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.BadGateway) ||
						(((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.BadRequest))
					{
						errorMessage = LoginFailureBadServer;
					}
					else if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
					{
						errorMessage = LoginFailureProxyCredentials;
					}
					else if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
					{
						errorMessage = LoginFailureCredentials;
					}
					else
						errorMessage = LoginFailureUnknown;
				}
				else if ((e is WebException) &&
					(((WebException)e).Status == WebExceptionStatus.ConnectFailure))
				{
					errorMessage = LoginFailureBadConnection;
				}
				else
					errorMessage = LoginFailureUnknown;
			}

			if (AuthenticationStateChange != null)
			{
				switch (Api.AuthenticationState)
				{
					case RallyRestApi.AuthenticationResult.Authenticated:
						AuthenticationStateChange.Invoke(Api.AuthenticationState, Api);
						break;
					case RallyRestApi.AuthenticationResult.PendingSSO:
					case RallyRestApi.AuthenticationResult.NotAuthorized:
						AuthenticationStateChange.Invoke(Api.AuthenticationState, null);
						break;
					default:
						throw new NotImplementedException();
				}
			}
			return Api.AuthenticationState;
		}
		#endregion

		#region PerformLogoutFromRally
		/// <summary>
		/// Performs an logout from Rally.
		/// </summary>
		protected void PerformLogoutFromRally()
		{
			Api.Logout();
			AuthenticationStateChange.Invoke(Api.AuthenticationState, null);
		}
		#endregion
	}

	/// <summary>
	/// A delegate to indicate that the authenication state (logged in, logged out, pending SSO) has changed.
	/// </summary>
	/// <param name="authenticationResult">The status of authentication.</param>
	/// <param name="api">The authenticated API that can be used for the user who logged in.</param>
	public delegate void AuthenticationStateChange(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api);

	/// <summary>
	/// A delegate to indicate that SSO authentication has completed.
	/// </summary>
	/// <param name="authenticationResult">The status of authentication.</param>
	/// <param name="api">The authenticated API that can be used for the user who logged in.</param>
	public delegate void SsoAuthenticationComplete(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api);
}
