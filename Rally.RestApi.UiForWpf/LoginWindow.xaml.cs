using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Rally.RestApi.Auth;
using Rally.RestApi.Connection;

namespace Rally.RestApi.UiForWpf
{

	/// <summary>
	/// Interaction logic for LoginWindow.xaml
	/// </summary>
	internal partial class LoginWindow : Window
	{
		#region Enum: TabType
		private enum TabType
		{
			Credentials,
			Rally,
			Proxy
		}
		#endregion

		#region Enum: EditorControlType
		private enum EditorControlType
		{
			Username,
			Password,
			ConnectionType,
			RallyServer,
			IdpServer,
			TrustAllCertificates,
			ProxyServer,
			ProxyUsername,
			ProxyPassword
		}
		#endregion

		private static int ROW_HEIGHT = 28;

		Dictionary<EditorControlType, Control> controls;
		Dictionary<Control, Label> controlReadOnlyLabels;
		Dictionary<TabType, Control> tabControls;
		Dictionary<ConnectionType, string> connectionTypes;
		Dictionary<EditorControlType, RowDefinition> controlRowElements;

		internal RestApiAuthMgrWpf AuthMgr { get; set; }
		Selector tabControl;
		HeaderedContentControl rallyTab;
		Button loginButton;
		Button logoutButton;
		Button cancelButton;

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public LoginWindow()
		{
			InitializeComponent();

			RestApiAuthMgrWpf.AllowIdpBasedSso = true;
			headerLabel.Content = ApiAuthManager.LoginWindowHeaderLabelText;
			controls = new Dictionary<EditorControlType, Control>();
			controlReadOnlyLabels = new Dictionary<Control, Label>();
			tabControls = new Dictionary<TabType, Control>();
			controlRowElements = new Dictionary<EditorControlType, RowDefinition>();

			connectionTypes = new Dictionary<ConnectionType, string>();
            connectionTypes.Add(ConnectionType.BasicAuth, "Basic Authentication (will try CA Agile Central SSO if it fails)");
            connectionTypes.Add(ConnectionType.SpBasedSso, "CA Agile Central based SSO Authentication");
			connectionTypes.Add(ConnectionType.IdpBasedSso, "IDP Based SSO Authentication");
		}
		#endregion

		internal void SetLogo(ImageSource logo, ImageSource iconForUi)
		{
			if (logo != null)
				Logo.Source = logo;

			if (iconForUi != null)
				Icon = iconForUi;
		}

		#region UpdateLoginState
		/// <summary>
		/// Updates the login state to show the correct buttons.
		/// </summary>
		internal void UpdateLoginState()
		{
			bool isReadOnly = true;
			switch (AuthMgr.Api.AuthenticationState)
			{
				case RallyRestApi.AuthenticationResult.Authenticated:
					loginButton.Visibility = Visibility.Hidden;
					logoutButton.Visibility = Visibility.Visible;
					ShowMessage();
					break;
				case RallyRestApi.AuthenticationResult.PendingSSO:
					loginButton.Visibility = Visibility.Hidden;
					logoutButton.Visibility = Visibility.Hidden;
					ShowMessage(ApiAuthManager.LoginWindowSsoInProgressText);
					break;
				case RallyRestApi.AuthenticationResult.NotAuthorized:
					loginButton.Visibility = Visibility.Visible;
					logoutButton.Visibility = Visibility.Hidden;
					isReadOnly = false;
					break;
				default:
					throw new InvalidProgramException("Unknown authentication state.");
			}

			SetReadOnlyStateForEditors(isReadOnly);
		}
		#endregion

		#region BuildLayout
		internal void BuildLayout(RestApiAuthMgrWpf authMgr)
		{
			Title = ApiAuthManager.LoginWindowTitle;
			AuthMgr = authMgr;

			tabControl = GetTabControl();
			tabControl.Margin = new Thickness(10, 10, 10, 5);
			Grid.SetColumn(tabControl, 0);
			Grid.SetColumnSpan(tabControl, 3);
			Grid.SetRow(tabControl, 1);
			layoutGrid.Children.Add(tabControl);

			AddTab(tabControl, TabType.Credentials);
			AddTab(tabControl, TabType.Rally);
			AddTab(tabControl, TabType.Proxy);

			AddButtons();

			inputRow.Height = new GridLength(tabControl.Height + 20, GridUnitType.Pixel);
			inputRow.MinHeight = inputRow.Height.Value;

			Height = inputRow.Height.Value + (28 * 2) + 100;
			MinHeight = Height;
			MaxHeight = Height;

			SetDefaultValues();
			ConnectionTypeChanged(GetEditor(EditorControlType.ConnectionType), null);
		}
		#endregion

