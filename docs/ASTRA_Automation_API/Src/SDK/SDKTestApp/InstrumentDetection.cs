using System;
using System.Windows.Forms;

using SDKCommon;

namespace AstraClient
{
   public partial class InstrumentDetection : Form
   {
      public InstrumentDetection()
      {
         InitializeComponent();
      }

      protected override void OnLoad(EventArgs e)
      {
         base.OnLoad(e);
         CenterToParent();
         AstraAdmin.Get.InstrumentsDetected += OnInstrumentDetectionCompleted;
         // Make sure we didn't miss the message!
         if (AstraAdmin.Get.HasInstrumentDetectionCompleted)
            this.DialogResult = DialogResult.OK;
      }
      
      private void OnInstrumentDetectionCompleted()
      {
         this.DialogResult = DialogResult.OK;
      }
   }
}
