using Rally.RestApi;
using Rally.RestApi.Auth;
using Rally.RestApi.Exceptions;
using Rally.RestApi.UiForWinforms;
using Rally.RestApi.UiForWpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Test.Rally.RestApi.UiSample.Images;
using media = System.Windows.Media;
using System.Windows.Interop;
using Test.Rally.RestApi.UiSample.CustomControls;

namespace Test.Rally.RestApi.UiSample
{
	/// <summary>
	/// A sample application that shows how the UI authentication mangagers can be used. 
	/// Look for // HELP: tags in code for assistance on how to use.
	/// </summary>
	public partial class MainWindow : Window
	{
		// HELP: Declare an authentication manager.
		RestApiAuthMgrWinforms winFormsAuthMgr;
		RestApiAuthMgrWpf wpfAuthMgr;

		// HELP: Define your encryption items prior to instantiating authorization managers
		string applicationToken = "RallyRestAPISample";
		string encryptionKey = "UserSpecificSaltForEncryption";
		IEncryptionRoutines encryptionUtilities = new EncryptionUtilities();

		#region MainWindow
		public MainWindow()
		{
			InitializeComponent();
			headerLabel.Text = "Login Window Example";

			// HELP: Instantiate authorization managers
			winFormsAuthMgr = new RestApiAuthMgrWinforms(applicationToken, encryptionKey, encryptionUtilities);
			wpfAuthMgr = new RestApiAuthMgrWpf(applicationToken, encryptionKey, encryptionUtilities);

			// Help: Set logos. Any use of Rally's Logo is governed by your agreements with Rally.
			RestApiAuthMgrWinforms.SetLogo(ImageResources.RallyLogo40x40);
			RestApiAuthMgrWpf.SetLogo(GetImageSource(ImageResources.RallyLogo40x40),
				GetImageSource(ImageResources.RallyLogo40x40));


			// Help: You can auto-authenticate if you want. We do not have it enabled for this application.
			// wpfAuthMgr.AutoAuthenticate(true);

			UpdateAuthenticationResults(RallyRestApi.AuthenticationResult.NotAuthorized, null);
		}
		#endregion

		// HELP: This method has some setup code that may interest you.
		#region reconfigure_Click
		private void reconfigure_Click(object sender, RoutedEventArgs e)
		{
			Uri defaultProxyServer = null;
			if (!String.IsNullOrWhiteSpace(defaultProxyServerUri.Text))
				defaultProxyServer = new Uri(defaultProxyServerUri.Text);

			// HELP: This is for demo purposes only. We are clearing all known data in the authentication managers by doing this.
			winFormsAuthMgr = new RestApiAuthMgrWinforms(applicationToken, encryptionKey, encryptionUtilities);
			wpfAuthMgr = new RestApiAuthMgrWpf(applicationToken, encryptionKey, encryptionUtilities);

			UpdateAuthenticationResults(RallyRestApi.AuthenticationResult.NotAuthorized, null);

			// HELP: Configure labels for UI. These are global and used by both authentication managers to build their UI.
			// If this is not called, the default labels will be used.
			ApiAuthManager.Configure(loginWindowServerLabelText: "My Updated Server Label",
				loginWindowDefaultServer: new Uri("http://onprem.rally.url"));

			Uri defaultServer = null;
			if (!String.IsNullOrWhiteSpace(defaultServerUri.Text))
				defaultServer = new Uri(defaultServerUri.Text);

			ApiAuthManager.Configure(windowTitleLabel.Text, headerLabel.Text,
				credentialsTabLabel.Text, usernameLabel.Text, passwordLabel.Text,
				serverTabLabel.Text, String.Empty, serverLabel.Text, String.Empty, defaultServer,
				proxyTabLabel.Text, proxyServerLabel.Text, proxyUsernameLabel.Text,
				proxyPasswordLabel.Text, defaultProxyServer,
				loginWindowSsoInProgressLabel.Text,
				loginButtonLabel.Text, logoutButtonLabel.Text, cancelButtonLabel.Text);

			// HELP: If you need to use custom controls (Ex: from a third party vendor), you can set them using this code snippet.
			// This triggers a global change for the next time a window is created.
			if ((useCustomControls.IsChecked.HasValue) && (useCustomControls.IsChecked.Value))
			{
				RestApiAuthMgrWpf.SetCustomControlType(CustomWpfControlType.Buttons, typeof(CustomButton));
				RestApiAuthMgrWpf.SetCustomControlType(CustomWpfControlType.TabControl, typeof(CustomTabControl));
				RestApiAuthMgrWpf.SetCustomControlType(CustomWpfControlType.TabItem, typeof(CustomTabItem));
			}
		}
		#endregion

		#region GetImageSource
		/// <summary>
		/// Helper to convert a BitMap to a WPF ImageSource.
		/// </summary>
		public static media.ImageSource GetImageSource(Bitmap image)
		{
			return Imaging.CreateBitmapSourceFromHBitmap(
												image.GetHbitmap(),
												IntPtr.Zero,
												Int32Rect.Empty,
												BitmapSizeOptions.FromEmptyOptions());
		}
		#endregion

		// HELP: This method shows you how to open a login window.
		#region openWpfLogin_Click
		private void openWpfLogin_Click(object sender, RoutedEventArgs e)
		{
			// HELP: Delegates are provided so we can be notified that authentication or SSO authentication has completed.
			wpfAuthMgr.ShowUserLoginWindow(AuthenticationComplete, SsoAuthenticationComplete);
		}
		#endregion

		// HELP: This method shows you how to open a login window.
		#region openWinFormsLogin_Click
		private void openWinFormsLogin_Click(object sender, RoutedEventArgs e)
		{
			// HELP: Delegates are provided so we can be notified that authentication or SSO authentication has completed.
			winFormsAuthMgr.ShowUserLoginWindow(AuthenticationComplete, SsoAuthenticationComplete);
		}
		#endregion

		// HELP: This delegate notifies us that authentication has completed.
		#region AuthenticationStateChange
		private void AuthenticationComplete(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			UpdateAuthenticationResults(authenticationResult, api);
		}
		#endregion

		// HELP: This delegate notifies us that SSO authentication has completed.
		#region SsoAuthenticationComplete
		private void SsoAuthenticationComplete(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			UpdateAuthenticationResults(authenticationResult, api);
		}
		#endregion

		// HELP: This method handles the passthrough from the delegates.
		// This is where you would need to update your application to show the logged in state.
		#region UpdateAuthenticationResults
		private void UpdateAuthenticationResults(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			authResultLabel.Content = authenticationResult.ToString();
			if (api != null)
			{
				authTypeLabel.Content = api.ConnectionInfo.AuthType.ToString();
				zSessionIDLabel.Content = api.ConnectionInfo.ZSessionID;
			}
			else
			{
				authTypeLabel.Content = "None";
				zSessionIDLabel.Content = String.Empty;
			}
		}
		#endregion

		private void deleteFileContents_Click(object sender, RoutedEventArgs e)
		{
			bool result = wpfAuthMgr.DeleteCachedLoginDetailsFromDisk();
			if (!result)
			{
				MessageBox.Show("Failed to delete. Please try again later");
				return;
			}
			result = winFormsAuthMgr.DeleteCachedLoginDetailsFromDisk();
			if (!result)
			{
				MessageBox.Show("Failed to delete. Please try again later");
				return;
			}

			MessageBox.Show("File deleted. Please shutdown this application now.");
		}
	}
}
