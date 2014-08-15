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
		RallyRestApi api = null;

		#region MainWindow
		public MainWindow()
		{
			InitializeComponent();
			serverInput.Text = RallyRestApi.DEFAULT_SERVER;
			wsapiInput.Text = RallyRestApi.DEFAULT_WSAPI_VERSION;
			LogoutAndResetUI();
		}
		#endregion

		// For this example, RallyRestApi SSO configuration is done in here
		#region loginButton_Click
		private void loginButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Create a new RallyRestApi object, providing the appropriate SSO provider. If no 
				// provider is passed to the constructor, no SSO authentication will be attempted.
				api = new RallyRestApi(new WpfSsoDriver(), wsapiInput.Text);
				// Bind to the event for SSO notifications.
				api.SsoResults += api_SsoResults;
				// Call the authenticate method. This will throw WebExceptions if authentication fails.
				api.Authenticate(userNameInput.Text, String.Empty, serverInput.Text);

				loginButton.Visibility = Visibility.Hidden;
				logoutButton.Visibility = Visibility.Visible;
			}
			catch (RallyUnavailableException)
			{
				MessageBox.Show("Rally is Offline");
			}
			catch (WebException we)
			{
				if (((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.Unauthorized)
					MessageBox.Show("(401) Unauthorized");
				else if (((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.NotFound)
					MessageBox.Show("(404) Not Found");
				else
					MessageBox.Show("Exception: {0}", we.Message);

				// Make sure we log the user out in case they are using stored credentials.
				LogoutAndResetUI();
			}
		}
		#endregion

		#region logoutButton_Click
		private void logoutButton_Click(object sender, RoutedEventArgs e)
		{
			LogoutAndResetUI();
		}
		#endregion

		#region LogoutAndResetUI
		private void LogoutAndResetUI()
		{
			api = null;
			loginButton.Visibility = Visibility.Visible;
			logoutButton.Visibility = Visibility.Hidden;
		}
		#endregion

		// SSO Handler Method
		#region api_SsoResults
		/// <summary>
		/// api_SsoResults is called by the RallyRestApi once SSO authentication is completed.
		/// </summary>
		/// <param name="success">If SSO was completed.</param>
		/// <param name="zSessionID">The ZSessionID that was provided from Rally after SSO authentication.</param>
		void api_SsoResults(bool success, string zSessionID)
		{
			if ((success) && (api != null))
			{
				zSessionIDLabel.Content = zSessionID;
				dynamic currentUser = api.GetCurrentUser();
				MessageBox.Show(String.Format("SSO Session started for {0}", currentUser["_refObjectName"]));
			}
			else
			{
				zSessionIDLabel.Content = String.Empty;
				LogoutAndResetUI();
				MessageBox.Show("Failed to authenticate with SSO");
			}
		}
		#endregion
	}
}
