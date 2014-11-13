using Rally.RestApi.Auth;
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

namespace Rally.RestApi.UiForWinforms
{
	public partial class LoginWindow : Form
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

		#region Static Values
		private static object LogoImage;
		private static string HeaderLabelText;
		private static string CredentialsTabText;
		private static string RallyServerTabText;
		private static string ProxyServerTabText;

		private static string UserNameLabelText;
		private static string PwdLabelText;

		private static string ServerLabelText;
		private static string ProxyServerLabelText;
		private static string ProxyUserNameLabelText;
		private static string ProxyPwdLabelText;

		private static string SsoInProgressText;
		private static string LoginText;
		private static string LogoutText;
		private static string CancelText;

		private static Uri DefaultServer;
		private static Uri DefaultProxyServer;
		#endregion

		Dictionary<EditorControlType, Control> controls;
		Dictionary<Control, Label> controlReadOnlyLabels;

		internal RestApiAuthMgrWinforms AuthMgr { get; set; }
		internal event AuthenticationComplete AuthenticationComplete;
		Label ssoInProgressLabel;
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

			//Logo.Source = LogoImage;
			//headerLabel.Content = HeaderLabelText;
			controls = new Dictionary<EditorControlType, Control>();
			controlReadOnlyLabels = new Dictionary<Control, Label>();
		}
		#endregion

		#region Configure
		/// <summary>
		/// <para>Configure this control with the items that it needs to work.</para>
		/// <para>Nullable parameters have defaults that will be used if not provided.</para>
		/// </summary>
		internal static void Configure(object logo, string headerLabelText,
			Uri defaultServer, Uri defaultProxyServer,
			string credentialsTabText, string userNameLabelText, string pwdLabelText,
			string serverTabText, string serverLabelText,
			string proxyServerTabText, string proxyServerLabelText,
			string proxyUserNameLabelText, string proxyPwdLabelText, string ssoInProgressText,
			string loginText, string logoutText, string cancelText)
		{
			LogoImage = logo;
			HeaderLabelText = headerLabelText;
			CredentialsTabText = credentialsTabText;
			UserNameLabelText = userNameLabelText;
			PwdLabelText = pwdLabelText;

			DefaultServer = defaultServer;
			DefaultProxyServer = defaultProxyServer;

			RallyServerTabText = serverTabText;
			ServerLabelText = serverLabelText;

			ProxyServerTabText = proxyServerTabText;
			ProxyServerLabelText = proxyServerLabelText;
			ProxyUserNameLabelText = proxyUserNameLabelText;
			ProxyPwdLabelText = proxyPwdLabelText;

			SsoInProgressText = ssoInProgressText;
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

			#region Default Strings: Rally
			if (String.IsNullOrWhiteSpace(RallyServerTabText))
				RallyServerTabText = "Rally";

			if (String.IsNullOrWhiteSpace(ServerLabelText))
				ServerLabelText = "Server";
			#endregion

			#region Default Strings: Proxy
			if (String.IsNullOrWhiteSpace(ProxyServerTabText))
				ProxyServerTabText = "Proxy";

			if (String.IsNullOrWhiteSpace(ProxyServerLabelText))
				ProxyServerLabelText = "Server";

			if (String.IsNullOrWhiteSpace(ProxyUserNameLabelText))
				ProxyUserNameLabelText = "User Name";

			if (String.IsNullOrWhiteSpace(ProxyPwdLabelText))
				ProxyPwdLabelText = "Password";
			#endregion

			#region Default Strings: Buttons
			if (String.IsNullOrWhiteSpace(SsoInProgressText))
				SsoInProgressText = "SSO in Progress";

			if (String.IsNullOrWhiteSpace(LoginText))
				LoginText = "Login";

			if (String.IsNullOrWhiteSpace(LogoutText))
				LogoutText = "Logout";

			if (String.IsNullOrWhiteSpace(CancelText))
				CancelText = "Cancel";
			#endregion
		}
		#endregion

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
					loginButton.Visible = false;
					logoutButton.Visible = true;
					ssoInProgressLabel.Visible = false;
					break;
				case RallyRestApi.AuthenticationResult.PendingSSO:
					loginButton.Visible = false;
					logoutButton.Visible = false;
					ssoInProgressLabel.Visible = true;
					break;
				case RallyRestApi.AuthenticationResult.NotAuthorized:
					loginButton.Visible = true;
					logoutButton.Visible = false;
					ssoInProgressLabel.Visible = false;
					isReadOnly = false;
					break;
				default:
					throw new InvalidProgramException("Unknown authentication state.");
			}

			SetReadOnlyStateForEditors(isReadOnly);
		}
		#endregion

		#region Removed - BuildLayout
		internal void BuildLayout(RestApiAuthMgrWinforms authMgr)
		{
			AuthMgr = authMgr;
			TabControl tabControl = new TabControl();
			tabControl.Margin = new Padding(10);
			//TableLayoutPanel.SetColumn(tabControl, 0);
			//TableLayoutPanel.SetColumnSpan(tabControl, 2);
			//TableLayoutPanel.SetRow(tabControl, 1);
			//layoutTableLayoutPanel.Children.Add(tabControl);

			//AddTab(tabControl, TabType.Credentials);
			//AddTab(tabControl, TabType.Rally);
			//AddTab(tabControl, TabType.Proxy);

			//inputRow.Height = new TableLayoutPanelLength(tabControl.Height + 35, TableLayoutPanelUnitType.Pixel);
			//inputRow.MinHeight = inputRow.Height.Value;

			//this.Height = inputRow.Height.Value + (28 * 2) + 50 + 50;
			//this.MinHeight = this.Height;
			//this.MaxHeight = this.Height;

			AddButtons();
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
					control.Visible = false;
				else
					control.Visible = true;

				if (controlReadOnlyLabels.ContainsKey(control))
				{
					Label label = controlReadOnlyLabels[control];
					TextBox textBox = control as TextBox;
					if (textBox != null)
						label.Text = textBox.Text;

					if (isReadOnly)
						label.Visible = true;
					else
						label.Visible = false;
				}
			}
		}
		#endregion

		#region Removed - AddTab
		private void AddTab(TabControl tabControl, TabType tabType)
		{
			//TabPage tab = new TabPage();
			//tabControl.TabPages.Add(tab);

			//TableLayoutPanel tabTableLayoutPanel = new TableLayoutPanel();
			//tab.Content = tabTableLayoutPanel;
			//tabTableLayoutPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
			//tabTableLayoutPanel.VerticalAlignment = VerticalAlignment.Top;
			//AddColumnDefinition(tabTableLayoutPanel, 120);
			//AddColumnDefinition(tabTableLayoutPanel);

			//if (tabType == TabType.Credentials)
			//{
			//	tab.Header = CredentialsTabText;
			//	AddInputToTabTableLayoutPanel(tabTableLayoutPanel, UserNameLabelText, GetEditor(EditorControlType.Username));
			//	AddInputToTabTableLayoutPanel(tabTableLayoutPanel, PwdLabelText, GetEditor(EditorControlType.Password), true);
			//}
			//else if (tabType == TabType.Rally)
			//{
			//	tab.Header = RallyServerTabText;
			//	AddInputToTabTableLayoutPanel(tabTableLayoutPanel, ServerLabelText, GetEditor(EditorControlType.RallyServer));
			//}
			//else if (tabType == TabType.Proxy)
			//{
			//	tab.Header = ProxyServerTabText;
			//	AddInputToTabTableLayoutPanel(tabTableLayoutPanel, ProxyServerLabelText, GetEditor(EditorControlType.ProxyServer));
			//	AddInputToTabTableLayoutPanel(tabTableLayoutPanel, ProxyUserNameLabelText, GetEditor(EditorControlType.ProxyUsername));
			//	AddInputToTabTableLayoutPanel(tabTableLayoutPanel, ProxyPwdLabelText, GetEditor(EditorControlType.ProxyPassword), true);
			//}
			//else
			//	throw new NotImplementedException();

			//if ((tabControl.Height.ToString().Equals("NaN", StringComparison.InvariantCultureIgnoreCase)) ||
			//	(tabControl.Height < tabTableLayoutPanel.Height + 20))
			//{
			//	tabControl.Height = tabTableLayoutPanel.Height + 20;
			//}
		}
		#endregion

		#region Removed - AddInputToTabTableLayoutPanel
		private void AddInputToTabTableLayoutPanel(TableLayoutPanel tabTableLayoutPanel, string labelText, Control control, bool skipReadOnlyLabel = false)
		{
			//int rowIndex = tabTableLayoutPanel.RowDefinitions.Count;
			//AddRowDefinition(tabTableLayoutPanel, 28);
			//Label label = new Label();
			//label.Text = labelText;
			//label.Font.Bold = true;
			//AddControlToTableLayoutPanel(tabTableLayoutPanel, label, rowIndex, 0);

			//if (control != null)
			//{
			//	AddControlToTableLayoutPanel(tabTableLayoutPanel, control, rowIndex, 1);
			//	if (!skipReadOnlyLabel)
			//	{
			//		Label readOnlyLabel = new Label();
			//		controlReadOnlyLabels.Add(control, readOnlyLabel);
			//		AddControlToTableLayoutPanel(tabTableLayoutPanel, readOnlyLabel, rowIndex, 1);
			//	}
			//}
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
					case EditorControlType.Password:
					case EditorControlType.ProxyPassword:
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
								else
									textBox.Text = DefaultServer.ToString();
								break;
							case EditorControlType.ProxyServer:
								if ((AuthMgr.Api.ConnectionInfo != null) &&
									(AuthMgr.Api.ConnectionInfo.Proxy != null))
								{
									textBox.Text = AuthMgr.Api.ConnectionInfo.Proxy.Address.ToString();
								}
								else if (DefaultProxyServer != null)
									textBox.Text = DefaultProxyServer.ToString();
								break;
							case EditorControlType.Password:
							case EditorControlType.ProxyPassword:
								textBox.UseSystemPasswordChar = true;
								break;
							default:
								break;
						}
						control = textBox;
						break;
					default:
						throw new NotImplementedException();
				}

				control.Margin = new Padding(0, 0, 10, 0);
				// TODO: Fix width
				//control.HorizontalAlignment = HorizontalAlignment.Stretch;
				//control.MinWidth = 150;
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

			return null;
		}
		#endregion

		#region Removed - AddButtons
		private void AddButtons()
		{
			//TableLayoutPanel buttonTableLayoutPanel = new TableLayoutPanel();
			//TableLayoutPanel.SetColumn(buttonTableLayoutPanel, 1);
			//TableLayoutPanel.SetRow(buttonTableLayoutPanel, 4);
			//layoutTableLayoutPanel.Children.Add(buttonTableLayoutPanel);

			//AddColumnDefinition(buttonTableLayoutPanel, 70);
			//AddColumnDefinition(buttonTableLayoutPanel, 70);
			//AddColumnDefinition(buttonTableLayoutPanel);

			//Padding margin = new Padding(5, 0, 5, 0);

			//ssoInProgressLabel = new Label();
			//ssoInProgressLabel.Content = SsoInProgressText;
			//AddControlToTableLayoutPanel(buttonTableLayoutPanel, ssoInProgressLabel, 0, 0);

			//loginButton = new Button();
			//loginButton.Margin = margin;
			//loginButton.IsDefault = true;
			//loginButton.Text = LoginText;
			//loginButton.Click += loginButton_Click;
			//AddControlToTableLayoutPanel(buttonTableLayoutPanel, loginButton, 0, 0);

			//logoutButton = new Button();
			//logoutButton.Margin = margin;
			//logoutButton.Text = LogoutText;
			//logoutButton.Click += logoutButton_Click;
			//AddControlToTableLayoutPanel(buttonTableLayoutPanel, logoutButton, 0, 0);

			//cancelButton = new Button();
			//cancelButton.Margin = margin;
			//cancelButton.Text = CancelText;
			//cancelButton.Click += cancelButton_Click;
			//AddControlToTableLayoutPanel(buttonTableLayoutPanel, cancelButton, 0, 1);
		}
		#endregion

		#region Removed - AddControlToTableLayoutPanel
		private void AddControlToTableLayoutPanel(TableLayoutPanel grid, Control control, int row, int column, int rowSpan = 1, int colSpan = 1)
		{
			//if (row >= 0)
			//	TableLayoutPanel.SetRow(control, row);
			//if (rowSpan > 1)
			//	TableLayoutPanel.SetRowSpan(control, rowSpan);

			//if (column >= 0)
			//	TableLayoutPanel.SetColumn(control, column);
			//if (colSpan > 1)
			//	TableLayoutPanel.SetColumnSpan(control, colSpan);

			//grid.Children.Add(control);
		}
		#endregion

		#region Removed - AddRowDefinition
		private void AddRowDefinition(TableLayoutPanel grid, int pixels = Int32.MaxValue)
		{
			//RowDefinition rowDef = new RowDefinition();
			//if (pixels == Int32.MaxValue)
			//	rowDef.Height = TableLayoutPanelLength.Auto;
			//else
			//	rowDef.Height = new TableLayoutPanelLength(pixels, TableLayoutPanelUnitType.Pixel);
			//grid.RowDefinitions.Add(rowDef);

			//if (pixels != Int32.MaxValue)
			//{
			//	grid.MinHeight += pixels + 2;
			//	grid.Height = grid.MinHeight;
			//}
			//else
			//	grid.Height = double.NaN;
		}
		#endregion

		#region Removed - AddColumnDefinition
		private void AddColumnDefinition(TableLayoutPanel grid, int pixels = Int32.MaxValue)
		{
			//ColumnDefinition colDef = new ColumnDefinition();
			//if (pixels != Int32.MaxValue)
			//	colDef.Width = new TableLayoutPanelLength(pixels, TableLayoutPanelUnitType.Pixel);

			//grid.ColumnDefinitions.Add(colDef);
		}
		#endregion

		#region loginButton_Click

		void loginButton_Click(object sender, EventArgs e)
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
				GetEditorValue(EditorControlType.RallyServer), proxy);

			if (AuthenticationComplete != null)
			{
				switch (AuthMgr.Api.AuthenticationState)
				{
					case RallyRestApi.AuthenticationResult.Authenticated:
						AuthenticationComplete.Invoke(AuthMgr.Api.AuthenticationState, AuthMgr.Api);
						Close();
						break;
					case RallyRestApi.AuthenticationResult.PendingSSO:
					case RallyRestApi.AuthenticationResult.NotAuthorized:
						AuthenticationComplete.Invoke(AuthMgr.Api.AuthenticationState, null);
						break;
					default:
						throw new NotImplementedException();
				}
			}
		}
		#endregion

		#region logoutButton_Click
		void logoutButton_Click(object sender, EventArgs e)
		{
			AuthMgr.Api.Logout();
			AuthenticationComplete.Invoke(AuthMgr.Api.AuthenticationState, null);
		}
		#endregion

		#region cancelButton_Click
		void cancelButton_Click(object sender, EventArgs e)
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

		#region OnClosing
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			AuthMgr.LoginWindowSsoAuthenticationComplete = null;
			base.OnClosing(e);
		}
		#endregion
	}
}
