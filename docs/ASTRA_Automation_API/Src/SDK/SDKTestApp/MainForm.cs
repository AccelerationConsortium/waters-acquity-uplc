using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using AstraLib;
using SDKCommon;
using System.IO;

namespace AstraClient
{
    // Main form class.
    public partial class MainForm : Form
    {
        private bool astraVisible_ = false;
        private const int waitingBlinkTicks_ = 5;
        private int ticks_ = 0;

        // ListViewItem images
        private enum ListImage
        {
            Ready,
            Waiting,
            Busy1,
            Busy2,
            Busy3,
            Busy4,
            Busy5,
            HasData
        }

        public delegate void ExperimentMethodCallback (Experiment experiment);

        // Constructor.
        public MainForm ()
        {
            InitializeComponent ();
            experimentColumn.Width = experimentList.ClientSize.Width;
            DisplayExperiment (null);
            AstraAdmin.Get.SetAutomationIdentity ("SDKTestApp", "1.0.0.0", Process.GetCurrentProcess ().Id, $"{Guid.NewGuid ()}", 1);
            AstraAdmin.Get.ExperimentClosed += OnExperimentClosed;
            AstraAdmin.Get.ExperimentStatusChanged += OnExperimentStatusChanged;
        }

        public void Shutdown ()
        {
            AstraAdmin.Get.ExperimentClosed -= OnExperimentClosed;
            AstraAdmin.Get.ExperimentStatusChanged -= OnExperimentStatusChanged;
        }

        // Populate form with experiment details.
        private void DisplayExperiment (Experiment experiment)
        {
            if (experiment != null)
            {
                experimentDescription.Text = experiment.Description;
                duration.Text = experiment.CollectionDuration.ToString ("f3");
                flowRate.Text = experiment.FlowRate.ToString ("f3");
                injectedVolume.Text = experiment.InjectedVolume.ToString ("f3");
                sampleName.Text = experiment.Sample.name;
                sampleDescription.Text = experiment.Sample.description;
                sampleDndc.Text = experiment.Sample.dndc.ToString ("f3");
                sampleA2.Text = experiment.Sample.a2.ToString ("e4");
                sampleUvExtinction.Text = experiment.Sample.uvExtinction.ToString ("e4");
                sampleConcentration.Text = experiment.Sample.concentration.ToString ("e4");

                // Tag each TextBox with the corresponding value from the Experiment,
                // so that we can tell when a field is changing (for validation purposes).
                experimentDescription.Tag = experiment.Description;
                duration.Tag = experiment.CollectionDuration;
                flowRate.Tag = experiment.FlowRate;
                injectedVolume.Tag = experiment.InjectedVolume;
                sampleName.Tag = experiment.Sample.name;
                sampleDescription.Tag = experiment.Sample.description;
                sampleDndc.Tag = experiment.Sample.dndc;
                sampleA2.Tag = experiment.Sample.a2;
                sampleUvExtinction.Tag = experiment.Sample.uvExtinction;
                sampleConcentration.Tag = experiment.Sample.concentration;
            }
            else
            {
                experimentDescription.Text = "";
                duration.Text = "";
                flowRate.Text = "";
                injectedVolume.Text = "";
                sampleName.Text = "";
                sampleDescription.Text = "";
                sampleDndc.Text = "";
                sampleA2.Text = "";
                sampleUvExtinction.Text = "";
                sampleConcentration.Text = "";
            }

            applyButton.Enabled = false;
            resetButton.Enabled = false;
        }

        // Update experiment with form values and apply changes.
        private void UpdateExperiment (Experiment experiment)
        {
            if (experiment == null)
                return;

            experiment.Description = experimentDescription.Text;

            double result;

            if (Double.TryParse (duration.Text, out result))
                experiment.CollectionDuration = result;

            if (Double.TryParse (flowRate.Text, out result))
                experiment.FlowRate = result;

            if (Double.TryParse (injectedVolume.Text, out result))
                experiment.InjectedVolume = result;

            SampleInfo sample = new SampleInfo ();
            sample.name = sampleName.Text;
            sample.description = sampleDescription.Text;

            if (Double.TryParse (sampleDndc.Text, out result))
                sample.dndc = result;

            if (Double.TryParse (sampleA2.Text, out result))
                sample.a2 = result;

            if (Double.TryParse (sampleUvExtinction.Text, out result))
                sample.uvExtinction = result;

            if (Double.TryParse (sampleConcentration.Text, out result))
                sample.concentration = result;

            experiment.Sample = sample;
            experiment.Apply ();
            applyButton.Enabled = false;
            resetButton.Enabled = false;
        }

