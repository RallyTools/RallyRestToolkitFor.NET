using Rally.RestApi;
using Rally.RestApi.Exceptions;
using Rally.RestApi.UiForWinforms;
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

namespace Test.Rally.RestApi.UiForWinForms
{
	public partial class MainWindow : Form
	{
		RallyRestApi api = null;
		bool isLoggedIn = false;

		#region Constructor
		public MainWindow()
		{
			InitializeComponent();
			serverInput.Text = RallyRestApi.DEFAULT_SERVER;
			wsapiInput.Text = RallyRestApi.DEFAULT_WSAPI_VERSION;
			zSessionIDLabel.Text = String.Empty;
			LogoutAndResetUI();
		}
		#endregion

		// For this example, RallyRestApi SSO configuration is done in here
		#region actionButton_Click
		private void actionButton_Click(object sender, EventArgs e)
		{
			if (isLoggedIn)
			{
				LogoutAndResetUI();
			}
			else
			{
				try
				{
					// Create a new RallyRestApi object, providing the appropriate SSO provider. If no 
					// provider is passed to the constructor, no SSO authentication will be attempted.
					api = new RallyRestApi(new WinFormSsoDriver(), wsapiInput.Text);
					// Bind to the event for SSO notifications.
					api.SsoResults += api_SsoResults;
					// Call the authenticate method. This will throw WebExceptions if authentication fails.
					api.Authenticate(usernameInput.Text, String.Empty, serverInput.Text);
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
		}
		#endregion

		#region LogoutAndResetUI
		private void LogoutAndResetUI()
		{
			api = null;
			actionButton.Text = "Login";
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
				actionButton.Text = "Logout";
				zSessionIDLabel.Text = String.Format("ZSessionID: ", zSessionID);
				dynamic currentUser = api.GetCurrentUser();
				MessageBox.Show(String.Format("SSO Session started for {0}", currentUser["_refObjectName"]));
			}
			else
			{
				actionButton.Text = "Login";
				zSessionIDLabel.Text = String.Empty;
				LogoutAndResetUI();
				MessageBox.Show("Failed to authenticate with SSO");
			}
		}
		#endregion
	}
}
