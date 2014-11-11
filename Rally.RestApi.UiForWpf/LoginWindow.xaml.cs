using Rally.RestApi.Auth;
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
using System.Windows.Shapes;

namespace Rally.RestApi.UiForWpf
{
	/// <summary>
	/// Interaction logic for LoginWindow.xaml
	/// </summary>
	public partial class LoginWindow : Window
	{
		#region Enum: TabType
		private enum TabType
		{
			Credentials,
			Server,
		}
		#endregion

		#region Enum: EditorControlType
		private enum EditorControlType
		{
			Username,
			Password,
			Server,
			ProxyServer,
			ProxyUsername,
			ProxyPassword,
		}
		#endregion

		#region Static Labels
		private static ImageSource LogoImage;
		private static string HeaderLabelText;
		private static string CredentialsTabText;
		private static string ServerTabText;

		private static string UserNameLabelText;
		private static string PwdLabelText;

		private static string ServerLabelText;
		private static string ProxyServerLabelText;
		private static string ProxyUserNameLabelText;
		private static string ProxyPwdLabelText;

		private static string LoginText;
		private static string LogoutText;
		private static string CancelText;
		#endregion

		Dictionary<EditorControlType, Control> controls;

		internal RestApiAuthMgrWpf AuthMgr { get; set; }
		internal event AuthenticationComplete AuthenticationComplete;

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public LoginWindow()
		{
			InitializeComponent();

			Logo.Source = LogoImage;
			headerLabel.Content = HeaderLabelText;
			controls = new Dictionary<EditorControlType, Control>();
			BuildDynamicLayout();
		}
		#endregion

		#region Configure
		/// <summary>
		/// <para>Configure this control with the items that it needs to work.</para>
		/// <para>Nullable parameters have defaults that will be used if not provided.</para>
		/// </summary>
		internal static void Configure(ImageSource logo, string headerLabelText,
			string credentialsTabText = null, string userNameLabelText = null, string pwdLabelText = null,
			string serverTabText = null, string serverLabelText = null, string proxyServerLabelText = null,
			string proxyUserNameLabelText = null, string proxyPwdLabelText = null,
			string loginText = null, string logoutText = null, string cancelText = null)
		{
			LogoImage = logo;
			HeaderLabelText = headerLabelText;
			CredentialsTabText = credentialsTabText;
			UserNameLabelText = userNameLabelText;
			PwdLabelText = pwdLabelText;

			ServerTabText = serverTabText;
			ServerLabelText = serverLabelText;
			ProxyServerLabelText = proxyServerLabelText;
			ProxyUserNameLabelText = proxyUserNameLabelText;
			ProxyPwdLabelText = proxyPwdLabelText;

			LoginText = loginText;
			LogoutText = logoutText;
			CancelText = cancelText;

			#region Default Strings: Credentials
			if (String.IsNullOrWhiteSpace(CredentialsTabText))
				CredentialsTabText = "Credentials";

			if (String.IsNullOrWhiteSpace(UserNameLabelText))
				UserNameLabelText = "User Name";

			if (String.IsNullOrWhiteSpace(PwdLabelText))
				PwdLabelText = "Password";
			#endregion

			#region Default Strings: Connection
			if (String.IsNullOrWhiteSpace(ServerTabText))
				ServerTabText = "Connection";

			if (String.IsNullOrWhiteSpace(ServerLabelText))
				ServerLabelText = "Server";

			if (String.IsNullOrWhiteSpace(ProxyServerLabelText))
				ProxyServerLabelText = "Proxy Server";

			if (String.IsNullOrWhiteSpace(ProxyUserNameLabelText))
				ProxyUserNameLabelText = "Proxy User Name";

			if (String.IsNullOrWhiteSpace(ProxyPwdLabelText))
				ProxyPwdLabelText = "Proxy Password";
			#endregion

			#region Default Strings: Buttons
			if (String.IsNullOrWhiteSpace(LoginText))
				LoginText = "Login";

			if (String.IsNullOrWhiteSpace(LogoutText))
				LogoutText = "Logout";

			if (String.IsNullOrWhiteSpace(CancelText))
				CancelText = "Cancel";
			#endregion
		}
		#endregion