        // Get the currently selected experiment.
        private Experiment GetSelectedExperiment ()
        {
            Experiment experiment = null;

            if (experimentList.SelectedItems.Count == 1)
            {
                ListViewItem selectedItem = experimentList.SelectedItems [0];
                int experimentID = (int) selectedItem.Tag;
                experiment = AstraAdmin.Get.GetExperiment (experimentID);
            }

            return experiment;
        }

        // Find the list item corresponding to the experiment
        private ListViewItem FindExperimentItem (Experiment experiment)
        {
            foreach (ListViewItem item in experimentList.Items)
            {
                if ((int) item.Tag == experiment.Id)
                    return item;
            }
            return null;
        }

        // Prompt user to choose an experiment template and return the id.
        private string SelectExperimentTemplate ()
        {
            const string prefix = "//dbf";
            const char seperator = '/';

            List<TreeViewDialog.TreeViewItem> items = new List<TreeViewDialog.TreeViewItem> ();
            Array templates = AstraAdmin.Get.GetExperimentTemplates ();

            if (templates == null)
            {
                MessageBox.Show ("Either System Database is unreachable, or SDK feature key not activated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            // Convert experiment templates to tree view items (its a pretty undramatic mapping)
            foreach (object template in templates)
            {
                // Split full path into name and path
                string fullPath = template as string;
                Debug.Assert (fullPath != null);

                if (fullPath != null)
                {
                    int pathLength = fullPath.LastIndexOf (seperator) + 1;

                    if (pathLength < fullPath.Length)
                    {
                        string path = fullPath.Substring (0, pathLength);
                        // Remove "//dbf/" prefix from path
                        if (path.Substring (0, prefix.Length) == prefix)
                            path = path.Remove (0, prefix.Length);

                        string name = fullPath.Substring (pathLength);

                        TreeViewDialog.TreeViewItem item;
                        item.data = fullPath;
                        item.path = path;
                        item.name = name;
                        items.Add (item);
                    }
                }
            }

            TreeViewDialog dialog = new TreeViewDialog ("Browse For Template", "Select a template for the new experiment:", false);
            dialog.Items = items;

            if (dialog.ShowDialog (this) == DialogResult.OK)
            {
                TreeViewDialog.TreeViewItem selectedItem = dialog.SelectedItem;
                Debug.Assert (selectedItem.data != null);
                if (selectedItem.data != null)
                    return (string) dialog.SelectedItem.data;
            }

            return null;
        }

        private string GetDataDatabaseDirectory ()
        {
            List<TreeViewDialog.TreeViewItem> items = new List<TreeViewDialog.TreeViewItem> ();
            Array directories = AstraAdmin.Get.GetDataDatabaseDirectory ("");

            if (directories == null)
            {
                MessageBox.Show ("Either Data Database is unreachable, or SDK feature key not activated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            var prefix = "//dbf";
            var seperator = "/";
            // Convert experiment templates to tree view items (its a pretty undramatic mapping)
            foreach (object element in directories)
            {
                // Split full path into name and path
                var fullPath = element as string;
                Debug.Assert (fullPath != null);

                if (fullPath != null)
                {
                    fullPath.Replace ('\\', '/');

                    var directory = seperator;
                    var fileName = "";
                    var fileNameOffset = 1;

                    if (fullPath.StartsWith (prefix))
                    {
                        fullPath = fullPath.Remove (0, prefix.Length);
                        fileNameOffset = fullPath.LastIndexOf (seperator) + 1;
                    }
                    else if (!fullPath.StartsWith (seperator))
                        fullPath = fullPath.Insert (0, "/");

                    directory = fullPath.Substring (0, fileNameOffset);
                    fileName = fullPath.Substring (fileNameOffset);

                    items.Add (new TreeViewDialog.TreeViewItem { path = directory, name = fileName, data = fullPath });
                }
            }

            var dialog = new TreeViewDialog ("Browse For Folder", "Select a save folder for the experiment:", true);
            dialog.Items = items;

            if (dialog.ShowDialog (this) == DialogResult.OK)
            {
                TreeViewDialog.TreeViewItem selectedItem = dialog.SelectedItem;
                Debug.Assert (selectedItem.data != null);
                if (selectedItem.data != null)
                    return (string) dialog.SelectedItem.data;
            }

            return null;
        }

        private void RemoveListItem (Experiment experiment)
        {
            var item = FindExperimentItem (experiment);
            if (item == null)
            {
                Debug.Assert (false, "Item should have an associated experiment");
            }
            else
            {
                experimentList.Items.Remove (item);
            }
        }

        // Update the status icon for the experiment in the list
        private void UpdateListItems ()
        {
            const int progressStart = (int) ListImage.Busy1;
            const int progressEnd = (int) ListImage.Busy5;

            foreach (ListViewItem item in experimentList.Items)
            {
                var experiment = AstraAdmin.Get.GetExperiment ((int) item.Tag);
                if (experiment == null)
                {
                    continue;
                }

                switch (experiment.Status)
                {
                    case ExperimentStatus.Ready:
                        item.ImageIndex = experiment.HasData ? (int) ListImage.HasData : (int) ListImage.Ready;
                        break;

                    case ExperimentStatus.Busy:
                        // Start/restart animation or advance to next frame
                        if (item.ImageIndex < progressStart || item.ImageIndex >= progressEnd)
                            item.ImageIndex = progressStart;
                        else
                            item.ImageIndex += 1;
                        break;

                    case ExperimentStatus.WaitingForAutoInject:
                        // Toggle waiting image every few frames
                        if (++ticks_ == waitingBlinkTicks_)
                        {
                            if (item.ImageIndex == (int) ExperimentStatus.WaitingForAutoInject)
                                item.ImageIndex = (int) ExperimentStatus.Ready;
                            else
                                item.ImageIndex = (int) ExperimentStatus.WaitingForAutoInject;

                            ticks_ = 0;
                        }
                        break;
                    case ExperimentStatus.Executed:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException ();
                }
            }
        }

        #region ToolStrip Events

        // File menu is opening. Enable items based on whether an experiment is selected.
        private void fileToolStripMenuItem_DropDownOpening (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();
            bool enable = experiment != null;

            saveToolStripMenuItem.Enabled = enable;
            closeToolStripMenuItem.Enabled = enable;
            showASTRAToolStripMenuItem.Text = astraVisible_ ? "Hide &ASTRA" : "Show &ASTRA";
        }

        // Create a new experiment from template.
        private void newToolStripMenuItem_Click (object sender, EventArgs e)
        {
            string template = SelectExperimentTemplate ();

            if (String.IsNullOrEmpty (template))
                return;

            int experimentID = AstraAdmin.Get.NewExperimentFromTemplate (template);
            Experiment experiment = AstraAdmin.Get.GetExperiment (experimentID);

            if (experiment == null)
                return;

            ListViewItem item = new ListViewItem (experiment.Name);
            item.Tag = experiment.Id;
            item.Selected = true;
            experimentList.Items.Add (item);
        }

        private void openExperimentToolStripMenuItem_Click (object sender, EventArgs e)
        {
            if ((null == openFileDialog) || (openFileDialog.ShowDialog () != DialogResult.OK))
                return;

            var fullPath = openFileDialog.FileName;
            if (string.IsNullOrEmpty (fullPath))
                return;

            var item = new ListViewItem ("Loading new experiment...") { Tag = -1, Selected = true };
            experimentList.Items.Add (item);

            var experimentID = AstraAdmin.Get.OpenExperiment (fullPath);
            var experiment = AstraAdmin.Get.GetExperiment (experimentID);

            if (experiment == null)
            {
                experimentList.Items.Remove (item);
                return;
            }

            item.Text = experiment.Name;
            item.Tag= experimentID;
        }

        // Save experiment.
        private void saveToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var experiment = GetSelectedExperiment ();
            var fullPath = "";

            if (experiment == null)
                return;

            if (!AstraAdmin.Get.IsSecurityPackActive ())
            {
                if ((null == saveFileDialog) || (saveFileDialog.ShowDialog () != DialogResult.OK))
                    return;

                fullPath = saveFileDialog.FileName;
                var fileName = Path.GetFileNameWithoutExtension (fullPath);
                var dirPath = Path.GetDirectoryName (fullPath);

                fullPath = dirPath + "\\" + fileName;
            }
            else
            {
                var directory = GetDataDatabaseDirectory ();

                if (String.IsNullOrEmpty (directory))
                    return;

                if (!directory.EndsWith ("\\") && !directory.EndsWith ("/"))
                    directory += "/";

                fullPath = "//dbf" + directory + experiment.Name;
            }

            AstraAdmin.Get.SaveExperiment (experiment.Id, fullPath);
        }

        private void exportToolStripMenuItem_Click (object sender, EventArgs e)
        {
            if ((null == saveFileDialog) || (saveFileDialog.ShowDialog () != DialogResult.OK))
                return;

            Experiment experiment = GetSelectedExperiment ();
            string fileName = saveFileDialog.FileName;

            AstraAdmin.Get.SaveExperiment (experiment.Id, fileName);
        }

        // Close experiment.
        private void closeToolStripMenuItem_Click (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();

            if (experiment != null)
                AstraAdmin.Get.CloseExperiment (experiment.Id);
        }

        // Show/hide the ASTRA application window.
        private void showASTRAToolStripMenuItem_Click (object sender, EventArgs e)
        {
            astraVisible_ = !astraVisible_;
            AstraAdmin.Get.ShowWindow (astraVisible_);
        }

        // Exit the client.
        private void exitToolStripMenuItem_Click (object sender, EventArgs e)
        {
            Shutdown ();
            AstraAdmin.Get.Shutdown ();

            this.Close ();
        }

        // Experiment menu is opening. Enable items based on whether selected experiment
        // is collecting.
        private void experimentToolStripMenuItem_DropDownOpening (object sender, EventArgs e)
        {
            var experiment = GetSelectedExperiment ();

            if (experiment == null)
            {
                validateToolStripMenuItem.Enabled = false;
                startCollectionToolStripMenuItem.Enabled = false;
                stopCollectionToolStripMenuItem.Enabled = false;
            }
            else
            {
                bool ready = (experiment.Status == ExperimentStatus.Ready);
                validateToolStripMenuItem.Enabled = ready;
                startCollectionToolStripMenuItem.Enabled = ready && !experiment.HasData;

                bool canStop = false;
                if (experiment.Status == ExperimentStatus.Busy || experiment.Status == ExperimentStatus.WaitingForAutoInject)
                {
                    canStop = true;
                }

                stopCollectionToolStripMenuItem.Enabled = canStop;
            }
        }

        // Validate experiment prior to collection
        private void validateToolStripMenuItem_Click (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();

            if (experiment == null)
                return;

            string details;

            if (AstraAdmin.Get.ValidateExperiment (experiment.Id, out details))
                MessageBox.Show ("Experiment passed validation.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show (details, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // getBaselines
        private void getBaselinesToolStripMenuItem_Click (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();
            if (experiment == null)
            {
                MessageBox.Show ("No experiment selected.");
                return;
            }

            BaselinesForm form = new BaselinesForm (experiment.Id);
            form.Show ();
        }

        // getPeakRanges
        private void getPeakRangesToolStripMenuItem_Click (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();
            if (experiment == null)
            {
                MessageBox.Show ("No experiment selected.");
                return;
            }

            PeakRangesForm form = new PeakRangesForm (experiment.Id);
            form.Show ();
        }

        // Start a collection.
        private void startCollectionToolStripMenuItem_Click (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();
            if (experiment != null)
            {
                bool state = useInstrumentCC.Checked;
                AstraAdmin.Get.UseInstrumentCalibrationConstant (experiment.Id, state);
                AstraAdmin.Get.StartCollection (experiment.Id);
            }
        }

        // Stop a running collection.
        private void stopCollectionToolStripMenuItem_Click (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();

            if (experiment != null && !AstraAdmin.Get.StopCollection (experiment.Id))
                MessageBox.Show ("Unable to stop collection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Show about form.
        private void aboutToolStripMenuItem_Click (object sender, EventArgs e)
        {
            AboutDialog about = new AboutDialog ();
            about.ShowDialog ();
        }

        #endregion

        #region Other Events

        // Display detecting instruments dialog on startup (if needed)
        private void MainForm_Load (object sender, EventArgs e)
        {
            this.Show ();

            if (!AstraAdminUi.Logon ())
                fileToolStripMenuItem.DropDownItems.Remove (exportToolStripMenuItem);
            else
            {
                var activeUserInfo = AstraAdmin.Get.GetActiveUser ();

                if (!String.IsNullOrWhiteSpace (activeUserInfo.fullUserName))
                {
                    var suffix = String.Format (" - {0}", activeUserInfo.fullUserName);

                    this.Text = this.Text + suffix;
                }

                this.Text = this.Text.Replace ("ASTRA", "ASTRA SP");
            }

            if (!AstraAdmin.Get.HasInstrumentDetectionCompleted)
            {
                InstrumentDetection dialog = new InstrumentDetection ();
                if (DialogResult.Cancel == dialog.ShowDialog (this))
                    this.Close ();
            }
        }

        // Experiment closed, remove item from list.
        void OnExperimentClosed (object source, Experiment experiment)
        {
            this.BeginInvoke (new ExperimentMethodCallback (this.RemoveListItem), experiment);
        }

        // Experiment status changed in ASTRA, refresh screen if ready.
        private void OnExperimentStatusChanged (object source, Experiment experiment)
        {
            if (experiment.Status == ExperimentStatus.Ready)
            {
                this.BeginInvoke (new ExperimentMethodCallback (this.DisplayExperiment), experiment);
            }
        }

        // Change the selected experiment
        private void experimentList_SelectedIndexChanged (object sender, EventArgs e)
        {
            DisplayExperiment (GetSelectedExperiment ());
        }

        // Reset experiment to last applied state (or initial state).
        private void resetButton_Click (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();

            if (experiment != null)
            {
                experiment.Reset ();
                DisplayExperiment (experiment);
            }
        }

        // Communicate changes in experiment properties to ASTRA
        private void applyButton_Click (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();

            if (experiment == null)
                return;

            AstraAdmin.Get.WaitExperimentNotRunning (experiment.Id);

            UpdateExperiment (experiment);

            AstraAdmin.Get.WaitExperimentNotRunning (experiment.Id);

            experiment = GetSelectedExperiment ();

            DisplayExperiment (experiment);
        }

        // Perform validation for fields containing doubles.
        private void doubleField_Validating (object sender, CancelEventArgs e)
        {
            if (GetSelectedExperiment () == null)
                return;

            TextBox textBox = (TextBox) sender;
            double result;

            if (Double.TryParse (textBox.Text, out result))
            {
                // Check if entered value differs from Experiment value (stored in Tag).
                if ((double) textBox.Tag != result)
                {
                    applyButton.Enabled = true;
                    resetButton.Enabled = true;
                }
            }
            else
            {
                e.Cancel = true;
                textBox.SelectAll ();

                MessageBox.Show ("Please enter a floating point value.",
                                 "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Enable apply button if form data changes.
        private void textBox_TextChanged (object sender, EventArgs e)
        {
            if (GetSelectedExperiment () != null)
                applyButton.Enabled = true;
        }

        // Event handler for animation timer.
        private void animationTimer_Tick (object sender, EventArgs e)
        {
            UpdateListItems ();
        }

        #endregion

        private void getBaselines (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();
            if (experiment == null)
            {
                MessageBox.Show ("No experiment selected.");
                return;
            }
            Array baselines = AstraAdmin.Get.GetBaselines (experiment.Id);
        }

        private void getResultsToolStripMenuItem_Click (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();
            if (experiment == null)
            {
                MessageBox.Show ("No experiment selected.");
                return;
            }

            var form = new ResultsForm (experiment.Id);
            form.Show ();
        }

        private void getDataSetToolStripMenuItem_Click (object sender, EventArgs e)
        {
            Experiment experiment = GetSelectedExperiment ();
            if (experiment == null)
            {
                MessageBox.Show ("No experiment selected.");
                return;
            }

            var form = new DataSetForm (experiment.Id);
            form.Show ();
        }

        private void autofindToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var experiment = GetSelectedExperiment ();
            if (experiment == null)
            {
                MessageBox.Show ("No experiment selected.");
                return;
            }

            var success = AstraAdmin.Get.SetAutoAutofindBaselines (experiment.Id, true) && AstraAdmin.Get.SetAutoAutofindPeaks (experiment.Id, true);

            if (success)
                MessageBox.Show ("Autofind succeeded");
            else
                MessageBox.Show ("Autofind failed");
        }
    }
}