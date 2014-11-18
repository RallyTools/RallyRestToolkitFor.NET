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
	public class RestApiAuthMgrWpf : ApiAuthManager
	{
		static Dictionary<CustomWpfControlType, Type> customControlTypes = new Dictionary<CustomWpfControlType, Type>();
		static ImageSource logoForUi = null;
		LoginWindow loginControl = null;
		internal SsoAuthenticationComplete LoginWindowSsoAuthenticationComplete;

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public RestApiAuthMgrWpf(string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
			: base(true, webServiceVersion)
		{
		}
		#endregion

		#region SetLogo
		/// <summary>
		/// Sets the logo used in the user controls.
		/// </summary>
		public static void SetLogo(ImageSource logo)
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

		#region SetCustomControlType
		/// <summary>
		/// Sets a custom control for the specified control type. Please see the
		/// enumeration help for what the types must extend from in order to work.
		/// </summary>
		public static void SetCustomControlType(CustomWpfControlType customWpfControlType, Type type)
		{
			if (customControlTypes.ContainsKey(customWpfControlType))
				customControlTypes[customWpfControlType] = type;
			else
				customControlTypes.Add(customWpfControlType, type);
		}
		#endregion

		#region GetCustomControlType
		/// <summary>
		/// Gets a custom control for the specified control type. 
		/// </summary>
		internal static Type GetCustomControlType(CustomWpfControlType customWpfControlType)
		{
			if (customControlTypes.ContainsKey(customWpfControlType))
				return customControlTypes[customWpfControlType];

			return null;
		}
		#endregion
	}
}
