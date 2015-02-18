namespace Rally.RestApi.UiForWinforms
{
	partial class LoginWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panel1 = new System.Windows.Forms.Panel();
			this.headerTitleLabel = new System.Windows.Forms.Label();
			this.logoIcon = new System.Windows.Forms.PictureBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabCredentials = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.passwordInput = new System.Windows.Forms.TextBox();
			this.passwordLabel = new System.Windows.Forms.Label();
			this.userNameLabel = new System.Windows.Forms.Label();
			this.usernameInput = new System.Windows.Forms.TextBox();
			this.tabServer = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.rallyServerLabel = new System.Windows.Forms.Label();
			this.rallyServerInput = new System.Windows.Forms.TextBox();
			this.tabProxy = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.proxyServerInput = new System.Windows.Forms.TextBox();
			this.proxyServerLabel = new System.Windows.Forms.Label();
			this.proxyPasswordInput = new System.Windows.Forms.TextBox();
			this.proxyPasswordLabel = new System.Windows.Forms.Label();
			this.proxyUserNameLabel = new System.Windows.Forms.Label();
			this.proxyUserNameInput = new System.Windows.Forms.TextBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.logoutBtn = new System.Windows.Forms.Button();
			this.loginBtn = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.userMessageLabel = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.logoIcon)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabCredentials.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tabServer.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tabProxy.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.headerTitleLabel);
			this.panel1.Controls.Add(this.logoIcon);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(381, 68);
			this.panel1.TabIndex = 2;
			// 
			// headerTitleLabel
			// 
			this.headerTitleLabel.AutoSize = true;
			this.headerTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.headerTitleLabel.Location = new System.Drawing.Point(58, 21);
			this.headerTitleLabel.Name = "headerTitleLabel";
			this.headerTitleLabel.Size = new System.Drawing.Size(178, 31);
			this.headerTitleLabel.TabIndex = 0;
			this.headerTitleLabel.Text = "Login to Rally";
			// 
			// logoIcon
			// 
			this.logoIcon.Location = new System.Drawing.Point(12, 12);
			this.logoIcon.Name = "logoIcon";
			this.logoIcon.Size = new System.Drawing.Size(40, 40);
			this.logoIcon.TabIndex = 1;
			this.logoIcon.TabStop = false;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabCredentials);
			this.tabControl1.Controls.Add(this.tabServer);
			this.tabControl1.Controls.Add(this.tabProxy);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tabControl1.Location = new System.Drawing.Point(10, 10);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(361, 104);
			this.tabControl1.TabIndex = 1;
			this.tabControl1.TabStop = false;
			// 
			// tabCredentials
			// 
			this.tabCredentials.Controls.Add(this.tableLayoutPanel1);
			this.tabCredentials.Location = new System.Drawing.Point(4, 22);
			this.tabCredentials.Name = "tabCredentials";
			this.tabCredentials.Size = new System.Drawing.Size(353, 78);
			this.tabCredentials.TabIndex = 0;
			this.tabCredentials.Text = "Credentials";
			this.tabCredentials.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.passwordInput, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.passwordLabel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.userNameLabel, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.usernameInput, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(353, 80);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// passwordInput
			// 
			this.passwordInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.passwordInput.Location = new System.Drawing.Point(103, 28);
			this.passwordInput.Name = "passwordInput";
			this.passwordInput.PasswordChar = '*';
			this.passwordInput.Size = new System.Drawing.Size(251, 20);
			this.passwordInput.TabIndex = 2;
			// 
			// passwordLabel
			// 
			this.passwordLabel.AutoSize = true;
			this.passwordLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.passwordLabel.Location = new System.Drawing.Point(3, 25);
			this.passwordLabel.Name = "passwordLabel";
			this.passwordLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.passwordLabel.Size = new System.Drawing.Size(94, 25);
			this.passwordLabel.TabIndex = 3;
			this.passwordLabel.Text = "Password";
			this.passwordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// userNameLabel
			// 
			this.userNameLabel.AutoSize = true;
			this.userNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.userNameLabel.Location = new System.Drawing.Point(3, 0);
			this.userNameLabel.Name = "userNameLabel";
			this.userNameLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.userNameLabel.Size = new System.Drawing.Size(94, 25);
			this.userNameLabel.TabIndex = 4;
			this.userNameLabel.Text = "User Name";
			this.userNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// usernameInput
			// 
			this.usernameInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usernameInput.Location = new System.Drawing.Point(103, 3);
			this.usernameInput.Name = "usernameInput";
			this.usernameInput.Size = new System.Drawing.Size(251, 20);
			this.usernameInput.TabIndex = 1;
			// 
			// tabServer
			// 
			this.tabServer.Controls.Add(this.tableLayoutPanel2);
			this.tabServer.Location = new System.Drawing.Point(4, 22);
			this.tabServer.Name = "tabServer";
			this.tabServer.Size = new System.Drawing.Size(354, 78);
			this.tabServer.TabIndex = 1;
			this.tabServer.Text = "Server";
			this.tabServer.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.rallyServerLabel, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.rallyServerInput, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(354, 80);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// rallyServerLabel
			// 
			this.rallyServerLabel.AutoSize = true;
			this.rallyServerLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rallyServerLabel.Location = new System.Drawing.Point(3, 0);
			this.rallyServerLabel.Name = "rallyServerLabel";
			this.rallyServerLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.rallyServerLabel.Size = new System.Drawing.Size(94, 25);
			this.rallyServerLabel.TabIndex = 0;
			this.rallyServerLabel.Text = "Server";
			this.rallyServerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// rallyServerInput
			// 
			this.rallyServerInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rallyServerInput.Location = new System.Drawing.Point(103, 3);
			this.rallyServerInput.Name = "rallyServerInput";
			this.rallyServerInput.Size = new System.Drawing.Size(250, 20);
			this.rallyServerInput.TabIndex = 3;
			// 
			// tabProxy
			// 
			this.tabProxy.Controls.Add(this.tableLayoutPanel3);
			this.tabProxy.Location = new System.Drawing.Point(4, 22);
			this.tabProxy.Name = "tabProxy";
			this.tabProxy.Size = new System.Drawing.Size(354, 78);
			this.tabProxy.TabIndex = 2;
			this.tabProxy.Text = "Proxy";
			this.tabProxy.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.Controls.Add(this.proxyServerInput, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.proxyServerLabel, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.proxyPasswordInput, 1, 2);
			this.tableLayoutPanel3.Controls.Add(this.proxyPasswordLabel, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.proxyUserNameLabel, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.proxyUserNameInput, 1, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 3;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(354, 80);
			this.tableLayoutPanel3.TabIndex = 0;
			// 
			// proxyServerInput
			// 
			this.proxyServerInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.proxyServerInput.Location = new System.Drawing.Point(103, 3);
			this.proxyServerInput.Name = "proxyServerInput";
			this.proxyServerInput.Size = new System.Drawing.Size(250, 20);
			this.proxyServerInput.TabIndex = 4;
			// 
			// proxyServerLabel
			// 
			this.proxyServerLabel.AutoSize = true;
			this.proxyServerLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.proxyServerLabel.Location = new System.Drawing.Point(3, 0);
			this.proxyServerLabel.Name = "proxyServerLabel";
			this.proxyServerLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.proxyServerLabel.Size = new System.Drawing.Size(94, 25);
			this.proxyServerLabel.TabIndex = 5;
			this.proxyServerLabel.Text = "Server";
			this.proxyServerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// proxyPasswordInput
			// 
			this.proxyPasswordInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.proxyPasswordInput.Location = new System.Drawing.Point(103, 53);
			this.proxyPasswordInput.Name = "proxyPasswordInput";
			this.proxyPasswordInput.PasswordChar = '*';
			this.proxyPasswordInput.Size = new System.Drawing.Size(250, 20);
			this.proxyPasswordInput.TabIndex = 6;
			// 
			// proxyPasswordLabel
			// 
			this.proxyPasswordLabel.AutoSize = true;
			this.proxyPasswordLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.proxyPasswordLabel.Location = new System.Drawing.Point(3, 50);
			this.proxyPasswordLabel.Name = "proxyPasswordLabel";
			this.proxyPasswordLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.proxyPasswordLabel.Size = new System.Drawing.Size(94, 30);
			this.proxyPasswordLabel.TabIndex = 7;
			this.proxyPasswordLabel.Text = "Password";
			this.proxyPasswordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// proxyUserNameLabel
			// 
			this.proxyUserNameLabel.AutoSize = true;
			this.proxyUserNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.proxyUserNameLabel.Location = new System.Drawing.Point(3, 25);
			this.proxyUserNameLabel.Name = "proxyUserNameLabel";
			this.proxyUserNameLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.proxyUserNameLabel.Size = new System.Drawing.Size(94, 25);
			this.proxyUserNameLabel.TabIndex = 8;
			this.proxyUserNameLabel.Text = "User Name";
			this.proxyUserNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// proxyUserNameInput
			// 
			this.proxyUserNameInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.proxyUserNameInput.Location = new System.Drawing.Point(103, 28);
			this.proxyUserNameInput.Name = "proxyUserNameInput";
			this.proxyUserNameInput.Size = new System.Drawing.Size(250, 20);
			this.proxyUserNameInput.TabIndex = 5;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.cancelBtn);
			this.flowLayoutPanel1.Controls.Add(this.logoutBtn);
			this.flowLayoutPanel1.Controls.Add(this.loginBtn);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 209);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(381, 30);
			this.flowLayoutPanel1.TabIndex = 1;
			// 
			// cancelBtn
			// 
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = new System.Drawing.Point(303, 3);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(75, 23);
			this.cancelBtn.TabIndex = 0;
			this.cancelBtn.TabStop = false;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// logoutBtn
			// 
			this.logoutBtn.Location = new System.Drawing.Point(222, 3);
			this.logoutBtn.Name = "logoutBtn";
			this.logoutBtn.Size = new System.Drawing.Size(75, 23);
			this.logoutBtn.TabIndex = 1;
			this.logoutBtn.TabStop = false;
			this.logoutBtn.Text = "Logout";
			this.logoutBtn.UseVisualStyleBackColor = true;
			this.logoutBtn.Click += new System.EventHandler(this.logoutBtn_Click);
			// 
			// loginBtn
			// 
			this.loginBtn.Location = new System.Drawing.Point(141, 3);
			this.loginBtn.Name = "loginBtn";
			this.loginBtn.Size = new System.Drawing.Size(75, 23);
			this.loginBtn.TabIndex = 2;
			this.loginBtn.TabStop = false;
			this.loginBtn.Text = "Login";
			this.loginBtn.UseVisualStyleBackColor = true;
			this.loginBtn.Click += new System.EventHandler(this.loginBtn_Click);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.userMessageLabel);
			this.panel2.Controls.Add(this.tabControl1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 68);
			this.panel2.Name = "panel2";
			this.panel2.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);
			this.panel2.Size = new System.Drawing.Size(381, 141);
			this.panel2.TabIndex = 0;
			// 
			// userMessageLabel
			// 
			this.userMessageLabel.AutoSize = true;
			this.userMessageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.userMessageLabel.ForeColor = System.Drawing.Color.Red;
			this.userMessageLabel.Location = new System.Drawing.Point(13, 117);
			this.userMessageLabel.Name = "userMessageLabel";
			this.userMessageLabel.Size = new System.Drawing.Size(106, 17);
			this.userMessageLabel.TabIndex = 0;
			this.userMessageLabel.Text = "User Messages";
			// 
			// LoginWindow
			// 
			this.AcceptButton = this.loginBtn;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(381, 239);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "LoginWindow";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Login to Rally";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.logoIcon)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabCredentials.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tabServer.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.tabProxy.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label headerTitleLabel;
		private System.Windows.Forms.PictureBox logoIcon;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabCredentials;
		private System.Windows.Forms.TabPage tabServer;
		private System.Windows.Forms.TabPage tabProxy;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label userNameLabel;
		private System.Windows.Forms.TextBox usernameInput;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label rallyServerLabel;
		private System.Windows.Forms.TextBox rallyServerInput;
		private System.Windows.Forms.TextBox passwordInput;
		private System.Windows.Forms.Label passwordLabel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.TextBox proxyPasswordInput;
		private System.Windows.Forms.Label proxyPasswordLabel;
		private System.Windows.Forms.Label proxyUserNameLabel;
		private System.Windows.Forms.TextBox proxyUserNameInput;
		private System.Windows.Forms.TextBox proxyServerInput;
		private System.Windows.Forms.Label proxyServerLabel;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.Button logoutBtn;
		private System.Windows.Forms.Button loginBtn;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label userMessageLabel;
	}
}