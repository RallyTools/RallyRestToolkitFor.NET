using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Rally.RestApi.UiForWpf
{
	/// <summary>
	/// Interaction logic for LoginControl.xaml
	/// </summary>
	public partial class LoginControl : UserControl
	{
		RoutedEventHandler loginButtonClick;
		RoutedEventHandler logoutButtonClick;
		RoutedEventHandler cancelButtonClick;

		/// <summary>
		/// The value that is in the server name input box.
		/// </summary>
		public string ServerName { get { return serverInput.Text; } set { serverInput.Text = value; } }
		/// <summary>
		/// The value that is in the username input box.
		/// </summary>
		public string Username { get { return serverInput.Text; } set { serverInput.Text = value; } }
		/// <summary>
		/// The value that is in the password input box.
		/// </summary>
		public string Password { get { return serverInput.Text; } set { serverInput.Text = value; } }


		/// <summary>
		/// Constructor
		/// </summary>
		[Obsolete("Constructor used by designer only.", true)]
		public LoginControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public LoginControl(ImageSource logo, string headerLabelText, RoutedEventHandler loginButtonClick, RoutedEventHandler logoutButtonClick, 
			RoutedEventHandler cancelButtonClick = null, string serverLabelText = null, string userNameLabelText = null, string pwdLabelText = null,
			string loginText = null, string logoutText = null)
		{
			if (loginButtonClick == null)
				throw new ArgumentNullException("loginButtonClick", "You must provide a callback for the login button.");

			if (logoutButtonClick == null)
				throw new ArgumentNullException("logoutButtonClick", "You must provide a callback for the logout button.");
			
			InitializeComponent();

			Logo.Source = logo;
			headerLabel.Content = headerLabelText;

			if (!String.IsNullOrWhiteSpace(serverLabelText))
				serverLabel.Content = serverLabelText;
			if (!String.IsNullOrWhiteSpace(userNameLabelText))
				userNameLabel.Content = userNameLabelText;
			if (!String.IsNullOrWhiteSpace(pwdLabelText))
				pwdLabel.Content = pwdLabelText;
			if (!String.IsNullOrWhiteSpace(pwdLabelText))
				pwdLabel.Content = pwdLabelText;
			if (!String.IsNullOrWhiteSpace(loginText))
				loginButton.Content = loginText;
			if (!String.IsNullOrWhiteSpace(logoutText))
				logoutButton.Content = logoutText;

			this.loginButtonClick = loginButtonClick;
			this.logoutButtonClick = logoutButtonClick;

			loginButton.Click += loginButton_Click;
			logoutButton.Click += logoutButton_Click;

			if (cancelButtonClick != null)
			{
				cancelButton.Click += cancelButton_Click;
				this.cancelButtonClick = cancelButtonClick;
				cancelButton.Visibility = Visibility.Visible;
			}
			else
			{
				cancelButton.Visibility = Visibility.Hidden;
			}
		}

		void loginButton_Click(object sender, RoutedEventArgs e)
		{
			logoutButtonClick.Invoke(sender, e);
		}

		void logoutButton_Click(object sender, RoutedEventArgs e)
		{
			logoutButtonClick.Invoke(sender, e);
		}

		void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			if (cancelButtonClick != null)
				cancelButtonClick.Invoke(sender, e);
		}
	}
}
