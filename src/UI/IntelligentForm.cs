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

            // Current Control (read-only text, shows what's currently mapped)
            var colCurrentControl = new DataGridViewTextBoxColumn();
            colCurrentControl.Name = "CurrentControl";
            colCurrentControl.HeaderText = "Current Control";
            colCurrentControl.Width = 120;
            colCurrentControl.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colCurrentControl);

            // Detected Control (dropdown list, editable)
            var colDetectedControl = new DataGridViewComboBoxColumn();
            colDetectedControl.Name = "DetectedControl";
            colDetectedControl.HeaderText = "Detected Control";
            colDetectedControl.Width = 120;
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
            colConfidence.Width = 80;
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
                mMappingDataGridView.Rows[rowIndex].Cells["CurrentControl"].Value = GetCurrentControlForSensor(r.TempID);
                mMappingDataGridView.Rows[rowIndex].Cells["DetectedControl"].Value = r.ControlID;
                mMappingDataGridView.Rows[rowIndex].Cells["Confidence"].Value = $"{r.Confidence:F1}%";
            }

            mStartButton.Enabled = true;
            mStopButton.Enabled = false;
            mAcceptButton.Enabled = true;
            mCreateProfileButton.Enabled = true;
        }

        private string GetCurrentControlForSensor(string sensorID)
        {
            // Lookup current control for sensor from ControlManager
            var cm = ControlManager.getInstance();
            var currentData = cm.ControlDataList;
            foreach (var data in currentData)
            {
                foreach (var fan in data.FanDataList)
                {
                    if (fan.ID == sensorID)
                        return data.ID;
                }
            }
            return "-";
        }

        private void AcceptMapping()
        {
            try
            {
                // Update current results with edited values from DataGridView
                for (int i = 0; i < mMappingDataGridView.Rows.Count; i++)
                {
                    string detectedControl = mMappingDataGridView.Rows[i].Cells["DetectedControl"].Value?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(detectedControl))
                    {
                        mCurrentResults[i].ControlID = detectedControl;
                    }
                }

                // Save mapping to ControlManager
                SaveMappingToControlManager(mCurrentResults);

                MessageBox.Show("Mapping saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                mAcceptButton.Enabled = false;
                mCreateProfileButton.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving mapping: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateProfile()
        {
            try
            {
                // First accept the mapping
                AcceptMapping();

                // Create a new automatic profile based on mapping
                var cm = ControlManager.getInstance();
                var newProfile = CreateAutomaticProfile(mCurrentResults);

                if (newProfile != null)
                {
                    cm.setControlDataList(new MODE_TYPE.CUSTOM, new List<ControlData> { newProfile });
                    cm.write();
                    MessageBox.Show("Auto profile created and saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating profile: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMappingToControlManager(List<MappingResult> results)
        {
            var cm = ControlManager.getInstance();
            var currentData = cm.ControlDataList;

            // Update each ControlData with new fan mappings
            foreach (var result in results)
            {
                // Find or create FanData for this mapping
                var sensorID = result.TempID;
                var controlID = result.ControlID;

                // Find the control in its ControlData and update reference
                foreach (var data in currentData)
                {
                    foreach (var fan in data.FanDataList)
                    {
                        if (fan.ID == sensorID)
                        {
                            // Update mapping: sensor is now mapped to controlID
                            fan.ID = controlID; // This is simplified; actual implementation may need different approach
                        }
                    }
                }
            }

            cm.setControlDataList(cm.ModeType, currentData);
            cm.write();
        }

        private ControlData CreateAutomaticProfile(List<MappingResult> results)
        {
            // Create a new control profile with auto-generated curves
            var profile = new ControlData();
            profile.ID = "AutoProfile_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            foreach (var result in results)
            {
                var fan = new FanData();
                fan.ID = result.ControlID;
                fan.Name = $"Auto_{result.TempID}";

                // Create a simple curve: 30% at 50°C, 100% at 80°C
                var curve = new CURVE_TYPE();
                curve.Add(new CURVE_POINT() { X = 50, Y = 30 });
                curve.Add(new CURVE_POINT() { X = 65, Y = 50 });
                curve.Add(new CURVE_POINT() { X = 80, Y = 100 });

                fan.CurveList.Clear();
                fan.CurveList.Add(result.TempID, curve); // Map sensor to this curve

                profile.FanDataList.Add(fan);
            }

            return profile;
        }
    }
}

