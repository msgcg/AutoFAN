using System;
using System.Collections.Generic;
using FanCtrl.Resources;
using System.Windows.Forms;

namespace FanCtrl
{
    public partial class IntelligentForm : ThemeForm
    {
        private List<MappingResult> mCurrentResults = new List<MappingResult>();

        public IntelligentForm()
        {
            InitializeComponent();
            this.localizeComponent();

            mStartButton.Click += (s, e) => { Start(); };
            mStopButton.Click += (s, e) => { Stop(); };
            mAcceptButton.Click += (s, e) => { AcceptMapping(); };
            mCreateProfileButton.Click += (s, e) => { CreateProfile(); };
            mCancelButton.Click += (s, e) => { this.Close(); };

            IntelligentManager.getInstance().onLog += (msg) => { this.BeginInvoke(new Action(() => { AddLog(msg); })); };
            IntelligentManager.getInstance().onProgress += (p) => { this.BeginInvoke(new Action(() => { mProgressBar.Value = p; })); };
            IntelligentManager.getInstance().onFinished += (list) => { this.BeginInvoke(new Action(() => { ShowResults(list); })); };

            SetupDataGridView();
        }

        private void localizeComponent()
        {
            this.Text = "Intelligent Mode";
            this.mStartButton.Text = "Start";
            this.mStopButton.Text = "Stop";
            this.mAcceptButton.Text = "Accept";
            this.mCreateProfileButton.Text = "Create Profile";
            this.mCancelButton.Text = "Cancel";
        }

        private void SetupDataGridView()
        {
            mMappingDataGridView.Columns.Clear();

            // Sensor Name (read-only text)
            var colSensor = new DataGridViewTextBoxColumn();
            colSensor.Name = "SensorName";
            colSensor.HeaderText = "Temperature Sensor";
            colSensor.Width = 150;
            colSensor.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colSensor);

            // Detected Control (dropdown list, editable)
            var colDetectedControl = new DataGridViewComboBoxColumn();
            colDetectedControl.Name = "DetectedControl";
            colDetectedControl.HeaderText = "Detected Control";
            colDetectedControl.Width = 150;
            // Populate with available controls
            var hw = HardwareManager.getInstance();
            foreach (var control in hw.ControlBaseList)
            {
                colDetectedControl.Items.Add(control.ID);
            }
            mMappingDataGridView.Columns.Add(colDetectedControl);

            // Confidence (read-only percentage)
            var colConfidence = new DataGridViewTextBoxColumn();
            colConfidence.Name = "Confidence";
            colConfidence.HeaderText = "Confidence %";
            colConfidence.Width = 100;
            colConfidence.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colConfidence);

            mMappingDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void Start()
        {
            mStartButton.Enabled = false;
            mStopButton.Enabled = true;
            mResultListView.Items.Clear();
            mMappingDataGridView.Rows.Clear();
            mProgressBar.Value = 0;
            mAcceptButton.Enabled = false;
            mCreateProfileButton.Enabled = false;
            var _ = IntelligentManager.getInstance().StartMappingAsync();
        }

        private void Stop()
        {
            IntelligentManager.getInstance().Stop();
            mStartButton.Enabled = true;
            mStopButton.Enabled = false;
        }

        private void AddLog(string msg)
        {
            // append to listview as log entry
            var lvi = new System.Windows.Forms.ListViewItem(msg);
            mResultListView.Items.Add(lvi);
            mResultListView.EnsureVisible(mResultListView.Items.Count - 1);
        }

