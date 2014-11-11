using Rally.RestApi;
using Rally.RestApi.Exceptions;
using Rally.RestApi.UiForWpf;
using System;
using System.Collections.Generic;
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

namespace Test.Rally.RestApi.UiForWpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		RestApiAuthMgrWpf authMgr;

		#region MainWindow
		public MainWindow()
		{
			InitializeComponent();
			headerLabel.Text = "Login Window Example";
			Configure();
		}
		#endregion

		#region Configure
		private void Configure()
		{
			RestApiAuthMgrWpf.Configure(true, null, headerLabel.Text, credentialsTabLabel.Text,
				usernameLabel.Text, passwordLabel.Text, serverTabLabel.Text, serverLabel.Text,
				proxyServerLabel.Text, proxyUsernameLabel.Text, proxyPasswordLabel.Text,
				loginButtonLabel.Text, logoutButtonLabel.Text, cancelButtonLabel.Text);
			authMgr = new RestApiAuthMgrWpf();
		}
		#endregion

		#region reconfigure_Click
		private void reconfigure_Click(object sender, RoutedEventArgs e)
		{
			Configure();
		}
		#endregion

		#region openLogin_Click
		private void openLogin_Click(object sender, RoutedEventArgs e)
		{
			authMgr.ShowUserLoginWindow(AuthenticationComplete, SsoAuthenticationComplete);
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
