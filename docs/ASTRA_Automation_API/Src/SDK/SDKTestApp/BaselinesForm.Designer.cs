
namespace AstraClient
{
   partial class BaselinesForm
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
            this.components = new System.ComponentModel.Container();
            this.baselineDetailsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.bType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bX1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bX2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bY1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bY2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.baselineDetailsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // baselineDetailsBindingSource
            // 
            this.baselineDetailsBindingSource.DataSource = typeof(AstraLib.BaselineDetails);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(850, 477);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(143, 28);
            this.button1.TabIndex = 1;
            this.button1.Text = "Apply";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Baselines_Apply);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(850, 522);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(143, 28);
            this.button2.TabIndex = 2;
            this.button2.Text = "Close";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Baselines_Close);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.bType,
            this.bName,
            this.bX1,
            this.bX2,
            this.bY1,
            this.bY2});
            this.dataGridView1.Location = new System.Drawing.Point(16, 15);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(824, 533);
            this.dataGridView1.TabIndex = 3;
            // 
            // bType
            // 
            this.bType.Frozen = true;
            this.bType.HeaderText = "Type";
            this.bType.MinimumWidth = 6;
            this.bType.Name = "bType";
            this.bType.ReadOnly = true;
            this.bType.Width = 125;
            // 
            // bName
            // 
            this.bName.Frozen = true;
            this.bName.HeaderText = "Name";
            this.bName.MinimumWidth = 6;
            this.bName.Name = "bName";
            this.bName.ReadOnly = true;
            this.bName.Width = 125;
            // 
            // bX1
            // 
            this.bX1.HeaderText = "X1";
            this.bX1.MinimumWidth = 6;
            this.bX1.Name = "bX1";
            this.bX1.Width = 125;
            // 
            // bX2
            // 
            this.bX2.HeaderText = "X2";
            this.bX2.MinimumWidth = 6;
            this.bX2.Name = "bX2";
            this.bX2.Width = 125;
            // 
            // bY1
            // 
            this.bY1.HeaderText = "Y1";
            this.bY1.MinimumWidth = 6;
            this.bY1.Name = "bY1";
            this.bY1.Width = 125;
            // 
            // bY2
            // 
            this.bY2.HeaderText = "Y2";
            this.bY2.MinimumWidth = 6;
            this.bY2.Name = "bY2";
            this.bY2.Width = 125;
            // 
            // BaselinesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 563);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BaselinesForm";
            this.Text = "Baselines";
            ((System.ComponentModel.ISupportInitialize)(this.baselineDetailsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

      }

      #endregion
      private System.Windows.Forms.BindingSource baselineDetailsBindingSource;
      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.Button button2;
      private System.Windows.Forms.DataGridView dataGridView1;
      private System.Windows.Forms.DataGridViewTextBoxColumn bType;
      private System.Windows.Forms.DataGridViewTextBoxColumn bName;
      private System.Windows.Forms.DataGridViewTextBoxColumn bX1;
      private System.Windows.Forms.DataGridViewTextBoxColumn bX2;
      private System.Windows.Forms.DataGridViewTextBoxColumn bY1;
      private System.Windows.Forms.DataGridViewTextBoxColumn bY2;
   }
}