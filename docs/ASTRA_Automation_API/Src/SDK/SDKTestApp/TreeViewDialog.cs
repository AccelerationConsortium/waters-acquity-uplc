using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AstraClient
{
   // Generic dialog for choosing an item from a hierarchy.
   public partial class TreeViewDialog : Form
   {
      // Structure representing an item in the tree.
      public struct TreeViewItem
      {
         public string path;
         public string name;
         public object data;
      }

      // Enumeration for indexing into TreeView ImageList.
      private enum TreeNodeImage
      {
         FolderClosed = 0,
         FolderOpen,
         Template
      };

      // TreeView path items.
      private List<TreeViewItem> _items = new List<TreeViewItem>();
      private TreeViewItem       _selectedItem;
      private bool               _isFolderBrowser = false;

      // Constructor.
      public TreeViewDialog(string title, string message, bool isFolderBrowser)
      {
         _isFolderBrowser = isFolderBrowser;

         InitializeComponent();
         treeView.ContextMenuStrip = treeViewContextMenu;
         Text = title;
         treeViewLabel.Text = message;
      }

      // Items property for accessing items which the node collection will
      // be built from.
      public List<TreeViewItem> Items
      {
         get { return _items; }
         set { _items = value; }
      }

      // SelectedItem property for accessing item selected in dialog.
      public TreeViewItem SelectedItem
      {
         get { return _selectedItem; }
      }

      // Load tree nodes from item data.
      private void TreeViewDialog_Load(object sender, EventArgs e)
      {
         treeView.Nodes.Clear();
         treeView.BeginUpdate();

         foreach (TreeViewItem item in _items)
            AddNode(item);

         if (!_isFolderBrowser)
            treeView.TreeViewNodeSorter = new FullPathComparer();
         else
            treeView.TreeViewNodeSorter = new PathComparer ();

         treeView.EndUpdate();
      }

      // Add a node to the tree control.
      private void AddNode(TreeViewItem item)
      {
         // Split directory and experiment template name from path
         const char seperator = '/';
         string[] folders = item.path.Split(new char[] { seperator }, StringSplitOptions.RemoveEmptyEntries);

         // Try finding the child node path matching the template path, excluding the
         // deepest folder from the search path with each iteration (we are working up the path)
         var nodes    = treeView.Nodes;
         var index    = folders.Length;
         var rootNode = nodes.Find ("/", false);

         if (rootNode != null && rootNode.Length > 0)
         {
            rootNode[0].Expand ();
            nodes = rootNode[0].Nodes;
         }

         while (index > 0)
         {
            var searchPath = String.Format("/{0}/", String.Join(Char.ToString(seperator), folders, 0, index));
            var matches    = nodes.Find(searchPath, true);

            if (matches.Length == 1)
            {
               nodes = matches[0].Nodes;
               break;
            }
            --index;
         }

         TreeNode node = null;

         if (index < folders.Length)
         {
            // Add subfolder child nodes to parent
            for (int i = index; i < folders.Length; ++i)
            {
               var key = "";

               key      = String.Format ("/{0}/", String.Join (Char.ToString (seperator), folders, 0, i + 1));
               node     = nodes.Add (key, folders[i], (int)TreeNodeImage.FolderClosed, (int)TreeNodeImage.FolderClosed);
               node.Tag = (!_isFolderBrowser) ? null : item.data;
               nodes    = node.Nodes;
            }
         }

         if (!_isFolderBrowser)
         {
            node = nodes.Add(item.path + item.name, item.name, (int)TreeNodeImage.Template, (int)TreeNodeImage.Template);
            node.Tag = item.data;
         }
         else if ("/" == item.path)
         {
            node = nodes.Add (item.path, item.path, (int)TreeNodeImage.FolderClosed, (int)TreeNodeImage.FolderClosed);
            node.Tag = item.data;
         }
      }

      #region TreeView Events
      // Set node images to expanded image.
      private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
      {
         e.Node.ImageIndex = e.Node.SelectedImageIndex = (int)TreeNodeImage.FolderOpen;
      }

      // Set node images to collapsed image.
      private void treeView_AfterCollapse(object sender, TreeViewEventArgs e)
      {
         e.Node.ImageIndex = e.Node.SelectedImageIndex = (int)TreeNodeImage.FolderClosed;
      }

      // Enable/disable OK button depending on selection.
      private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
      {
         TreeNode node = treeView.SelectedNode;

         okButton.Enabled = node != null && node.Tag != null;
      }

      // Save user selection and close dialog if selection is valid.
      private void ValidateAndClose(object sender, EventArgs e)
      {
         TreeNode node = treeView.SelectedNode;

         if (node == null || node.Tag == null)
            return;

         var path = (!_isFolderBrowser) ? node.FullPath.Substring(0, node.FullPath.Length - node.Text.Length) : node.Name;

         _selectedItem.path = path;
         _selectedItem.name = node.Text;
         _selectedItem.data = node.Tag;

         DialogResult = DialogResult.OK;
      }

      #endregion

      #region ContextMenu Events
      // TreeView context menu is opening. We cancel the operation if the tree
      // is empty, and customize the menu to include an expand/collapse item if
      // a TreeNode has the focus.
      private void treeNodeContextMenu_Opening(object sender, CancelEventArgs e)
      {
         if (treeView.Nodes.Count == 0)
         {
            e.Cancel = true;
            return;
         }

         Point p = PointToScreen(treeView.Location);
         int x = treeViewContextMenu.Left - p.X;
         int y = treeViewContextMenu.Top - p.Y;

         TreeNode node = treeView.GetNodeAt(x, y);

         if (node != null)
         {
            // Normally right-clicking will not select an unselected node, and focus
            // jumps back to the previously selected node. Its annoying so we fix it!
            treeView.SelectedNode = node;

            ToolStripItem nodeItem = treeViewContextMenu.Items[0];
            ToolStripItem seperator = treeViewContextMenu.Items[1];

            if (node.Nodes.Count > 0)
            {
               nodeItem.Enabled = true;

               if (node.IsExpanded)
                  nodeItem.Text = "Collapse";
               else
                  nodeItem.Text = "Expand";
            }
            else
            {
               nodeItem.Text = "Expand";
               nodeItem.Enabled = false;
            }
         }

         e.Cancel = false;
      }

      // Expand or collapse the node, based on the nodes state.
      private void expandCollapseMenuItem_Click(object sender, EventArgs e)
      {
         TreeNode node = treeView.SelectedNode;

         if (node == null || !node.IsVisible)
            return;

         if (node.IsExpanded)
            node.Collapse();
         else
            node.Expand();
      }

      // Expand all tree nodes.
      private void expandAllMenuItem_Click(object sender, EventArgs e)
      {
         treeView.ExpandAll();
         treeView.SelectedNode.EnsureVisible();
      }

      // Collapse all tree nodes.
      private void collapseAllMenuItem_Click(object sender, EventArgs e)
      {
         treeView.CollapseAll();
      }
      #endregion
   }

   // Comparer class for sorting full path names. Folders end in a slash and precede
   // files in the sorting order.
   internal class FullPathComparer : IComparer
   {
      int IComparer.Compare(object a, object b)
      {
         string path1 = ((TreeNode)a).Name;
         string path2 = ((TreeNode)b).Name;

         if (path1[path1.Length - 1] == '/' && path2[path2.Length - 1] != '/')
         {
            return -1;   // path1 is a directory, path2 is a file
         }
         else if (path1[path1.Length - 1] != '/' && path2[path2.Length - 1] == '/')
         {
            return 1;  // path1 is a file, path2 is a directory
         }

         return path1.CompareTo(path2);
      }
   }

   internal class PathComparer : IComparer
   {
      int IComparer.Compare (object a, object b)
      {
         var path1 = ((TreeNode)a).Name;
         var path2 = ((TreeNode)b).Name;

         return path1.CompareTo (path2);
      }
   }
}