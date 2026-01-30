using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SDKCommon;

namespace AstraClient
{
   public partial class ResultsForm : Form
   {
      int SelectedExpId = 0;

      public ResultsForm(int experimentId)
      {
         InitializeComponent();

         SelectedExpId = experimentId;

         var result = AstraAdmin.Get.GetResults(SelectedExpId);

         this.textBox1.Text = result;
      }

      private void exportButton_Click(object sender, EventArgs e)
      {
         if ((null == saveFileDialog) || (saveFileDialog.ShowDialog() != DialogResult.OK))
            return;

         string fileName = saveFileDialog.FileName;

         AstraAdmin.Get.SaveResults(SelectedExpId, fileName);
      }
   }
}
