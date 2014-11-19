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
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		RestApiAuthMgrWinforms winFormsAuthMgr;
		RestApiAuthMgrWpf wpfAuthMgr;

		#region MainWindow
		public MainWindow()
		{
			InitializeComponent();
			headerLabel.Text = "Login Window Example";

			// Initiate authorization managers
			winFormsAuthMgr = new RestApiAuthMgrWinforms();
			wpfAuthMgr = new RestApiAuthMgrWpf();

			UpdateAuthenticationResults(RallyRestApi.AuthenticationResult.NotAuthorized, null);
			defaultServerUri.Text = RallyRestApi.DEFAULT_SERVER;
		}
		#endregion

		#region reconfigure_Click
		private void reconfigure_Click(object sender, RoutedEventArgs e)
		{
			Uri defaultProxyServer = null;
			if (!String.IsNullOrWhiteSpace(defaultProxyServerUri.Text))
				defaultProxyServer = new Uri(defaultProxyServerUri.Text);

			// Initiate new authorization managers
			winFormsAuthMgr = new RestApiAuthMgrWinforms();
			wpfAuthMgr = new RestApiAuthMgrWpf();

			UpdateAuthenticationResults(RallyRestApi.AuthenticationResult.NotAuthorized, null);

			// Configure labels for UI
			ApiAuthManager.Configure(windowTitleLabel.Text, headerLabel.Text,
				credentialsTabLabel.Text, usernameLabel.Text, passwordLabel.Text,
				serverTabLabel.Text, serverLabel.Text, new Uri(defaultServerUri.Text),
				proxyTabLabel.Text, proxyServerLabel.Text, proxyUsernameLabel.Text,
				proxyPasswordLabel.Text, defaultProxyServer,
				loginWindowSsoInProgressLabel.Text,
				loginButtonLabel.Text, logoutButtonLabel.Text, cancelButtonLabel.Text);

			RestApiAuthMgrWinforms.SetLogo(ImageResources.RallyLogo40x40);
			RestApiAuthMgrWpf.SetLogo(GetImageSource(ImageResources.RallyLogo40x40));

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

		#region openWpfLogin_Click
		private void openWpfLogin_Click(object sender, RoutedEventArgs e)
		{
			wpfAuthMgr.ShowUserLoginWindow(AuthenticationComplete, SsoAuthenticationComplete);
		}
		#endregion

		#region openWinFormsLogin_Click
		private void openWinFormsLogin_Click(object sender, RoutedEventArgs e)
		{
			winFormsAuthMgr.ShowUserLoginWindow(AuthenticationComplete, SsoAuthenticationComplete);
		}
		#endregion

		#region AuthenticationComplete
		private void AuthenticationComplete(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			UpdateAuthenticationResults(authenticationResult, api);
		}
		#endregion

		#region SsoAuthenticationComplete
		private void SsoAuthenticationComplete(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			UpdateAuthenticationResults(authenticationResult, api);
		}
		#endregion

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
	}
}
