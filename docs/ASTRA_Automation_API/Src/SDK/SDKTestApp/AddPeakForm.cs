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
   public partial class AddPeakForm : Form
   {
      int SelectedExpId = 0;
      public AddPeakForm(int experimentId)
      {
         InitializeComponent();

         SelectedExpId = experimentId;
      }

      private void AddPeak_Close(object sender, EventArgs e)
      {
         Close();
      }

      private void AddPeak_Add(object sender, EventArgs e)
      {
         double start, end;
         Double.TryParse(textBox1.Text, out start);
         Double.TryParse(textBox2.Text, out end);
         AstraAdmin.Get.AddPeakRange(SelectedExpId, start, end);
         Close();
      }
   }
}
