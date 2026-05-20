using System;
using System.Collections.Generic;
using System.Linq;
using FanCtrl.Resources;
using System.Windows.Forms;
using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Grammars;
using System.Text.Json;

namespace FanCtrl
{
    public partial class IntelligentForm : ThemeForm
    {
        private List<MappingResult> mCurrentResults = new List<MappingResult>();
        private ComboBox mModeComboBox;

        private Action<string> mLogAction;
        private Action<int> mProgressAction;
        private Action<List<MappingResult>> mFinishedAction;

        public IntelligentForm()
        {
            InitializeComponent();
            this.localizeComponent();

            // Dynamically add Mode selection UI
            var label = new DarkUI.Controls.DarkLabel { 
                Text = "Target Mode:", 
                Location = new System.Drawing.Point(mCreateProfileButton.Left, mCreateProfileButton.Top - 25), 
                AutoSize = true 
            };
            mModeComboBox = new ComboBox { 
                Location = new System.Drawing.Point(label.Right + 5, label.Top - 3), 
                Width = 100, 
                DropDownStyle = ComboBoxStyle.DropDownList 
            };
            mModeComboBox.Items.AddRange(new object[] { MODE_TYPE.NORMAL, MODE_TYPE.SILENCE, MODE_TYPE.PERFORMANCE, MODE_TYPE.GAME });
            mModeComboBox.SelectedItem = ControlManager.getInstance().ModeType;
            
            this.Controls.Add(label);
            this.Controls.Add(mModeComboBox);

            mStartButton.Click += (s, e) => { Start(); };
            mStopButton.Click += (s, e) => { Stop(); };
            mAcceptButton.Click += (s, e) => { AcceptMapping(); };
            mCreateProfileButton.Click += (s, e) => { CreateProfile(); };
            mOptimizeAIButton.Click += (s, e) => { OptimizeWithAI(); };
            mCancelButton.Click += (s, e) => { this.Close(); };

            mLogAction = (msg) => { if (!this.IsDisposed && this.IsHandleCreated) this.BeginInvoke(new Action(() => { AddLog(msg); })); };
            mProgressAction = (p) => { if (!this.IsDisposed && this.IsHandleCreated) this.BeginInvoke(new Action(() => { mProgressBar.Value = p; })); };
            mFinishedAction = (list) => { if (!this.IsDisposed && this.IsHandleCreated) this.BeginInvoke(new Action(() => { ShowResults(list); })); };

            IntelligentManager.getInstance().onLog += mLogAction;
            IntelligentManager.getInstance().onProgress += mProgressAction;
            IntelligentManager.getInstance().onFinished += mFinishedAction;

            this.FormClosed += (s, e) => {
                IntelligentManager.getInstance().onLog -= mLogAction;
                IntelligentManager.getInstance().onProgress -= mProgressAction;
                IntelligentManager.getInstance().onFinished -= mFinishedAction;
            };

            SetupDataGridView();
        }

        private void localizeComponent()
        {
            this.Text = "Intelligent Mode";
            this.mStartButton.Text = "Start";
            this.mStopButton.Text = "Stop";
            this.mAcceptButton.Text = "Accept";
            this.mCreateProfileButton.Text = "Create Profile";
            this.mOptimizeAIButton.Text = "AI Remap";
            this.mCancelButton.Text = "Cancel";
        }

        private void SetupDataGridView()
        {
            mMappingDataGridView.Columns.Clear();

            // Sensor Name
            var colSensor = new DataGridViewTextBoxColumn();
            colSensor.Name = "SensorName";
            colSensor.HeaderText = "Temperature Sensor";
            colSensor.Width = 150;
            colSensor.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colSensor);

            // Base Temp
            var colBase = new DataGridViewTextBoxColumn();
            colBase.Name = "BaseTemp";
            colBase.HeaderText = "Base °C";
            colBase.Width = 60;
            colBase.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colBase);

