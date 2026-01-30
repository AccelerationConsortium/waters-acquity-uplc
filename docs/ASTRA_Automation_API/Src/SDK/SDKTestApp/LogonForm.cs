using System;
using System.Windows.Forms;
using AstraLib;
using SDKCommon;

namespace AstraClient
{
   public partial class LogonForm : Form
   {
      public LogonForm ()
      {
         InitializeComponent ();
      }
      public string GetUserName()
      {
         return ebUserName.Text;
      }
      public string GetPassword ()
      {
         return ebPassword.Text;
      }
      public string GetDomain ()
      {
         return cbDomain.Text;
      }
      private void btnLogon_Click (object sender, EventArgs e)
      {
         var userName = ebUserName.Text;
         var password = ebPassword.Text;
         var domain   = cbDomain.Text;

         try
         {
            this.btnLogon.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            Application.DoEvents ();
            // Execute your time-intensive hashing code here...

            // Set cursor as default arrow
            var logonResult = AstraAdmin.Get.ValidateLogon (userName, password, domain);

            if (0 == logonResult.isValid)
            {
               this.btnLogon.Enabled = false;
               Cursor.Current = Cursors.Default;
               Application.DoEvents ();
               MessageBox.Show (new Form() { TopMost = true }, logonResult.errorMessage, "ASTRA Logon Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
               return;
            }

            this.btnLogon.Enabled = false;
            Cursor.Current = Cursors.Default;
            Application.DoEvents ();
         }
         catch (System.Exception ex)
         {
            this.btnLogon.Enabled = false;
            Cursor.Current = Cursors.Default;
            Application.DoEvents ();

            var securityPackActive = AstraAdmin.Get.IsSecurityPackActive ();
            if (securityPackActive)
            {
               var isLoggedIn = AstraAdmin.Get.IsLoggedIn ();
               if (!isLoggedIn)
               {
                  MessageBox.Show (new Form() { TopMost = true }, ex.Message, "ASTRA Logon Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  return;
               }
            }
         }

         this.DialogResult = DialogResult.OK;
         this.Close ();
      }
      private void LogonForm_Shown (object sender, EventArgs e)
      {
         this.Activate ();
      }
   }
}
