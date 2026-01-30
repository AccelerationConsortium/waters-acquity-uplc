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
   public partial class BaselinesForm : Form
   {
      int SelectedExpId = 0;

      public BaselinesForm(int experimentId)
      {
         InitializeComponent();

         SelectedExpId = experimentId;

         var baselines = AstraAdmin.Get.GetBaselines(SelectedExpId);

         foreach (AstraLib.BaselineDetails baseline in baselines)
         {
            this.dataGridView1.Rows.Add(AstraAdmin.Get.GetBaselineTypeString(baseline.type), baseline.seriesName, baseline.start.x, baseline.end.x, baseline.start.y, baseline.end.y);
         }
      }

      private void Baselines_Close(object sender, EventArgs e)
      {
         Close();
      }

      private void Baselines_Apply(object sender, EventArgs e)
      {
         int row_count = this.dataGridView1.Rows.Count;

         var baselines = new AstraLib.BaselineDetails[row_count];

         for (int i=0; i<row_count; i++)
         {
            baselines[i].type       = AstraAdmin.Get.GetBaselineTypeInt (Convert.ToString(this.dataGridView1.Rows[i].Cells["bType"].Value));
            baselines[i].seriesName = Convert.ToString(this.dataGridView1.Rows[i].Cells["bName"].Value);
            baselines[i].start.x    = Convert.ToDouble(this.dataGridView1.Rows[i].Cells["bX1"].Value);
            baselines[i].start.y    = Convert.ToDouble(this.dataGridView1.Rows[i].Cells["bY1"].Value);
            baselines[i].end.x      = Convert.ToDouble(this.dataGridView1.Rows[i].Cells["bX2"].Value);
            baselines[i].end.y      = Convert.ToDouble(this.dataGridView1.Rows[i].Cells["bY2"].Value);
         }

         AstraAdmin.Get.UpdateBaselines(SelectedExpId, baselines);
      }
   }
}
