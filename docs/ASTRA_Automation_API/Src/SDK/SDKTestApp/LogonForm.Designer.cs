
namespace AstraClient
{
   partial class LogonForm
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose (bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose ();
         }
         base.Dispose (disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent ()
      {
            this.label1 = new System.Windows.Forms.Label();
            this.ebPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ebUserName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbDomain = new System.Windows.Forms.ComboBox();
            this.btnLogon = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "User Name";
            // 
            // ebPassword
            // 
            this.ebPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ebPassword.Location = new System.Drawing.Point(103, 55);
            this.ebPassword.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ebPassword.Name = "ebPassword";
            this.ebPassword.PasswordChar = '*';
            this.ebPassword.Size = new System.Drawing.Size(253, 22);
            this.ebPassword.TabIndex = 3;
            this.ebPassword.UseSystemPasswordChar = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 59);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Password";
            // 
            // ebUserName
            // 
            this.ebUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ebUserName.Location = new System.Drawing.Point(103, 18);
            this.ebUserName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ebUserName.Name = "ebUserName";
            this.ebUserName.Size = new System.Drawing.Size(253, 22);
            this.ebUserName.TabIndex = 1;
            this.ebUserName.WordWrap = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 98);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Domain";
            // 
            // cbDomain
            // 
            this.cbDomain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDomain.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbDomain.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbDomain.FormattingEnabled = true;
            this.cbDomain.Location = new System.Drawing.Point(103, 94);
            this.cbDomain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbDomain.Name = "cbDomain";
            this.cbDomain.Size = new System.Drawing.Size(253, 24);
            this.cbDomain.TabIndex = 5;
            // 
            // btnLogon
            // 
            this.btnLogon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogon.Location = new System.Drawing.Point(384, 17);
            this.btnLogon.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnLogon.Name = "btnLogon";
            this.btnLogon.Size = new System.Drawing.Size(100, 28);
            this.btnLogon.TabIndex = 6;
            this.btnLogon.Text = "Log on";
            this.btnLogon.UseVisualStyleBackColor = true;
            this.btnLogon.Click += new System.EventHandler(this.btnLogon_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(384, 55);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // LogonForm
            // 
            this.AcceptButton = this.btnLogon;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(501, 135);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ebUserName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ebPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbDomain);
            this.Controls.Add(this.btnLogon);
            this.Controls.Add(this.btnCancel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LogonForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LogonForm";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.LogonForm_Load);
            this.Shown += new System.EventHandler(this.LogonForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

      }
      private void LogonForm_Load (object sender, System.EventArgs e)
      {
         var cbDefaultIndex = this.cbDomain.Items.Add (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties ().DomainName); ;
         this.cbDomain.SelectedIndex = cbDefaultIndex;
      }
      #endregion

      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox ebUserName;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox ebPassword;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.ComboBox cbDomain;
      private System.Windows.Forms.Button btnLogon;
      private System.Windows.Forms.Button btnCancel;
   }
}
