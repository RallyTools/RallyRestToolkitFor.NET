using Rally.RestApi.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
			Rally,
			Proxy,
		}
		#endregion

		#region Enum: EditorControlType
		private enum EditorControlType
		{
			Username,
			Password,
			RallyServer,
			ProxyServer,
			ProxyUsername,
			ProxyPassword,
		}
		#endregion

		Dictionary<EditorControlType, Control> controls;
		Dictionary<Control, Label> controlReadOnlyLabels;

		internal RestApiAuthMgrWpf AuthMgr { get; set; }
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

			headerLabel.Content = ApiAuthManager.LoginWindowHeaderLabelText;
			controls = new Dictionary<EditorControlType, Control>();
			controlReadOnlyLabels = new Dictionary<Control, Label>();
		}
		#endregion

		internal void SetLogo(ImageSource logo)
		{
			Logo.Source = logo;
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
			Selector tabControl = GetTabControl();
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

			this.Height = inputRow.Height.Value + (28 * 2) + 100;
			this.MinHeight = this.Height;
			this.MaxHeight = this.Height;
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

		#region SetReadOnlyStateForEditor
		private void SetReadOnlyStateForEditors(bool isReadOnly)
		{
			Array controlTypes = Enum.GetValues(typeof(EditorControlType));
			foreach (EditorControlType editorControlType in controlTypes)
			{
				Control control = GetEditor(editorControlType);
				if (isReadOnly)
					control.Visibility = Visibility.Hidden;
				else
					control.Visibility = Visibility.Visible;

				if (controlReadOnlyLabels.ContainsKey(control))
				{
					Label label = controlReadOnlyLabels[control];
					TextBox textBox = control as TextBox;
					if (textBox != null)
						label.Content = textBox.Text;

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

			Grid tabGrid = new Grid();
			tab.Content = tabGrid;
			tabGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
			tabGrid.VerticalAlignment = VerticalAlignment.Top;
			AddColumnDefinition(tabGrid, 120);
			AddColumnDefinition(tabGrid);

			if (tabType == TabType.Credentials)
			{
				tab.Header = ApiAuthManager.LoginWindowCredentialsTabText;
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowUserNameLabelText, GetEditor(EditorControlType.Username));
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowPwdLabelText, GetEditor(EditorControlType.Password), true);
			}
			else if (tabType == TabType.Rally)
			{
				tab.Header = ApiAuthManager.LoginWindowRallyServerTabText;
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowServerLabelText, GetEditor(EditorControlType.RallyServer));
			}
			else if (tabType == TabType.Proxy)
			{
				tab.Header = ApiAuthManager.LoginWindowProxyServerTabText;
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowProxyServerLabelText, GetEditor(EditorControlType.ProxyServer));
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowProxyUserNameLabelText, GetEditor(EditorControlType.ProxyUsername));
				AddInputToTabGrid(tabGrid, ApiAuthManager.LoginWindowProxyPwdLabelText, GetEditor(EditorControlType.ProxyPassword), true);
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
		private void AddInputToTabGrid(Grid tabGrid, string labelText, Control control, bool skipReadOnlyLabel = false)
		{
			int rowIndex = tabGrid.RowDefinitions.Count;
			AddRowDefinition(tabGrid, 28);
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
					case EditorControlType.ProxyServer:
					case EditorControlType.ProxyUsername:
						TextBox textBox = new TextBox();
						switch (controlType)
						{
							case EditorControlType.Username:
								if (AuthMgr.Api.ConnectionInfo != null)
									textBox.Text = AuthMgr.Api.ConnectionInfo.UserName;
								break;
							case EditorControlType.RallyServer:
								if ((AuthMgr.Api.ConnectionInfo != null) &&
									(!String.IsNullOrWhiteSpace(AuthMgr.Api.ConnectionInfo.Server.ToString())))
								{
									textBox.Text = AuthMgr.Api.ConnectionInfo.Server.ToString();
								}
								else if (ApiAuthManager.LoginWindowDefaultServer != null)
									textBox.Text = ApiAuthManager.LoginWindowDefaultServer.ToString();
								else
									textBox.Text = RallyRestApi.DEFAULT_SERVER;
								break;
							case EditorControlType.ProxyServer:
								if ((AuthMgr.Api.ConnectionInfo != null) &&
									(AuthMgr.Api.ConnectionInfo.Proxy != null))
								{
									textBox.Text = AuthMgr.Api.ConnectionInfo.Proxy.Address.ToString();
								}
								else if (ApiAuthManager.LoginWindowDefaultProxyServer != null)
									textBox.Text = ApiAuthManager.LoginWindowDefaultProxyServer.ToString();
								break;
							default:
								break;
						}
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
			Grid.SetColumn(buttonGrid, 2);
			Grid.SetRow(buttonGrid, 3);
			layoutGrid.Children.Add(buttonGrid);
			AddRowDefinition(buttonGrid, 30);

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
			string errorMessage;
			AuthMgr.PerformAuthenticationCheck(GetEditorValue(EditorControlType.Username),
				GetEditorValue(EditorControlType.Password),
				GetEditorValue(EditorControlType.RallyServer),
				GetEditorValue(EditorControlType.ProxyServer),
				GetEditorValue(EditorControlType.ProxyUsername),
				GetEditorValue(EditorControlType.ProxyUsername),
				out errorMessage);
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
		}
		#endregion

		#region OnClosing
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			AuthMgr.LoginWindowSsoAuthenticationComplete = null;
			base.OnClosing(e);
		}
		#endregion
	}
}
