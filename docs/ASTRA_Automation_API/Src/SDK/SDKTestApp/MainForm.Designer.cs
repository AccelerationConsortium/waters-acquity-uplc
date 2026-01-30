namespace AstraClient
{
   partial class MainForm
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
            this.components = new System.ComponentModel.Container ();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (MainForm));
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.menuStrip = new System.Windows.Forms.MenuStrip ();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.openExperimentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator ();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator ();
            this.showASTRAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator ();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.experimentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.validateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator ();
            this.startCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.stopCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.getBaselinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.getPeakRangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.getResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.getDataSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.autofindToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.statusImages = new System.Windows.Forms.ImageList (this.components);
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog ();
            this.saveResultsDialog = new System.Windows.Forms.SaveFileDialog ();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog ();
            this.animationTimer = new System.Windows.Forms.Timer (this.components);
            this.resetButton = new System.Windows.Forms.Button ();
            this.applyButton = new System.Windows.Forms.Button ();
            this.label3 = new System.Windows.Forms.Label ();
            this.label1 = new System.Windows.Forms.Label ();
            this.injectedVolume = new System.Windows.Forms.TextBox ();
            this.flowRate = new System.Windows.Forms.TextBox ();
            this.label9 = new System.Windows.Forms.Label ();
            this.label10 = new System.Windows.Forms.Label ();
            this.groupBox1 = new System.Windows.Forms.GroupBox ();
            this.label25 = new System.Windows.Forms.Label ();
            this.label24 = new System.Windows.Forms.Label ();
            this.sampleA2 = new System.Windows.Forms.TextBox ();
            this.sampleDndc = new System.Windows.Forms.TextBox ();
            this.label23 = new System.Windows.Forms.Label ();
            this.label22 = new System.Windows.Forms.Label ();
            this.sampleUvExtinction = new System.Windows.Forms.TextBox ();
            this.sampleConcentration = new System.Windows.Forms.TextBox ();
            this.label21 = new System.Windows.Forms.Label ();
            this.label20 = new System.Windows.Forms.Label ();
            this.label19 = new System.Windows.Forms.Label ();
            this.label18 = new System.Windows.Forms.Label ();
            this.label17 = new System.Windows.Forms.Label ();
            this.label16 = new System.Windows.Forms.Label ();
            this.sampleName = new System.Windows.Forms.TextBox ();
            this.label7 = new System.Windows.Forms.Label ();
            this.sampleDescription = new System.Windows.Forms.TextBox ();
            this.groupBox3 = new System.Windows.Forms.GroupBox ();
            this.label6 = new System.Windows.Forms.Label ();
            this.duration = new System.Windows.Forms.TextBox ();
            this.label4 = new System.Windows.Forms.Label ();
            this.label5 = new System.Windows.Forms.Label ();
            this.experimentDescription = new System.Windows.Forms.TextBox ();
            this.groupBox2 = new System.Windows.Forms.GroupBox ();
            this.useInstrumentCC = new System.Windows.Forms.CheckBox ();
            this.experimentColumn = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader ()));
            this.experimentList = new System.Windows.Forms.ListView ();
            this.menuStrip.SuspendLayout ();
            this.groupBox1.SuspendLayout ();
            this.groupBox3.SuspendLayout ();
            this.groupBox2.SuspendLayout ();
            this.SuspendLayout ();
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.helpToolStripMenuItem.DropDownItems.AddRange (new System.Windows.Forms.ToolStripItem [] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::AstraClient.Properties.Resources.help_about;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(133, 26);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.experimentToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.menuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip.Size = new System.Drawing.Size(767, 28);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openExperimentToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripSeparator2,
            this.closeToolStripMenuItem,
            this.toolStripSeparator3,
            this.showASTRAToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "&File";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.fileToolStripMenuItem_DropDownOpening);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(281, 26);
            this.newToolStripMenuItem.Text = "&New Experiment...";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openExperimentToolStripMenuItem
            // 
            this.openExperimentToolStripMenuItem.Name = "openExperimentToolStripMenuItem";
            this.openExperimentToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openExperimentToolStripMenuItem.Size = new System.Drawing.Size(281, 26);
            this.openExperimentToolStripMenuItem.Text = "Open Experiment...";
            this.openExperimentToolStripMenuItem.Click += new System.EventHandler(this.openExperimentToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = global::AstraClient.Properties.Resources.experiment_save;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(281, 26);
            this.saveToolStripMenuItem.Text = "&Save Experiment As...";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Image = global::AstraClient.Properties.Resources.experiment_save;
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(281, 26);
            this.exportToolStripMenuItem.Text = "&Export Experiment...";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(278, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(281, 26);
            this.closeToolStripMenuItem.Text = "&Close Experiment";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(278, 6);
            // 
            // showASTRAToolStripMenuItem
            // 
            this.showASTRAToolStripMenuItem.Image = global::AstraClient.Properties.Resources.application;
            this.showASTRAToolStripMenuItem.Name = "showASTRAToolStripMenuItem";
            this.showASTRAToolStripMenuItem.Size = new System.Drawing.Size(281, 26);
            this.showASTRAToolStripMenuItem.Text = "Show &ASTRA";
            this.showASTRAToolStripMenuItem.Click += new System.EventHandler(this.showASTRAToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(278, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(281, 26);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // experimentToolStripMenuItem
            // 
            this.experimentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.validateToolStripMenuItem,
            this.toolStripMenuItem1,
            this.startCollectionToolStripMenuItem,
            this.stopCollectionToolStripMenuItem,
            this.getBaselinesToolStripMenuItem,
            this.getPeakRangesToolStripMenuItem,
            this.getResultsToolStripMenuItem,
            this.getDataSetToolStripMenuItem,
            this.autofindToolStripMenuItem});
            this.experimentToolStripMenuItem.Name = "experimentToolStripMenuItem";
            this.experimentToolStripMenuItem.Size = new System.Drawing.Size (98, 24);
            this.experimentToolStripMenuItem.Text = "&Experiment";
            this.experimentToolStripMenuItem.DropDownOpening += new System.EventHandler (this.experimentToolStripMenuItem_DropDownOpening);
            // 
            // validateToolStripMenuItem
            // 
            this.validateToolStripMenuItem.Image = global::AstraClient.Properties.Resources.validate;
            this.validateToolStripMenuItem.Name = "validateToolStripMenuItem";
            this.validateToolStripMenuItem.Size = new System.Drawing.Size (201, 26);
            this.validateToolStripMenuItem.Text = "&Validate";
            this.validateToolStripMenuItem.Click += new System.EventHandler (this.validateToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size (198, 6);
            // 
            // startCollectionToolStripMenuItem
            // 
            this.startCollectionToolStripMenuItem.Image = global::AstraClient.Properties.Resources.play_button;
            this.startCollectionToolStripMenuItem.Name = "startCollectionToolStripMenuItem";
            this.startCollectionToolStripMenuItem.Size = new System.Drawing.Size (201, 26);
            this.startCollectionToolStripMenuItem.Text = "&Start Collection";
            this.startCollectionToolStripMenuItem.Click += new System.EventHandler (this.startCollectionToolStripMenuItem_Click);
            // 
            // stopCollectionToolStripMenuItem
            // 
            this.stopCollectionToolStripMenuItem.Enabled = false;
            this.stopCollectionToolStripMenuItem.Image = global::AstraClient.Properties.Resources.stop_button;
            this.stopCollectionToolStripMenuItem.Name = "stopCollectionToolStripMenuItem";
            this.stopCollectionToolStripMenuItem.Size = new System.Drawing.Size (201, 26);
            this.stopCollectionToolStripMenuItem.Text = "S&top Collection";
            this.stopCollectionToolStripMenuItem.Click += new System.EventHandler (this.stopCollectionToolStripMenuItem_Click);
            // 
            // getBaselinesToolStripMenuItem
            // 
            this.getBaselinesToolStripMenuItem.Name = "getBaselinesToolStripMenuItem";
            this.getBaselinesToolStripMenuItem.Size = new System.Drawing.Size (201, 26);
            this.getBaselinesToolStripMenuItem.Text = "Get Baselines";
            this.getBaselinesToolStripMenuItem.Click += new System.EventHandler (this.getBaselinesToolStripMenuItem_Click);
            // 
            // getPeakRangesToolStripMenuItem
            // 
            this.getPeakRangesToolStripMenuItem.Name = "getPeakRangesToolStripMenuItem";
            this.getPeakRangesToolStripMenuItem.Size = new System.Drawing.Size (201, 26);
            this.getPeakRangesToolStripMenuItem.Text = "Get Peak Ranges";
            this.getPeakRangesToolStripMenuItem.Click += new System.EventHandler (this.getPeakRangesToolStripMenuItem_Click);
            // 
            // getResultsToolStripMenuItem
            // 
            this.getResultsToolStripMenuItem.Name = "getResultsToolStripMenuItem";
            this.getResultsToolStripMenuItem.Size = new System.Drawing.Size (201, 26);
            this.getResultsToolStripMenuItem.Text = "Get Results";
            this.getResultsToolStripMenuItem.Click += new System.EventHandler (this.getResultsToolStripMenuItem_Click);
            // 
            // getDataSetToolStripMenuItem
            // 
            this.getDataSetToolStripMenuItem.Name = "getDataSetToolStripMenuItem";
            this.getDataSetToolStripMenuItem.Size = new System.Drawing.Size (201, 26);
            this.getDataSetToolStripMenuItem.Text = "Get DataSet";
            this.getDataSetToolStripMenuItem.Click += new System.EventHandler (this.getDataSetToolStripMenuItem_Click);
            // 
            // autorunToolStripMenuItem
            // 
            this.autofindToolStripMenuItem.Name = "autorunToolStripMenuItem";
            this.autofindToolStripMenuItem.Size = new System.Drawing.Size (201, 26);
            this.autofindToolStripMenuItem.Text = "AutoFind";
            this.autofindToolStripMenuItem.Click += new System.EventHandler (this.autofindToolStripMenuItem_Click);
            // 
            // statusImages
            // 
            this.statusImages.ImageStream = ((System.Windows.Forms.ImageListStreamer) (resources.GetObject ("statusImages.ImageStream")));
            this.statusImages.TransparentColor = System.Drawing.Color.Transparent;
            this.statusImages.Images.SetKeyName (0, "experiment.png");
            this.statusImages.Images.SetKeyName (1, "waiting.png");
            this.statusImages.Images.SetKeyName (2, "busy1.bmp");
            this.statusImages.Images.SetKeyName (3, "busy2.bmp");
            this.statusImages.Images.SetKeyName (4, "busy3.bmp");
            this.statusImages.Images.SetKeyName (5, "busy4.bmp");
            this.statusImages.Images.SetKeyName (6, "busy5.bmp");
            this.statusImages.Images.SetKeyName (7, "executed.png");
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "vaf";
            this.saveFileDialog.Filter = "ASTRA (*.afe8)|*.afe8";
            // 
            // saveResultsDialog
            // 
            this.saveResultsDialog.DefaultExt = "txt";
            this.saveResultsDialog.Filter = "Text (*.txt)|*.txt";
            // 
            // animationTimer
            // 
            this.animationTimer.Enabled = true;
            this.animationTimer.Tick += new System.EventHandler (this.animationTimer_Tick);
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.resetButton.Location = new System.Drawing.Point (649, 546);
            this.resetButton.Margin = new System.Windows.Forms.Padding (4, 4, 4, 4);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size (100, 28);
            this.resetButton.TabIndex = 11;
            this.resetButton.Text = "&Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler (this.resetButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point (541, 546);
            this.applyButton.Margin = new System.Windows.Forms.Padding (4, 4, 4, 4);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size (100, 28);
            this.applyButton.TabIndex = 10;
            this.applyButton.Text = "&Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler (this.applyButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point (61, 52);
            this.label3.Margin = new System.Windows.Forms.Padding (0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size (110, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "Injected volume:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point (68, 21);
            this.label1.Margin = new System.Windows.Forms.Padding (0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size (105, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Pump flow rate:";
            // 
            // injectedVolume
            // 
            this.injectedVolume.Location = new System.Drawing.Point (185, 54);
            this.injectedVolume.Margin = new System.Windows.Forms.Padding (4, 4, 4, 4);
            this.injectedVolume.Name = "injectedVolume";
            this.injectedVolume.Size = new System.Drawing.Size (92, 22);
            this.injectedVolume.TabIndex = 3;
            this.injectedVolume.TextChanged += new System.EventHandler (this.textBox_TextChanged);
            this.injectedVolume.Validating += new System.ComponentModel.CancelEventHandler (this.doubleField_Validating);
            // 
            // flowRate
            // 
            this.flowRate.Location = new System.Drawing.Point (185, 20);
            this.flowRate.Margin = new System.Windows.Forms.Padding (4, 4, 4, 4);
            this.flowRate.Name = "flowRate";
            this.flowRate.Size = new System.Drawing.Size (92, 22);
            this.flowRate.TabIndex = 2;
            this.flowRate.TextChanged += new System.EventHandler (this.textBox_TextChanged);
            this.flowRate.Validating += new System.ComponentModel.CancelEventHandler (this.doubleField_Validating);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point (284, 21);
            this.label9.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size (53, 17);
            this.label9.TabIndex = 2;
            this.label9.Text = "mL/min";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point (284, 55);
            this.label10.Margin = new System.Windows.Forms.Padding (4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size (27, 17);
            this.label10.TabIndex = 5;
            this.label10.Text = "mL";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.flowRate);
            this.groupBox1.Controls.Add(this.injectedVolume);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(255, 224);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(7, 4, 7, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(495, 89);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Pump/Injector";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(148, 118);
            this.label25.Margin = new System.Windows.Forms.Padding(0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(29, 17);
            this.label25.TabIndex = 5;
            this.label25.Text = "A2:";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(127, 86);
            this.label24.Margin = new System.Windows.Forms.Padding(0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(47, 17);
            this.label24.TabIndex = 2;
            this.label24.Text = "dn/dc:";
            // 
            // sampleA2
            // 
            this.sampleA2.Location = new System.Drawing.Point(185, 117);
            this.sampleA2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sampleA2.Name = "sampleA2";
            this.sampleA2.Size = new System.Drawing.Size(172, 22);
            this.sampleA2.TabIndex = 7;
            this.sampleA2.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.sampleA2.Validating += new System.ComponentModel.CancelEventHandler(this.doubleField_Validating);
            // 
            // sampleDndc
            // 
            this.sampleDndc.Location = new System.Drawing.Point(185, 85);
            this.sampleDndc.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sampleDndc.Name = "sampleDndc";
            this.sampleDndc.Size = new System.Drawing.Size(172, 22);
            this.sampleDndc.TabIndex = 6;
            this.sampleDndc.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.sampleDndc.Validating += new System.ComponentModel.CancelEventHandler(this.doubleField_Validating);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(12, 150);
            this.label23.Margin = new System.Windows.Forms.Padding(0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(162, 17);
            this.label23.TabIndex = 9;
            this.label23.Text = "UV extinction coefficient:";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(77, 182);
            this.label22.Margin = new System.Windows.Forms.Padding(0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(100, 17);
            this.label22.TabIndex = 12;
            this.label22.Text = "Concentration:";
            // 
            // sampleUvExtinction
            // 
            this.sampleUvExtinction.Location = new System.Drawing.Point(185, 149);
            this.sampleUvExtinction.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sampleUvExtinction.Name = "sampleUvExtinction";
            this.sampleUvExtinction.Size = new System.Drawing.Size(172, 22);
            this.sampleUvExtinction.TabIndex = 8;
            this.sampleUvExtinction.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.sampleUvExtinction.Validating += new System.ComponentModel.CancelEventHandler(this.doubleField_Validating);
            // 
            // sampleConcentration
            // 
            this.sampleConcentration.Location = new System.Drawing.Point(185, 181);
            this.sampleConcentration.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sampleConcentration.Name = "sampleConcentration";
            this.sampleConcentration.Size = new System.Drawing.Size(172, 22);
            this.sampleConcentration.TabIndex = 9;
            this.sampleConcentration.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.sampleConcentration.Validating += new System.ComponentModel.CancelEventHandler(this.doubleField_Validating);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(364, 86);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(39, 17);
            this.label21.TabIndex = 4;
            this.label21.Text = "mL/g";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(361, 118);
            this.label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(65, 17);
            this.label20.TabIndex = 7;
            this.label20.Text = "mol mL/g";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(361, 150);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(71, 17);
            this.label19.TabIndex = 11;
            this.label19.Text = "mL/(g cm)";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(361, 182);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(39, 17);
            this.label18.TabIndex = 14;
            this.label18.Text = "g/mL";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(423, 114);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(13, 13);
            this.label17.TabIndex = 8;
            this.label17.Text = "2";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(128, 22);
            this.label16.Margin = new System.Windows.Forms.Padding(0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(49, 17);
            this.label16.TabIndex = 0;
            this.label16.Text = "Name:";
            // 
            // sampleName
            // 
            this.sampleName.Location = new System.Drawing.Point(185, 21);
            this.sampleName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sampleName.Name = "sampleName";
            this.sampleName.Size = new System.Drawing.Size(297, 22);
            this.sampleName.TabIndex = 4;
            this.sampleName.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(95, 54);
            this.label7.Margin = new System.Windows.Forms.Padding(0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(83, 17);
            this.label7.TabIndex = 15;
            this.label7.Text = "Description:";
            // 
            // sampleDescription
            // 
            this.sampleDescription.Location = new System.Drawing.Point(185, 53);
            this.sampleDescription.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sampleDescription.Name = "sampleDescription";
            this.sampleDescription.Size = new System.Drawing.Size(297, 22);
            this.sampleDescription.TabIndex = 5;
            this.sampleDescription.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.sampleDescription);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.sampleName);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.label17);
            this.groupBox3.Controls.Add(this.label18);
            this.groupBox3.Controls.Add(this.label19);
            this.groupBox3.Controls.Add(this.label20);
            this.groupBox3.Controls.Add(this.label21);
            this.groupBox3.Controls.Add(this.sampleConcentration);
            this.groupBox3.Controls.Add(this.sampleUvExtinction);
            this.groupBox3.Controls.Add(this.label22);
            this.groupBox3.Controls.Add(this.label23);
            this.groupBox3.Controls.Add(this.sampleDndc);
            this.groupBox3.Controls.Add(this.sampleA2);
            this.groupBox3.Controls.Add(this.label24);
            this.groupBox3.Controls.Add(this.label25);
            this.groupBox3.Location = new System.Drawing.Point(255, 320);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(7, 4, 7, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox3.Size = new System.Drawing.Size(495, 217);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Injected Sample";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(49, 103);
            this.label6.Margin = new System.Windows.Forms.Padding(0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(129, 17);
            this.label6.TabIndex = 2;
            this.label6.Text = "Collection duration:";
            // 
            // duration
            // 
            this.duration.Location = new System.Drawing.Point(189, 102);
            this.duration.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.duration.Name = "duration";
            this.duration.Size = new System.Drawing.Size(92, 22);
            this.duration.TabIndex = 1;
            this.duration.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.duration.Validating += new System.ComponentModel.CancelEventHandler(this.doubleField_Validating);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(288, 103);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 17);
            this.label4.TabIndex = 4;
            this.label4.Text = "min";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(45, 20);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 17);
            this.label5.TabIndex = 0;
            this.label5.Text = "Description:";
            // 
            // experimentDescription
            // 
            this.experimentDescription.Location = new System.Drawing.Point(52, 42);
            this.experimentDescription.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.experimentDescription.Multiline = true;
            this.experimentDescription.Name = "experimentDescription";
            this.experimentDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.experimentDescription.Size = new System.Drawing.Size(431, 51);
            this.experimentDescription.TabIndex = 0;
            this.experimentDescription.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.useInstrumentCC);
            this.groupBox2.Controls.Add(this.experimentDescription);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.duration);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(255, 44);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(7, 4, 7, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(495, 172);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Experiment";
            // 
            // useInstrumentCC
            // 
            this.useInstrumentCC.AutoSize = true;
            this.useInstrumentCC.Checked = true;
            this.useInstrumentCC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useInstrumentCC.Location = new System.Drawing.Point(53, 138);
            this.useInstrumentCC.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.useInstrumentCC.Name = "useInstrumentCC";
            this.useInstrumentCC.Size = new System.Drawing.Size(307, 21);
            this.useInstrumentCC.TabIndex = 5;
            this.useInstrumentCC.Text = "Use physical instrument calibration constant";
            this.useInstrumentCC.UseVisualStyleBackColor = true;
            // 
            // experimentList
            // 
            this.experimentList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.experimentList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.experimentColumn});
            this.experimentList.HideSelection = false;
            this.experimentList.LabelWrap = false;
            this.experimentList.Location = new System.Drawing.Point (12, 44);
            this.experimentList.Margin = new System.Windows.Forms.Padding (0);
            this.experimentList.MultiSelect = false;
            this.experimentList.Name = "experimentList";
            this.experimentList.Size = new System.Drawing.Size (227, 491);
            this.experimentList.SmallImageList = this.statusImages;
            this.experimentList.TabIndex = 12;
            this.experimentList.UseCompatibleStateImageBehavior = false;
            this.experimentList.View = System.Windows.Forms.View.List;
            this.experimentList.SelectedIndexChanged += new System.EventHandler (this.experimentList_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF (8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size (767, 590);
            this.Controls.Add (this.groupBox2);
            this.Controls.Add (this.groupBox3);
            this.Controls.Add (this.groupBox1);
            this.Controls.Add (this.experimentList);
            this.Controls.Add (this.applyButton);
            this.Controls.Add (this.resetButton);
            this.Controls.Add (this.menuStrip);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject ("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding (4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ASTRA Automation Client";
            this.Load += new System.EventHandler (this.MainForm_Load);
            this.menuStrip.ResumeLayout (false);
            this.menuStrip.PerformLayout ();
            this.groupBox1.ResumeLayout (false);
            this.groupBox1.PerformLayout ();
            this.groupBox3.ResumeLayout (false);
            this.groupBox3.PerformLayout ();
            this.groupBox2.ResumeLayout (false);
            this.groupBox2.PerformLayout ();
            this.ResumeLayout (false);
            this.PerformLayout ();

        }

      #endregion

      private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
      private System.Windows.Forms.MenuStrip menuStrip;
      private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
      private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
      private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
      private System.Windows.Forms.SaveFileDialog saveFileDialog;
      private System.Windows.Forms.SaveFileDialog saveResultsDialog;
      private System.Windows.Forms.OpenFileDialog openFileDialog;
      private System.Windows.Forms.ToolStripMenuItem experimentToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem startCollectionToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem stopCollectionToolStripMenuItem;
      private System.Windows.Forms.ImageList statusImages;
      private System.Windows.Forms.Timer animationTimer;
      private System.Windows.Forms.ToolStripMenuItem validateToolStripMenuItem;
      private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
      private System.Windows.Forms.Button resetButton;
      private System.Windows.Forms.Button applyButton;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox injectedVolume;
      private System.Windows.Forms.TextBox flowRate;
      private System.Windows.Forms.Label label9;
      private System.Windows.Forms.Label label10;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Label label25;
      private System.Windows.Forms.Label label24;
      private System.Windows.Forms.TextBox sampleA2;
      private System.Windows.Forms.TextBox sampleDndc;
      private System.Windows.Forms.Label label23;
      private System.Windows.Forms.Label label22;
      private System.Windows.Forms.TextBox sampleUvExtinction;
      private System.Windows.Forms.TextBox sampleConcentration;
      private System.Windows.Forms.Label label21;
      private System.Windows.Forms.Label label20;
      private System.Windows.Forms.Label label19;
      private System.Windows.Forms.Label label18;
      private System.Windows.Forms.Label label17;
      private System.Windows.Forms.Label label16;
      private System.Windows.Forms.TextBox sampleName;
      private System.Windows.Forms.Label label7;
      private System.Windows.Forms.TextBox sampleDescription;
      private System.Windows.Forms.GroupBox groupBox3;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.TextBox duration;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.TextBox experimentDescription;
      private System.Windows.Forms.GroupBox groupBox2;
      private System.Windows.Forms.ColumnHeader experimentColumn;
      private System.Windows.Forms.ListView experimentList;
      private System.Windows.Forms.ToolStripMenuItem showASTRAToolStripMenuItem;
      private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
      private System.Windows.Forms.CheckBox useInstrumentCC;
      private System.Windows.Forms.ToolStripMenuItem getBaselinesToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem openExperimentToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem getPeakRangesToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem getResultsToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem getDataSetToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem autofindToolStripMenuItem;
   }
}
