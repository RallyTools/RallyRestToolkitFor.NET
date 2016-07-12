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
		/// <param name="applicationToken">An application token to be used as the file name to store data as. Each 
		/// consuming application should use a unique name in order to ensure that the user credentials are not 
		/// overwritten by other applications.</param>
		/// <param name="encryptionKey">The encryption key, or salt, to be used for any encryption routines. This salt 
		/// should be different for each user, and not the same for everyone consuming the same application. Only used 
		/// for UI support.</param>
		/// <param name="encryptionRoutines">The encryption routines to use for encryption/decryption of data. Only used for UI support.</param>
		/// <param name="webServiceVersion">The version of the WSAPI API to use.</param>
		/// <param name="traceInfo">Controls diagnostic trace information being logged</param>
		public RestApiAuthMgrWinforms(string applicationToken, string encryptionKey,
			IEncryptionRoutines encryptionRoutines, string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION, TraceFieldEnum traceInfo = RallyRestApi.DEFAULT_TRACE_FIELDS)
			: base(true, applicationToken, encryptionKey, encryptionRoutines, webServiceVersion, traceInfo)
		{
		}
		#endregion

		#region SetLogo
		/// <summary>
		/// Sets the logo used in the user controls.
		/// </summary>
		/// <param name="logo">The image to use as a logo.</param>
		/// <example>
		/// <code language="C#">
		/// // ImageResources is a resource file that the logo has been added to.
		/// RestApiAuthMgrWpf.SetLogo(ImageResources.Logo);
		/// </code>
		/// </example>
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

		#region ReportSsoResults
		/// <summary>
		/// Reports the results of an SSO action.
		/// </summary>
		/// <param name="success">Was SSO authentication completed successfully?</param>
		/// <param name="rallyServer">The server that the ZSessionID is for.</param>
		/// <param name="zSessionID">The zSessionID that was returned from Rally.</param>
		internal void ReportSsoResultsToMgr(bool success, string rallyServer, string zSessionID)
		{
			ReportSsoResults(success, rallyServer, zSessionID);
		}
		#endregion

		#region NotifyLoginWindowSsoComplete
		/// <summary>
		/// Notifies the login window that SSO has been completed.
		/// </summary>
		/// <param name="authenticationResult">The current state of the authentication process. <see cref="RallyRestApi.AuthenticationResult"/></param>
		/// <param name="api">The API that was authenticated against.</param>
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
		internal RallyRestApi.AuthenticationResult PerformAuthenticationCheck(out string errorMessage)
		{
			return base.PerformAuthenticationCheck(out errorMessage, true);
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