		#region AddTabControls
		private void BuildDynamicLayout()
		{
			TabControl tabControl = new TabControl();
			tabControl.Margin = new Thickness(10);
			Grid.SetColumn(tabControl, 0);
			Grid.SetColumnSpan(tabControl, 2);
			Grid.SetRow(tabControl, 1);
			layoutGrid.Children.Add(tabControl);

			AddTab(tabControl, TabType.Credentials);
			AddTab(tabControl, TabType.Server);

			inputRow.Height = new GridLength(tabControl.Height + 35, GridUnitType.Pixel);
			inputRow.MinHeight = inputRow.Height.Value;

			this.Height = inputRow.Height.Value + (28 * 2) + 50 + 50;
			this.MinHeight = this.Height;
			this.MaxHeight = this.Height;

			AddButtons();
		}
		#endregion

		#region AddTab
		private void AddTab(TabControl tabControl, TabType tabType)
		{
			TabItem tab = new TabItem();
			tabControl.Items.Add(tab);

			Grid tabGrid = new Grid();
			tab.Content = tabGrid;
			tabGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
			tabGrid.VerticalAlignment = VerticalAlignment.Top;
			AddColumnDefinition(tabGrid, 120);
			AddColumnDefinition(tabGrid);

			if (tabType == TabType.Credentials)
			{
				tab.Header = CredentialsTabText;
				AddInputToTabGrid(tabGrid, UserNameLabelText, GetEditor(EditorControlType.Username));
				AddInputToTabGrid(tabGrid, PwdLabelText, GetEditor(EditorControlType.Password));
			}
			else if (tabType == TabType.Server)
			{
				tab.Header = ServerTabText;
				AddInputToTabGrid(tabGrid, ServerLabelText, GetEditor(EditorControlType.Server));
				AddInputToTabGrid(tabGrid, ProxyServerLabelText, GetEditor(EditorControlType.ProxyServer));
				AddInputToTabGrid(tabGrid, ProxyUserNameLabelText, GetEditor(EditorControlType.ProxyUsername));
				AddInputToTabGrid(tabGrid, ProxyPwdLabelText, GetEditor(EditorControlType.ProxyPassword));
			}
			else
				throw new NotImplementedException();

			if ((tabControl.Height.ToString().Equals("NaN", StringComparison.InvariantCultureIgnoreCase)) ||
				(tabControl.Height < tabGrid.Height + 20))
			{
				tabControl.Height = tabGrid.Height + 20;
				tabControl.MinHeight = tabControl.Height;
			}
		}
		#endregion

		#region AddInputToTabGrid
		private void AddInputToTabGrid(Grid tabGrid, string labelText, Control control)
		{
			int rowIndex = tabGrid.RowDefinitions.Count;
			AddRowDefinition(tabGrid, 28);
			Label label = new Label();
			label.Content = labelText;
			AddControlToGrid(tabGrid, label, rowIndex, 0);

			if (control != null)
				AddControlToGrid(tabGrid, control, rowIndex, 1);
		}
		#endregion

		#region GetEditor
		private Control GetEditor(EditorControlType controlType)
		{
			Control control = null;
			if (controls.ContainsKey(controlType))
				control = controls[controlType];
			else
			{
				switch (controlType)
				{
					case EditorControlType.Username:
					case EditorControlType.Server:
					case EditorControlType.ProxyServer:
					case EditorControlType.ProxyUsername:
						TextBox textBox = new TextBox();
						control = textBox;
						break;
					case EditorControlType.Password:
					case EditorControlType.ProxyPassword:
						PasswordBox passwordBox = new PasswordBox();
						passwordBox.PasswordChar = '*';
						control = passwordBox;
						break;
					default:
						throw new NotImplementedException();
				}

				control.Margin = new Thickness(0, 0, 10, 0);
				control.HorizontalAlignment = HorizontalAlignment.Stretch;
				control.MinWidth = 150;
				control.Height = 20;
				controls.Add(controlType, control);
			}

			return control;
		}
		#endregion

		#region GetEditorValue
		private string GetEditorValue(EditorControlType controlType)
		{
			Control control = GetEditor(controlType);
			if (control == null)
				return null;

			TextBox textBox = control as TextBox;
			if (textBox != null)
				return textBox.Text;

			PasswordBox passwordBox = control as PasswordBox;
			if (passwordBox != null)
				return passwordBox.Password;

			return null;
		}
		#endregion

