namespace AstraClient
{
   partial class TreeViewDialog
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TreeViewDialog));
         this.treeView = new System.Windows.Forms.TreeView();
         this.imageList1 = new System.Windows.Forms.ImageList(this.components);
         this.treeViewLabel = new System.Windows.Forms.Label();
         this.okButton = new System.Windows.Forms.Button();
         this.cancelButton = new System.Windows.Forms.Button();
         this.treeViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.expandCollapseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
         this.expandAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.collapseAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.treeViewContextMenu.SuspendLayout();
         this.SuspendLayout();
         // 
         // treeView
         // 
         this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.treeView.HideSelection = false;
         this.treeView.ImageIndex = 0;
         this.treeView.ImageList = this.imageList1;
         this.treeView.Location = new System.Drawing.Point(12, 38);
         this.treeView.Name = "treeView";
         this.treeView.PathSeparator = "/";
         this.treeView.SelectedImageIndex = 0;
         this.treeView.Size = new System.Drawing.Size(333, 312);
         this.treeView.TabIndex = 1;
         this.treeView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterCollapse);
         this.treeView.DoubleClick += new System.EventHandler(this.ValidateAndClose);
         this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
         this.treeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterExpand);
         // 
         // imageList1
         // 
         this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
         this.imageList1.TransparentColor = System.Drawing.Color.Red;
         this.imageList1.Images.SetKeyName(0, "folder_closed.bmp");
         this.imageList1.Images.SetKeyName(1, "folder_open.bmp");
         this.imageList1.Images.SetKeyName(2, "document.bmp");
         // 
         // treeViewLabel
         // 
         this.treeViewLabel.AutoSize = true;
         this.treeViewLabel.Location = new System.Drawing.Point(12, 9);
         this.treeViewLabel.Name = "treeViewLabel";
         this.treeViewLabel.Size = new System.Drawing.Size(80, 13);
         this.treeViewLabel.TabIndex = 0;
         this.treeViewLabel.Text = "TreeView label:";
         // 
         // okButton
         // 
         this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.okButton.Enabled = false;
         this.okButton.Location = new System.Drawing.Point(189, 356);
         this.okButton.Name = "okButton";
         this.okButton.Size = new System.Drawing.Size(75, 23);
         this.okButton.TabIndex = 2;
         this.okButton.Text = "OK";
         this.okButton.UseVisualStyleBackColor = true;
         this.okButton.Click += new System.EventHandler(this.ValidateAndClose);
         // 
         // cancelButton
         // 
         this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.cancelButton.Location = new System.Drawing.Point(270, 356);
         this.cancelButton.Name = "cancelButton";
         this.cancelButton.Size = new System.Drawing.Size(75, 23);
         this.cancelButton.TabIndex = 3;
         this.cancelButton.Text = "Cancel";
         this.cancelButton.UseVisualStyleBackColor = true;
         // 
         // treeViewContextMenu
         // 
         this.treeViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandCollapseMenuItem,
            this.toolStripSeparator1,
            this.expandAllMenuItem,
            this.collapseAllMenuItem});
         this.treeViewContextMenu.Name = "treeNodeContextMenu";
         this.treeViewContextMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
         this.treeViewContextMenu.ShowImageMargin = false;
         this.treeViewContextMenu.Size = new System.Drawing.Size(115, 76);
         this.treeViewContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.treeNodeContextMenu_Opening);
         // 
         // expandCollapseMenuItem
         // 
         this.expandCollapseMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
         this.expandCollapseMenuItem.Name = "expandCollapseMenuItem";
         this.expandCollapseMenuItem.Size = new System.Drawing.Size(114, 22);
         this.expandCollapseMenuItem.Text = "Expand";
         this.expandCollapseMenuItem.Click += new System.EventHandler(this.expandCollapseMenuItem_Click);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(111, 6);
         // 
         // expandAllMenuItem
         // 
         this.expandAllMenuItem.Name = "expandAllMenuItem";
         this.expandAllMenuItem.Size = new System.Drawing.Size(114, 22);
         this.expandAllMenuItem.Text = "&Expand All";
         this.expandAllMenuItem.Click += new System.EventHandler(this.expandAllMenuItem_Click);
         // 
         // collapseAllMenuItem
         // 
         this.collapseAllMenuItem.Name = "collapseAllMenuItem";
         this.collapseAllMenuItem.Size = new System.Drawing.Size(114, 22);
         this.collapseAllMenuItem.Text = "&Collapse All";
         this.collapseAllMenuItem.Click += new System.EventHandler(this.collapseAllMenuItem_Click);
         // 
         // TreeViewDialog
         // 
         this.AcceptButton = this.okButton;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.cancelButton;
         this.ClientSize = new System.Drawing.Size(357, 391);
         this.Controls.Add(this.cancelButton);
         this.Controls.Add(this.okButton);
         this.Controls.Add(this.treeView);
         this.Controls.Add(this.treeViewLabel);
         this.DoubleBuffered = true;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "TreeViewDialog";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "Dialog Title";
         this.Load += new System.EventHandler(this.TreeViewDialog_Load);
         this.treeViewContextMenu.ResumeLayout(false);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TreeView treeView;
      private System.Windows.Forms.Label treeViewLabel;
      private System.Windows.Forms.Button okButton;
      private System.Windows.Forms.Button cancelButton;
      private System.Windows.Forms.ImageList imageList1;
      private System.Windows.Forms.ContextMenuStrip treeViewContextMenu;
      private System.Windows.Forms.ToolStripMenuItem expandCollapseMenuItem;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      private System.Windows.Forms.ToolStripMenuItem expandAllMenuItem;
      private System.Windows.Forms.ToolStripMenuItem collapseAllMenuItem;
   }
}