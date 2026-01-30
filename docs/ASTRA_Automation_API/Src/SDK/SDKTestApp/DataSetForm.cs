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
   public partial class DataSetForm : Form
   {
      int SelectedExpId = 0;

      public DataSetForm(int experimentId)
      {
         InitializeComponent();

         SelectedExpId = experimentId;
      }

      public void DataSet_Get(object sender, EventArgs e)
      {
         var dataSetDefinitionName = textBox2.Text;
         var data = AstraAdmin.Get.GetDataSet(SelectedExpId, dataSetDefinitionName);
         textBox1.Text = data;
      }

      public void DataSet_Save(object sender, EventArgs e)
      {
         if ((null == saveFileDialog) || (saveFileDialog.ShowDialog() != DialogResult.OK))
            return;

         string fileName = saveFileDialog.FileName;
         var dataSetDefinitionName = textBox2.Text;

         AstraAdmin.Get.SaveDataSet(SelectedExpId, dataSetDefinitionName, fileName);
      }
   }
}