		#region GetTabControl
		private Selector GetTabControl()
		{
			Selector selector;

			Type tabControlType = RestApiAuthMgrWpf.GetCustomControlType(CustomWpfControlType.TabControl);
			if (tabControlType == null)
				selector = new TabControl();
			else
				selector = (Selector)Activator.CreateInstance(tabControlType);

			return selector;
		}
		#endregion

		#region SetDefaultValues
		private void SetDefaultValues()
		{
			Array controlTypes = Enum.GetValues(typeof(EditorControlType));
			foreach (EditorControlType editorControlType in controlTypes)
			{
				Control control = GetEditor(editorControlType);

				if (control is TextBox)
				{
					TextBox textBox = control as TextBox;
					if (textBox != null)
					{
						switch (editorControlType)
						{
							case EditorControlType.Username:
								if (AuthMgr.Api.ConnectionInfo != null)
									textBox.Text = AuthMgr.Api.ConnectionInfo.UserName;
								else
									textBox.Text = AuthMgr.LoginDetails.Username;
								break;
							case EditorControlType.RallyServer:
								if (!String.IsNullOrWhiteSpace(AuthMgr.LoginDetails.RallyServer))
								{
									textBox.Text = AuthMgr.LoginDetails.RallyServer;
								}
								else if (ApiAuthManager.LoginWindowDefaultServer != null)
									textBox.Text = ApiAuthManager.LoginWindowDefaultServer.ToString();
								else
									textBox.Text = RallyRestApi.DEFAULT_SERVER;
								break;
							case EditorControlType.IdpServer:
								textBox.Text = AuthMgr.LoginDetails.IdpServer;
								break;
							case EditorControlType.ProxyServer:
								if (AuthMgr.LoginDetails.ProxyServer != null)
									textBox.Text = AuthMgr.LoginDetails.ProxyServer;
								else if (ApiAuthManager.LoginWindowDefaultProxyServer != null)
									textBox.Text = ApiAuthManager.LoginWindowDefaultProxyServer.ToString();
								break;
							case EditorControlType.ProxyUsername:
								textBox.Text = AuthMgr.LoginDetails.ProxyUsername;
								break;
							default:
								throw new InvalidProgramException("The specified editor does not use a text box to set the value.");
						}
					}
				}
				else if (control is PasswordBox)
				{
					PasswordBox passwordBox = control as PasswordBox;
					if (passwordBox != null)
					{
						switch (editorControlType)
						{
							case EditorControlType.Password:
								// Password for user credentials is never sent back to the UI.
								break;
							case EditorControlType.ProxyPassword:
								passwordBox.Password = AuthMgr.LoginDetails.GetProxyPassword();
								break;
							default:
								throw new InvalidProgramException("The specified editor does not use a password box to set the value.");
						}
					}
				}
				else if (control is ComboBox)
				{
					ComboBox comboBox = control as ComboBox;
					if (comboBox != null)
					{
						switch (editorControlType)
						{
							case EditorControlType.ConnectionType:
								// Due to errors in sequencing, we are removing the event listener while we set the value.
								comboBox.SelectionChanged -= ConnectionTypeChanged;
								comboBox.SelectedValue = AuthMgr.LoginDetails.ConnectionType;
								comboBox.SelectionChanged += ConnectionTypeChanged;

								// We then trigger the event manually here.
								ConnectionTypeChanged(comboBox, null);
								break;
							default:
								throw new InvalidProgramException("The specified editor does not use a password box to set the value.");
						}
					}
				}
				else if (control is CheckBox)
				{
					CheckBox checkBox = control as CheckBox;
					if (checkBox != null)
					{
						switch (editorControlType)
						{
							case EditorControlType.TrustAllCertificates:
								checkBox.IsChecked = AuthMgr.LoginDetails.TrustAllCertificates;
								break;
							default:
								throw new InvalidProgramException("The specified editor does not use a checkbox to set the value.");
						}
					}
				}
				else
					throw new InvalidProgramException("Unknown handling of control type.");
			}
		}
		#endregion

