namespace Test.Rally.RestApi.UiForWinForms
{
	partial class MainWindow
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label7 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.zSessionIDLabel = new System.Windows.Forms.Label();
			this.actionButton = new System.Windows.Forms.Button();
			this.serverInput = new System.Windows.Forms.TextBox();
			this.usernameInput = new System.Windows.Forms.TextBox();
			this.wsapiInput = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.59887F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.40113F));
			this.tableLayoutPanel1.Controls.Add(this.wsapiInput, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.usernameInput, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.label7, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.actionButton, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.serverInput, 1, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(354, 123);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label7.Location = new System.Drawing.Point(3, 72);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(73, 24);
			this.label7.TabIndex = 7;
			this.label7.Text = "WSAPI Version";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(3, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(73, 24);
			this.label5.TabIndex = 4;
			this.label5.Text = "Username";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(73, 24);
			this.label3.TabIndex = 2;
			this.label3.Text = "Server";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(82, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(269, 24);
			this.label2.TabIndex = 1;
			this.label2.Text = "Login";
			// 
			// zSessionIDLabel
			// 
			this.zSessionIDLabel.AutoSize = true;
			this.zSessionIDLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zSessionIDLabel.Location = new System.Drawing.Point(0, 151);
			this.zSessionIDLabel.Name = "zSessionIDLabel";
			this.zSessionIDLabel.Size = new System.Drawing.Size(62, 13);
			this.zSessionIDLabel.TabIndex = 8;
			this.zSessionIDLabel.Text = "ZSessionID";
			// 
			// actionButton
			// 
			this.actionButton.Location = new System.Drawing.Point(82, 99);
			this.actionButton.Name = "actionButton";
			this.actionButton.Size = new System.Drawing.Size(75, 21);
			this.actionButton.TabIndex = 8;
			this.actionButton.Text = "Login";
			this.actionButton.UseVisualStyleBackColor = true;
			this.actionButton.Click += new System.EventHandler(this.actionButton_Click);
			// 
			// serverInput
			// 
			this.serverInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.serverInput.Location = new System.Drawing.Point(82, 27);
			this.serverInput.Name = "serverInput";
			this.serverInput.Size = new System.Drawing.Size(269, 20);
			this.serverInput.TabIndex = 9;
			// 
			// usernameInput
			// 
			this.usernameInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usernameInput.Location = new System.Drawing.Point(82, 51);
			this.usernameInput.Name = "usernameInput";
			this.usernameInput.Size = new System.Drawing.Size(269, 20);
			this.usernameInput.TabIndex = 11;
			// 
			// wsapiInput
			// 
			this.wsapiInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wsapiInput.Location = new System.Drawing.Point(82, 75);
			this.wsapiInput.Name = "wsapiInput";
			this.wsapiInput.Size = new System.Drawing.Size(269, 20);
			this.wsapiInput.TabIndex = 12;
			// 
			// MainWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(569, 164);
			this.Controls.Add(this.zSessionIDLabel);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "MainWindow";
			this.Text = "Test Application for Rally Rest API - WinForms";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button actionButton;
		private System.Windows.Forms.Label zSessionIDLabel;
		private System.Windows.Forms.TextBox wsapiInput;
		private System.Windows.Forms.TextBox usernameInput;
		private System.Windows.Forms.TextBox serverInput;
	}
}