            // Test Temp
            var colTest = new DataGridViewTextBoxColumn();
            colTest.Name = "TestTemp";
            colTest.HeaderText = "Test °C";
            colTest.Width = 60;
            colTest.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colTest);

            // Delta
            var colDelta = new DataGridViewTextBoxColumn();
            colDelta.Name = "Delta";
            colDelta.HeaderText = "Delta °C";
            colDelta.Width = 60;
            colDelta.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colDelta);

            // RPM
            var colRPM = new DataGridViewTextBoxColumn();
            colRPM.Name = "RPM";
            colRPM.HeaderText = "Max RPM";
            colRPM.Width = 70;
            colRPM.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colRPM);

            // Detected Control
            var colDetectedControl = new DataGridViewComboBoxColumn();
            colDetectedControl.Name = "DetectedControl";
            colDetectedControl.HeaderText = "Assigned Fan";
            colDetectedControl.Width = 150;
            var hw = HardwareManager.getInstance();
            foreach (var control in hw.ControlBaseList)
            {
                colDetectedControl.Items.Add(control.ID);
            }
            mMappingDataGridView.Columns.Add(colDetectedControl);

            // Confidence
            var colConfidence = new DataGridViewTextBoxColumn();
            colConfidence.Name = "Confidence";
            colConfidence.HeaderText = "Conf %";
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
            mOptimizeAIButton.Enabled = false;
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
                var lvi = new System.Windows.Forms.ListViewItem(r.TempName ?? r.TempID);
                lvi.SubItems.Add(r.ControlName ?? r.ControlID);
                lvi.SubItems.Add($"{r.Delta:F1}°C");
                mResultListView.Items.Add(lvi);

                // Add to DataGridView
                int rowIndex = mMappingDataGridView.Rows.Add();
                var row = mMappingDataGridView.Rows[rowIndex];
                row.Cells["SensorName"].Value = r.TempName ?? r.TempID;
                row.Cells["BaseTemp"].Value = $"{r.BaseTemp:F1}";
                row.Cells["TestTemp"].Value = $"{r.TestTemp:F1}";
                row.Cells["Delta"].Value = $"{r.Delta:F1}";
                row.Cells["RPM"].Value = r.RPM;
                row.Cells["DetectedControl"].Value = r.ControlID;
                row.Cells["Confidence"].Value = $"{r.Confidence:F0}%";
            }

            mStartButton.Enabled = true;
            mStopButton.Enabled = false;
            mAcceptButton.Enabled = true;
            mCreateProfileButton.Enabled = true;
            mOptimizeAIButton.Enabled = true;
        }

        private async void OptimizeWithAI()
        {
            if (mCurrentResults.Count == 0) return;
            mOptimizeAIButton.Enabled = false;
            AddLog("Starting AI analysis (CUDA)...");
            
            try
            {
                // Run in a separate thread to keep UI responsive
                await System.Threading.Tasks.Task.Run(async () =>
                {
                    string appDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    
                    try
                    {
                        // The user confirmed NativeLibraryConfig exists but NOT in LLama.Native
                        // Try accessing it via LLama.NativeLibraryConfig (root namespace)
                        var config = NativeLibraryConfig.Instance;
                        
                        string llamaPath = System.IO.Path.Combine(appDir, "libllama.dll");
                        if (!System.IO.File.Exists(llamaPath)) 
                            llamaPath = System.IO.Path.Combine(appDir, @"runtimes\win-x64\native\libllama.dll");

                        if (System.IO.File.Exists(llamaPath)) {
                            config.WithLibrary(llamaPath, "");
                        }

                        config.WithCuda(true);
                        config.WithLogCallback((level, message) => {  // ← сначала level, потом message!
                            if (level >= LLamaLogLevel.Info)  // ✅ Теперь level — это enum LLamaLogLevel
                            {
                                // Безопасный вызов из фонового потока
                                this.BeginInvoke(new Action(() => AddLog($"[LLama] {message}")));
                            }
                        });
                    }
                    catch { /* Might already be initialized */ }

                    string modelPath = System.IO.Path.Combine(appDir, @"src\models\gemma-4-E2B-it-Q4_K_M.gguf");
                    if (!System.IO.File.Exists(modelPath)) modelPath = @"src\models\gemma-4-E2B-it-Q4_K_M.gguf";

                    if (!System.IO.File.Exists(modelPath))
                    {
                        this.BeginInvoke(new Action(() => AddLog("Model file not found.")));
                        return;
                    }

                    var parameters = new LLama.Common.ModelParams(modelPath) 
                    { 
                        ContextSize = 2048, 
                        GpuLayerCount = 99 // Offload all layers to GPU
                    };

                    using (var weights = LLama.LLamaWeights.LoadFromFile(parameters))
                    {
                        var executor = new LLama.StatelessExecutor(weights, parameters);
                        var hw = HardwareManager.getInstance();
                        var allControls = hw.ControlBaseList.Select(c => new { ID = c.ID, Name = c.Name }).ToList();
                        
                        var diagnosticData = mCurrentResults.Select(r => new { 
                            Sensor = r.TempName,
                            AssignedFan = r.ControlName,
                            AssignedFanID = r.ControlID,
                            Delta = r.Delta,
                            RPM = r.RPM,
                            Confidence = r.Confidence
                        }).ToList();
                        
                        string prompt = "You are a PC hardware diagnostic AI. I performed a stress test by ramping up fans to see which sensor cools down.\n" +
                                        $"Available Fans: {System.Text.Json.JsonSerializer.Serialize(allControls)}\n" +
                                        $"Test Results: {System.Text.Json.JsonSerializer.Serialize(diagnosticData)}\n\n" +
                                        "Correct any obvious mistakes. Trust high Delta values (> 4.0) above all else. " +
                                        "Return ONLY a JSON array: [{\"SensorName\": \"...\", \"CorrectedFanID\": \"...\"}]";

                        string response = "";
                        var inferenceParams = new LLama.Common.InferenceParams() { MaxTokens = 1024, Temperature = 0.1f };

                        string gbnfPath = System.IO.Path.Combine(appDir, @"src\models\json.gbnf");
                        if (System.IO.File.Exists(gbnfPath))
                        {
                            var gbnf = System.IO.File.ReadAllText(gbnfPath).Trim();
                            // Grammar class is in LLama.Grammars namespace in 0.10.0
                            inferenceParams.Grammar = LLama.Grammars.Grammar.Parse(gbnf, "root").CreateInstance();
                        }

                        // Use InferAsync with await foreach for LLamaSharp 0.10.0
                        await foreach (var text in executor.InferAsync(prompt, inferenceParams)) 
                        {
                            response += text;
                        }

                        if (!string.IsNullOrEmpty(response))
                        {
                            try
                            {
                                int startIdx = response.IndexOf('[');
                                int endIdx = response.LastIndexOf(']');
                                if (startIdx >= 0 && endIdx >= startIdx)
                                {
                                    var corrected = System.Text.Json.JsonSerializer.Deserialize<List<JsonElement>>(response.Substring(startIdx, endIdx - startIdx + 1));
                                    this.BeginInvoke(new Action(() => {
                                        foreach (var item in corrected)
                                        {
                                            string sName = item.GetProperty("SensorName").GetString();
                                            string fID = item.GetProperty("CorrectedFanID").GetString();
                                            var res = mCurrentResults.FirstOrDefault(r => r.TempName == sName);
                                            if (res != null) res.ControlID = fID;
                                        }
                                        ShowResults(mCurrentResults);
                                        AddLog("AI Analysis applied (CUDA).");
                                    }));
                                }
                            }
                            catch (Exception ex) { 
                                System.Diagnostics.Debug.WriteLine(ex.ToString());
                                this.BeginInvoke(new Action(() => AddLog("AI Parse Error: " + ex.Message))); 
                            }
                        }
                    }
                });
            }
            catch (Exception ex) { 
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                this.BeginInvoke(new Action(() => AddLog("AI Error: " + ex.Message))); 
            }
            finally { this.BeginInvoke(new Action(() => mOptimizeAIButton.Enabled = true)); }
        }

        private void AcceptMapping()
        {
            if (mMappingDataGridView.Rows.Count == 0) return;
            for (int i = 0; i < mMappingDataGridView.Rows.Count && i < mCurrentResults.Count; i++)
            {
                string sName = mMappingDataGridView.Rows[i].Cells["SensorName"].Value?.ToString();
                string fID = mMappingDataGridView.Rows[i].Cells["DetectedControl"].Value?.ToString();
                var res = mCurrentResults.FirstOrDefault(r => r.TempName == sName);
                if (res != null) res.ControlID = fID;
            }
            MessageBox.Show("Mapping accepted. Choose a mode and click 'Create Profile'.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CreateProfile()
        {
            if (mCurrentResults.Count == 0) return;
            
            MODE_TYPE targetMode = (MODE_TYPE)mModeComboBox.SelectedItem; 
            
            var cm = ControlManager.getInstance();
            var bySensor = mCurrentResults.GroupBy(r => r.TempID);
            var profileList = new List<ControlData>();

            foreach (var sensorGroup in bySensor)
            {
                var controlData = new ControlData(sensorGroup.Key);
                foreach (var mapping in sensorGroup)
                {
                    var fan = new FanData(mapping.ControlID, FanValueUnit.Size_5, false, 3, 0, 2);
                    for (int i = 0; i < fan.getMaxFanValue(); i++)
                    {
                        int temp = i * 5;
                        int minPwm, maxPwm, targetTemp;
                        switch(targetMode) {
                            case MODE_TYPE.SILENCE: minPwm = 20; maxPwm = 80; targetTemp = 90; break;
                            case MODE_TYPE.PERFORMANCE: minPwm = 40; maxPwm = 100; targetTemp = 75; break;
                            case MODE_TYPE.GAME: minPwm = 35; maxPwm = 100; targetTemp = 80; break;
                            default: minPwm = 30; maxPwm = 100; targetTemp = 85; break;
                        }
                        
                        if (temp < 40) fan.ValueList[i] = minPwm;
                        else if (temp > targetTemp) fan.ValueList[i] = maxPwm;
                        else fan.ValueList[i] = minPwm + (int)((temp - 40) * (maxPwm - minPwm) / (double)(targetTemp - 40));
                    }
                    controlData.FanDataList.Add(fan);
                }
                profileList.Add(controlData);
            }

            cm.setControlDataList(targetMode, profileList);
            cm.write();
            AddLog($"Profile successfully saved to {targetMode} in Control.json");
            MessageBox.Show($"Profile created and applied to {targetMode} mode!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
