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
   public partial class PeakRangesForm : Form
   {
      int SelectedExpId = 0;

      public PeakRangesForm(int experimentId)
      {
         InitializeComponent();

         SelectedExpId = experimentId;

         var peaks = AstraAdmin.Get.GetPeakRanges(SelectedExpId);

         foreach (AstraLib.PeakRange peak in peaks)
         {
            this.dataGridView1.Rows.Add(peak.number, peak.start, peak.end);
         }
      }

      private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
      {
         var senderGrid = (DataGridView)sender;

         if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
             e.RowIndex >= 0)
         {
            AstraAdmin.Get.RemovePeakRange(SelectedExpId, Convert.ToInt32(this.dataGridView1.Rows[e.RowIndex].Cells["Number"].Value));
         }

         this.dataGridView1.Rows.Clear();

         var peaks = AstraAdmin.Get.GetPeakRanges(SelectedExpId);

         foreach (AstraLib.PeakRange peak in peaks)
         {
            this.dataGridView1.Rows.Add(peak.number, peak.start, peak.end);
         }
      }

      private void PeakRanges_Close(object sender, EventArgs e)
      {
         Close();
      }

      private void PeakRanges_Add(object sender, EventArgs e)
      {
         AddPeakForm form = new AddPeakForm(SelectedExpId);
         form.FormClosed += new FormClosedEventHandler(AddPeakForm_Closed);
         form.Show();
      }

      private void PeakRanges_Apply(object sender, EventArgs e)
      {
         int row_count = this.dataGridView1.Rows.Count;

         var peaks = new AstraLib.PeakRange[row_count];

         for (int i = 0; i < row_count; i++)
         {
            peaks[i].number = Convert.ToInt32(this.dataGridView1.Rows[i].Cells["Number"].Value);
            peaks[i].start = Convert.ToDouble(this.dataGridView1.Rows[i].Cells["Start"].Value);
            peaks[i].end = Convert.ToDouble(this.dataGridView1.Rows[i].Cells["End"].Value);
         }

         for (int i = 0; i < row_count; i++)
         {
            AstraAdmin.Get.UpdatePeakRange(SelectedExpId, peaks[i]);
         }
      }

      private void AddPeakForm_Closed(object sender, FormClosedEventArgs e)
      {
         this.dataGridView1.Rows.Clear();
         
         var peaks = AstraAdmin.Get.GetPeakRanges(SelectedExpId);

         foreach (AstraLib.PeakRange peak in peaks)
         {
            this.dataGridView1.Rows.Add(peak.number, peak.start, peak.end);
         }
      }
   }
}
