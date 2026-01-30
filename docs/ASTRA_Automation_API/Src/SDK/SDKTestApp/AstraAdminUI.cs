// ************************************************************************
// (c) Copyright 2021 by Wyatt Technology Corporation. All rights reserved.
// ************************************************************************
using SDKCommon;
using System.Windows.Forms;

namespace AstraClient
{
    // Thread safe AstraAdmin singleton
    public static class AstraAdminUi
    {
        // Singleton stuff
        private static readonly object SyncRoot = new object ();

        public static bool Logon ()
        {
            // var result = astraSP_.ValidateLogon("swtester_g", "still@SWT!er", "wyatt.com");
            var securityPackActive = AstraAdmin.Get.IsSecurityPackActive ();
            var isLoggedIn = AstraAdmin.Get.IsLoggedIn ();

            if (securityPackActive && !isLoggedIn)
            {
                var dlgLogon = new LogonForm ();

                if (DialogResult.OK != dlgLogon.ShowDialog ())
                {
                    dlgLogon.Close ();
                    Application.Exit ();
                    return false;
                }
            }

            return true;
        }
    }

}