		#region SetReadOnlyStateForEditor
		private void SetReadOnlyStateForEditors(bool isReadOnly)
		{
			Array controlTypes = Enum.GetValues(typeof(EditorControlType));
			foreach (EditorControlType editorControlType in controlTypes)
			{
				Control control = GetEditor(editorControlType);
				if (isReadOnly)
				{
					control.Visibility = Visibility.Hidden;
				}
				else
					control.Visibility = Visibility.Visible;

				if (controlReadOnlyLabels.ContainsKey(control))
				{
					Label label = controlReadOnlyLabels[control];
					if (control is TextBox)
					{
						TextBox textBox = control as TextBox;
						if (textBox != null)
							label.Content = textBox.Text;
					}
					else if (control is ComboBox)
					{
						ComboBox comboBox = control as ComboBox;
						if (comboBox != null)
							label.Content = comboBox.Text;
					}
					else if (control is CheckBox)
					{
						if (editorControlType == EditorControlType.TrustAllCertificates)
							label.Content = GetEditorValueAsBool(EditorControlType.TrustAllCertificates).ToString();
					}

					if (isReadOnly)
						label.Visibility = Visibility.Visible;
					else
						label.Visibility = Visibility.Hidden;
				}
			}
		}
		#endregion

		#region AddTab
		private void AddTab(Selector tabControl, TabType tabType)
		{
			HeaderedContentControl tab = GetTabItem();
			tabControl.Items.Add(tab);
			tabControls.Add(tabType, tab);

			Grid tabGrid = new Grid();
			tab.Content = tabGrid;
			tabGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
			tabGrid.VerticalAlignment = VerticalAlignment.Top;
			AddColumnDefinition(tabGrid, 120);
			AddColumnDefinition(tabGrid);

			if (tabType == TabType.Credentials)
			{
				tab.Header = ApiAuthManager.LoginWindowCredentialsTabText;
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowUserNameLabelText, EditorControlType.Username);
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowPwdLabelText, EditorControlType.Password, true);
			}
			else if (tabType == TabType.Rally)
			{
				rallyTab = tab;
				tab.Header = ApiAuthManager.LoginWindowRallyServerTabText;
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowConnectionTypeText, EditorControlType.ConnectionType);
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowServerLabelText, EditorControlType.RallyServer);
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowServerLabelText, EditorControlType.IdpServer);
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowTrustAllCertificatesText, EditorControlType.TrustAllCertificates);
			}
			else if (tabType == TabType.Proxy)
			{
				tab.Header = ApiAuthManager.LoginWindowProxyServerTabText;
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowProxyServerLabelText, EditorControlType.ProxyServer);
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowProxyUserNameLabelText, EditorControlType.ProxyUsername);
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowProxyPwdLabelText, EditorControlType.ProxyPassword, true);
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

		#region GetTabItem
		private HeaderedContentControl GetTabItem()
		{
			HeaderedContentControl tabItem;

			Type tabItemType = RestApiAuthMgrWpf.GetCustomControlType(CustomWpfControlType.TabItem);
			if (tabItemType == null)
				tabItem = new TabItem();
			else
				tabItem = (HeaderedContentControl)Activator.CreateInstance(tabItemType);

			return tabItem;
		}
		#endregion

		#region AddInputToTabGrid
		private void AddInputToTabGrid(Grid tabGrid, string labelText, EditorControlType controlType, bool skipReadOnlyLabel = false)
		{
			Control control = GetEditor(controlType);
			int rowIndex = tabGrid.RowDefinitions.Count;
			AddRowDefinition(tabGrid, controlType, ROW_HEIGHT);
			Label label = new Label();
			label.Content = labelText;
			label.FontWeight = FontWeights.Bold;
			AddControlToGrid(tabGrid, label, rowIndex, 0);

			if (control != null)
			{
				AddControlToGrid(tabGrid, control, rowIndex, 1);
				if (!skipReadOnlyLabel)
				{
					Label readOnlyLabel = new Label();
					controlReadOnlyLabels.Add(control, readOnlyLabel);
					AddControlToGrid(tabGrid, readOnlyLabel, rowIndex, 1);
				}
			}
		}
		#endregion

		#region GetEditor
		private Control GetEditor(EditorControlType controlType, string defaultValue = null)
		{
			Control control = null;
			if (controls.ContainsKey(controlType))
				control = controls[controlType];
			else
			{
				switch (controlType)
				{
					case EditorControlType.Username:
					case EditorControlType.RallyServer:
					case EditorControlType.IdpServer:
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
					case EditorControlType.ConnectionType:
						ComboBox comboBox = new ComboBox();
						comboBox.SelectedValuePath = "Key";
						comboBox.DisplayMemberPath = "Value";

						if ((!RestApiAuthMgrWpf.AllowIdpBasedSso) &&
							(connectionTypes.ContainsKey(ConnectionType.IdpBasedSso)))
						{
							connectionTypes.Remove(ConnectionType.IdpBasedSso);
						}

						comboBox.ItemsSource = connectionTypes;
						comboBox.SelectionChanged += ConnectionTypeChanged;
						control = comboBox;
						break;
					case EditorControlType.TrustAllCertificates:
						CheckBox checkBox = new CheckBox();
						control = checkBox;
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

		#region ConnectionTypeChanged
		void ConnectionTypeChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;
			if (comboBox != null)
			{
				AuthMgr.LoginDetails.ConnectionType = (ConnectionType)comboBox.SelectedValue;
				switch (AuthMgr.LoginDetails.ConnectionType)
				{
					case ConnectionType.BasicAuth:
						SetTabVisibility(TabType.Credentials, true);
						SetControlVisibility(EditorControlType.Password, true);
						SetControlVisibility(EditorControlType.RallyServer, true);
						SetControlVisibility(EditorControlType.IdpServer, false);
						break;
					case ConnectionType.SpBasedSso:
						SetTabVisibility(TabType.Credentials, true);
						SetControlVisibility(EditorControlType.Password, false);
						SetControlVisibility(EditorControlType.RallyServer, true);
						SetControlVisibility(EditorControlType.IdpServer, false);
						break;
					case ConnectionType.IdpBasedSso:
						SetTabVisibility(TabType.Credentials, false);
						SetControlVisibility(EditorControlType.RallyServer, false);
						SetControlVisibility(EditorControlType.IdpServer, true);
						tabControl.SelectedItem = rallyTab;
						break;
					default:
						throw new NotImplementedException();
				}

				rallyTab.Focus();
			}
		}
		#endregion

		#region SetTabVisibility
		private void SetTabVisibility(TabType tabType, bool isVisible)
		{
			if (isVisible)
			{
				tabControls[tabType].Visibility = Visibility.Visible;
				tabControls[tabType].Width = double.NaN;
			}
			else
			{
				tabControls[tabType].Visibility = Visibility.Hidden;
				tabControls[tabType].Width = 0.0;
			}
		}
		#endregion

		#region SetControlVisibility
		private void SetControlVisibility(EditorControlType controlType, bool isVisible)
		{
			if (isVisible)
			{
				controlRowElements[controlType].Height = new GridLength(ROW_HEIGHT);
			}
			else
			{
				controlRowElements[controlType].Height = new GridLength(0.0);
			}
		}
		#endregion

		#region GetEditorValue
		private string GetEditorValue(EditorControlType controlType)
		{
			Control control = GetEditor(controlType);
			if (control == null)
				return null;

			TextBox textBox = control as TextBox;
			if (textBox != null && controlType == EditorControlType.IdpServer)
				return AuthMgr.LoginDetails.RedirectIfIdpPointsAtLoginSso(textBox.Text);

			if (textBox != null)
				return textBox.Text;

			PasswordBox passwordBox = control as PasswordBox;
			if (passwordBox != null)
				return passwordBox.Password;
			return null;
		}
		#endregion

		#region GetEditorValueAsBool
		private bool GetEditorValueAsBool(EditorControlType controlType)
		{
			Control control = GetEditor(controlType);
			if (control == null)
				return false;

			CheckBox checkBox = control as CheckBox;
			if (checkBox != null)
			{
				if (checkBox.IsChecked.HasValue)
					return checkBox.IsChecked.Value;
			}

			return false;
		}
		#endregion

		#region AddButtons
		private void AddButtons()
		{
			Grid buttonGrid = new Grid();
			Grid.SetColumn(buttonGrid, 2);
			Grid.SetRow(buttonGrid, 3);
			layoutGrid.Children.Add(buttonGrid);
			AddRowDefinition(buttonGrid, null, 30);

			buttonGrid.Height = 100;
			AddColumnDefinition(buttonGrid, 70);
			AddColumnDefinition(buttonGrid, 70);
			AddColumnDefinition(buttonGrid);

			loginButton = GetButton();
			loginButton.IsDefault = true;
			loginButton.Content = ApiAuthManager.LoginWindowLoginText;
			loginButton.Click += loginButton_Click;
			AddControlToGrid(buttonGrid, loginButton, 1, 0);

			logoutButton = GetButton();
			logoutButton.Content = ApiAuthManager.LoginWindowLogoutText;
			logoutButton.Click += logoutButton_Click;
			AddControlToGrid(buttonGrid, logoutButton, 1, 0);

			cancelButton = GetButton();
			cancelButton.Content = ApiAuthManager.LoginWindowCancelText;
			cancelButton.Click += cancelButton_Click;
			AddControlToGrid(buttonGrid, cancelButton, 1, 1);
		}
		#endregion

		#region GetButton
		private Button GetButton()
		{
			Button button;

			Type buttonType = RestApiAuthMgrWpf.GetCustomControlType(CustomWpfControlType.Buttons);
			if (buttonType == null)
				button = new Button();
			else
				button = (Button)Activator.CreateInstance(buttonType);

			button.Margin = new Thickness(5, 0, 5, 0);
			button.Height = 25;

			return button;
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
		private void AddRowDefinition(Grid grid, EditorControlType? controlType, int pixels = Int32.MaxValue)
		{
			RowDefinition rowDef = new RowDefinition();
			if (pixels == Int32.MaxValue)
				rowDef.Height = GridLength.Auto;
			else
				rowDef.Height = new GridLength(pixels, GridUnitType.Pixel);
			grid.RowDefinitions.Add(rowDef);

			if (controlType.HasValue)
				controlRowElements.Add(controlType.Value, rowDef);

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
			string errorMessage;
            ShowMessage("Logging into CA Agile Central");

			AuthMgr.LoginDetails.Username = GetEditorValue(EditorControlType.Username);
			AuthMgr.LoginDetails.SetPassword(GetEditorValue(EditorControlType.Password));
			AuthMgr.LoginDetails.RallyServer = GetEditorValue(EditorControlType.RallyServer);
			AuthMgr.LoginDetails.IdpServer = GetEditorValue(EditorControlType.IdpServer);
			AuthMgr.LoginDetails.ProxyServer = GetEditorValue(EditorControlType.ProxyServer);
			AuthMgr.LoginDetails.ProxyUsername = GetEditorValue(EditorControlType.ProxyUsername);
			AuthMgr.LoginDetails.SetProxyPassword(GetEditorValue(EditorControlType.ProxyPassword));
			AuthMgr.LoginDetails.TrustAllCertificates = GetEditorValueAsBool(EditorControlType.TrustAllCertificates);

			AuthMgr.PerformAuthenticationCheck(out errorMessage);
			ShowMessage(errorMessage);

			UpdateLoginState();
			if (AuthMgr.Api.AuthenticationState == RallyRestApi.AuthenticationResult.Authenticated)
				Close();
		}
		#endregion

		#region logoutButton_Click
		void logoutButton_Click(object sender, RoutedEventArgs e)
		{
			AuthMgr.PerformLogout();
			SetDefaultValues();
			UpdateLoginState();
		}
		#endregion

		#region cancelButton_Click
		void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion

		#region SsoAuthenticationComplete
		internal void SsoAuthenticationComplete(RallyRestApi.AuthenticationResult authenticationResult, RallyRestApi api)
		{
			if (authenticationResult == RallyRestApi.AuthenticationResult.Authenticated)
				Close();
			else
			{
				UpdateLoginState();
			}
		}
		#endregion

		#region ShowMessage
		private void ShowMessage(string message = "")
		{
			userMessages.Content = message;
			// Sleep to allow UI to update.
			Thread.CurrentThread.Join(10);
		}
		#endregion

		#region OnClosing
		protected override void OnClosing(CancelEventArgs e)
		{
			AuthMgr.LoginWindowSsoAuthenticationComplete = null;
			base.OnClosing(e);
		}
		#endregion
	}
}