		#region AddButtons
		private void AddButtons()
		{
			Grid buttonGrid = new Grid();
			Grid.SetColumn(buttonGrid, 1);
			Grid.SetRow(buttonGrid, 4);
			layoutGrid.Children.Add(buttonGrid);

			AddColumnDefinition(buttonGrid, 70);
			AddColumnDefinition(buttonGrid, 70);
			AddColumnDefinition(buttonGrid);

			Thickness margin = new Thickness(5, 0, 5, 0);

			Button loginButton = new Button();
			loginButton.Margin = margin;
			loginButton.IsDefault = true;
			loginButton.Content = LoginText;
			loginButton.Click += loginButton_Click;
			AddControlToGrid(buttonGrid, loginButton, 0, 0);

			Button logoutButton = new Button();
			logoutButton.Margin = margin;
			logoutButton.IsDefault = true;
			logoutButton.Content = LogoutText;
			logoutButton.Click += logoutButton_Click;
			AddControlToGrid(buttonGrid, logoutButton, 0, 0);

			Button cancelButton = new Button();
			cancelButton.Margin = margin;
			cancelButton.IsDefault = true;
			cancelButton.Content = CancelText;
			cancelButton.Click += cancelButton_Click;
			AddControlToGrid(buttonGrid, cancelButton, 0, 1);
		}
		#endregion

		#region AddControlToGrid
		private void AddControlToGrid(Grid grid, UIElement control, int row, int column, int rowSpan = 1, int colSpan = 1)
		{
			if (row >= 0)
				Grid.SetRow(control, row);
			if (rowSpan > 1)
				Grid.SetRowSpan(control, rowSpan);

			if (column >= 0)
				Grid.SetColumn(control, column);
			if (colSpan > 1)
				Grid.SetColumnSpan(control, colSpan);

			grid.Children.Add(control);
		}
		#endregion

		#region AddRowDefinition
		private void AddRowDefinition(Grid grid, int pixels = Int32.MaxValue)
		{
			RowDefinition rowDef = new RowDefinition();
			if (pixels == Int32.MaxValue)
				rowDef.Height = GridLength.Auto;
			else
				rowDef.Height = new GridLength(pixels, GridUnitType.Pixel);
			grid.RowDefinitions.Add(rowDef);

			if (pixels != Int32.MaxValue)
			{
				grid.MinHeight += pixels + 2;
				grid.Height = grid.MinHeight;
			}
			else
				grid.Height = double.NaN;
		}
		#endregion

		#region AddColumnDefinition
		private void AddColumnDefinition(Grid grid, int pixels = Int32.MaxValue)
		{
			ColumnDefinition colDef = new ColumnDefinition();
			if (pixels != Int32.MaxValue)
				colDef.Width = new GridLength(pixels, GridUnitType.Pixel);

			grid.ColumnDefinitions.Add(colDef);
		}
		#endregion

		#region loginButton_Click
		void loginButton_Click(object sender, RoutedEventArgs e)
		{
			WebProxy proxy = null;
			string proxyServer = GetEditorValue(EditorControlType.ProxyServer);
			if (!String.IsNullOrWhiteSpace(proxyServer))
			{
				proxy = new WebProxy(new Uri(proxyServer));
				string proxyUser = GetEditorValue(EditorControlType.ProxyUsername);
				string proxyPassword = GetEditorValue(EditorControlType.ProxyUsername);
				if (!String.IsNullOrWhiteSpace(proxyUser))
					proxy.Credentials = new NetworkCredential(proxyUser, proxyPassword);
				else
					proxy.UseDefaultCredentials = true;
			}

			AuthMgr.Api.Authenticate(GetEditorValue(EditorControlType.Username), GetEditorValue(EditorControlType.Password),
				GetEditorValue(EditorControlType.Server), proxy);
			RallyRestApi.AuthenticationResult authResult = RallyRestApi.AuthenticationResult.NotAuthorized;

			if (AuthenticationComplete != null)
			{
				switch (authResult)
				{
					case RallyRestApi.AuthenticationResult.Authenticated:
						AuthenticationComplete.Invoke(authResult, AuthMgr.Api);
						break;
					case RallyRestApi.AuthenticationResult.PendingSSO:
					case RallyRestApi.AuthenticationResult.NotAuthorized:
						AuthenticationComplete.Invoke(authResult, null);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			throw new NotImplementedException();
		}
		#endregion

		#region logoutButton_Click
		void logoutButton_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region cancelButton_Click
		void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion
	}
}
