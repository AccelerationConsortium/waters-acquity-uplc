namespace AstraClient
{
   partial class AboutDialog
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDialog));
         this.product = new System.Windows.Forms.Label();
         this.button1 = new System.Windows.Forms.Button();
         this.copyright = new System.Windows.Forms.Label();
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
         this.SuspendLayout();
         // 
         // product
         // 
         this.product.AutoSize = true;
         this.product.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.product.Location = new System.Drawing.Point(66, 17);
         this.product.Name = "product";
         this.product.Size = new System.Drawing.Size(109, 13);
         this.product.TabIndex = 1;
         this.product.Text = "ProductName Version";
         // 
         // button1
         // 
         this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.button1.Location = new System.Drawing.Point(328, 12);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(75, 23);
         this.button1.TabIndex = 2;
         this.button1.Text = "OK";
         this.button1.UseVisualStyleBackColor = true;
         // 
         // copyright
         // 
         this.copyright.AutoSize = true;
         this.copyright.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.copyright.Location = new System.Drawing.Point(66, 36);
         this.copyright.Name = "copyright";
         this.copyright.Size = new System.Drawing.Size(54, 13);
         this.copyright.TabIndex = 3;
         this.copyright.Text = "Copyright";
         // 
         // pictureBox1
         // 
         this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
         this.pictureBox1.Location = new System.Drawing.Point(26, 17);
         this.pictureBox1.Margin = new System.Windows.Forms.Padding(5);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Size = new System.Drawing.Size(32, 32);
         this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
         this.pictureBox1.TabIndex = 4;
         this.pictureBox1.TabStop = false;
         // 
         // AboutDialog
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(415, 87);
         this.Controls.Add(this.pictureBox1);
         this.Controls.Add(this.copyright);
         this.Controls.Add(this.button1);
         this.Controls.Add(this.product);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "AboutDialog";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "About";
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label product;
      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.Label copyright;
      private System.Windows.Forms.PictureBox pictureBox1;
   }
}