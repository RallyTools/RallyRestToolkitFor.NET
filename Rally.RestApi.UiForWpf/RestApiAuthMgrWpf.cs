using Rally.RestApi.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Rally.RestApi.Connection;

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
		/// <param name="applicationToken">An application token to be used as the file name to store data as. Each 
		/// consuming application should use a unique name in order to ensure that the user credentials are not 
		/// overwritten by other applications.</param>
		/// <param name="encryptionKey">The encryption key, or salt, to be used for any encryption routines. This salt 
		/// should be different for each user, and not the same for everyone consuming the same application. Only used 
		/// for UI support.</param>
		/// <param name="encryptionRoutines">The encryption routines to use for encryption/decryption of data. Only used for UI support.</param>
		/// <param name="webServiceVersion">The version of the WSAPI API to use.</param>
		public RestApiAuthMgrWpf(string applicationToken, string encryptionKey,
			IEncryptionRoutines encryptionRoutines, string webServiceVersion = RallyRestApi.DEFAULT_WSAPI_VERSION)
			: base(true, applicationToken, encryptionKey, encryptionRoutines, webServiceVersion)
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
			try
			{
				SsoWindow window = new SsoWindow();
				window.ShowSsoPage(this, ssoUrl);
			}
			catch
			{
				SsoWindow window = null;
				Application.Current.Dispatcher.Invoke(delegate() { window = new SsoWindow(); });
				if (window != null)
				{
					Application.Current.Dispatcher.Invoke(delegate() { window.ShowSsoPage(this, ssoUrl); });
				}
			}
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
		internal new RallyRestApi.AuthenticationResult PerformAuthenticationCheck(out string errorMessage)
		{
			return base.PerformAuthenticationCheck(out errorMessage);
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