        private void ShowResults(List<MappingResult> list)
        {
            mCurrentResults = new List<MappingResult>(list);
            mResultListView.Items.Clear();
            mMappingDataGridView.Rows.Clear();

            foreach (var r in list)
            {
                // Add to log
                var lvi = new System.Windows.Forms.ListViewItem($"{r.TempID} -> {r.ControlID} (Delta: {r.Delta:F2}°C, Confidence: {r.Confidence:F1}%)");
                mResultListView.Items.Add(lvi);

                // Add to DataGridView for editing
                int rowIndex = mMappingDataGridView.Rows.Add();
                mMappingDataGridView.Rows[rowIndex].Cells["SensorName"].Value = r.TempID;
                mMappingDataGridView.Rows[rowIndex].Cells["DetectedControl"].Value = r.ControlID;
                mMappingDataGridView.Rows[rowIndex].Cells["Confidence"].Value = $"{r.Confidence:F1}%";
            }

            mStartButton.Enabled = true;
            mStopButton.Enabled = false;
            mAcceptButton.Enabled = true;
            mCreateProfileButton.Enabled = true;
        }

        private void AcceptMapping()
        {
            try
            {
                if (mMappingDataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("No mapping data to accept. Please run a scan first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update current results with edited values from DataGridView
                for (int i = 0; i < mMappingDataGridView.Rows.Count && i < mCurrentResults.Count; i++)
                {
                    string detectedControl = mMappingDataGridView.Rows[i].Cells["DetectedControl"].Value?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(detectedControl))
                    {
                        mCurrentResults[i].ControlID = detectedControl;
                    }
                    else
                    {
                        MessageBox.Show($"Row {i+1}: Please select a control for the mapping.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                MessageBox.Show("Mapping accepted. You can now create a profile.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accepting mapping: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateProfile()
        {
            try
            {
                if (mCurrentResults.Count == 0)
                {
                    MessageBox.Show("No mapping results available. Please run the scan first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check for empty or duplicate controls
                var seenControls = new HashSet<string>();
                for (int i = 0; i < mCurrentResults.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(mCurrentResults[i].ControlID))
                    {
                        MessageBox.Show($"Result {i+1}: Control cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    // Warn about duplicate control assignments
                    if (seenControls.Contains(mCurrentResults[i].ControlID))
                    {
                        var result = MessageBox.Show(
                            $"Warning: Multiple sensors are assigned to control '{mCurrentResults[i].ControlID}'.\n\nThis may cause unexpected behavior.\n\nDo you want to continue?",
                            "Duplicate Assignment Warning",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        if (result != DialogResult.Yes)
                            return;
                    }
                    seenControls.Add(mCurrentResults[i].ControlID);
                }

                // Create a new automatic profile based on mapping
                var cm = ControlManager.getInstance();
                var newProfile = CreateAutomaticProfile(mCurrentResults);

                if (newProfile != null)
                {
                    var profileList = new List<ControlData> { newProfile };
                    cm.setControlDataList(MODE_TYPE.NORMAL, profileList);
                    cm.write();
                    MessageBox.Show("Auto profile 'AutoFAN' created and saved to NORMAL mode!\n\nYou can now apply this profile using the mode menu.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating profile: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private ControlData CreateAutomaticProfile(List<MappingResult> results)
        {
            // Create a new control profile with auto-generated curves based on mapping
            var profile = new ControlData("AutoFAN");

            foreach (var result in results)
            {
                // Create a FanData for this control, mapped to the detected sensor
                var fan = new FanData(
                    id: result.ControlID,
                    unit: FanValueUnit.Size_5,
                    isStep: false,
                    hysteresis: 3,
                    auto: 0,
                    delayTime: 2
                );

                // Fill in the curve values (ValueList)
                // Simple linear curve: 30% at 50°C, 100% at 80°C
                // Using Size_5 (21 points for 0-100°C in 5° increments)
                for (int i = 0; i < fan.getMaxFanValue(); i++)
                {
                    int temp = i * 5;
                    int pwm;
                    if (temp < 50)
                        pwm = 30;
                    else if (temp > 80)
                        pwm = 100;
                    else
                        pwm = 30 + (int)((temp - 50) * 70.0 / 30.0);

                    fan.ValueList[i] = Math.Min(100, Math.Max(30, pwm));
                }

                profile.FanDataList.Add(fan);
            }

            return profile;
        }
    }
}

