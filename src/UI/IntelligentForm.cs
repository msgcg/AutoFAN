using System;
using System.Collections.Generic;
using System.Linq;
using FanCtrl.Resources;
using System.Windows.Forms;
using LLama;
using LLama.Common;
using LLama.Native;
using System.Text.Json;

namespace FanCtrl
{
    public partial class IntelligentForm : ThemeForm
    {
        private List<MappingResult> mCurrentResults = new List<MappingResult>();


        private Action<string> mLogAction;
        private Action<int> mProgressAction;
        private Action<List<MappingResult>> mFinishedAction;

        public IntelligentForm()
        {
            InitializeComponent();
            this.localizeComponent();



            mStartButton.Click += (s, e) => { Start(); };
            mStopButton.Click += (s, e) => { Stop(); };
            mAcceptButton.Click += (s, e) => { AcceptMapping(); };

            mOptimizeAIButton.Click += (s, e) => { OptimizeWithAI(); };
            mCancelButton.Click += (s, e) => { this.Close(); };

            mLogAction = (msg) => { if (!this.IsDisposed && this.IsHandleCreated) this.BeginInvoke(new Action(() => { AddLog(msg); })); };
            mProgressAction = (p) => { if (!this.IsDisposed && this.IsHandleCreated) this.BeginInvoke(new Action(() => { mProgressBar.Value = p; })); };
            mFinishedAction = (list) => { if (!this.IsDisposed && this.IsHandleCreated) this.BeginInvoke(new Action(() => { ShowResults(list); })); };

            IntelligentManager.getInstance().onLog += mLogAction;
            IntelligentManager.getInstance().onProgress += mProgressAction;
            IntelligentManager.getInstance().onFinished += mFinishedAction;

            this.FormClosed += (s, e) => {
                IntelligentManager.getInstance().Stop();
                IntelligentManager.getInstance().onLog -= mLogAction;
                IntelligentManager.getInstance().onProgress -= mProgressAction;
                IntelligentManager.getInstance().onFinished -= mFinishedAction;
            };

            SetupDataGridView();
        }

        private void localizeComponent()
        {
            this.Text = "Умный режим";
            this.mStartButton.Text = "Старт";
            this.mStopButton.Text = "Стоп";
            this.mAcceptButton.Text = "Принять";

            this.mOptimizeAIButton.Text = "AI Коррекция";
            this.mCancelButton.Text = "Отмена";
        }

        private void SetupDataGridView()
        {
            // Setup ListBox styles
            mResultListBox.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            mResultListBox.ForeColor = System.Drawing.Color.Gainsboro;
            mResultListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // Setup DataGridView styles
            mMappingDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(60, 63, 65);
            mMappingDataGridView.ForeColor = System.Drawing.Color.Gainsboro;
            mMappingDataGridView.GridColor = System.Drawing.Color.FromArgb(81, 81, 81);
            mMappingDataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(57, 60, 62);
            mMappingDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.Gainsboro;
            mMappingDataGridView.EnableHeadersVisualStyles = false;
            mMappingDataGridView.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            mMappingDataGridView.DefaultCellStyle.ForeColor = System.Drawing.Color.Gainsboro;
            mMappingDataGridView.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(92, 92, 92);

            mMappingDataGridView.Columns.Clear();

            // Sensor Name
            var colSensor = new DataGridViewTextBoxColumn();
            colSensor.Name = "SensorName";
            colSensor.HeaderText = "Датчик температуры";
            colSensor.Width = 150;
            colSensor.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colSensor);

            // Base Temp
            var colBase = new DataGridViewTextBoxColumn();
            colBase.Name = "BaseTemp";
            colBase.HeaderText = "База °C";
            colBase.Width = 60;
            colBase.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colBase);

            // Test Temp
            var colTest = new DataGridViewTextBoxColumn();
            colTest.Name = "TestTemp";
            colTest.HeaderText = "Тест °C";
            colTest.Width = 60;
            colTest.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colTest);

            // Delta
            var colDelta = new DataGridViewTextBoxColumn();
            colDelta.Name = "Delta";
            colDelta.HeaderText = "Дельта °C";
            colDelta.Width = 60;
            colDelta.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colDelta);

            // RPM
            var colRPM = new DataGridViewTextBoxColumn();
            colRPM.Name = "RPM";
            colRPM.HeaderText = "Макс. RPM";
            colRPM.Width = 70;
            colRPM.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colRPM);

            // Detected Control
            var colDetectedControl = new DataGridViewComboBoxColumn();
            colDetectedControl.Name = "DetectedControl";
            colDetectedControl.HeaderText = "Вентилятор";
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
            colConfidence.HeaderText = "Уверенность %";
            colConfidence.Width = 100;
            colConfidence.ReadOnly = true;
            mMappingDataGridView.Columns.Add(colConfidence);

            mMappingDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void Start()
        {
            mStartButton.Enabled = false;
            mStopButton.Enabled = true;
            mResultListBox.Items.Clear();
            mMappingDataGridView.Rows.Clear();
            mProgressBar.Value = 0;
            mAcceptButton.Enabled = false;

            mOptimizeAIButton.Enabled = false;
            var _ = IntelligentManager.getInstance().StartMappingAsync();
        }

        private void Stop()
        {
            IntelligentManager.getInstance().Stop();
            mStartButton.Enabled = true;
            mStopButton.Enabled = false;
            mProgressBar.Value = 0;
        }

        private void AddLog(string msg)
        {
            mResultListBox.Items.Add(msg);
            mResultListBox.TopIndex = mResultListBox.Items.Count - 1;
        }

        private void ShowResults(List<MappingResult> list)
        {
            mCurrentResults = new List<MappingResult>(list);
            mMappingDataGridView.Rows.Clear();

            foreach (var r in list)
            {
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

            mOptimizeAIButton.Enabled = true;
        }

        private async void OptimizeWithAI()
        {
            if (mCurrentResults.Count == 0) return;
            
            // Sync any manual edits from the DataGridView first
            for (int i = 0; i < mMappingDataGridView.Rows.Count && i < mCurrentResults.Count; i++)
            {
                string sName = mMappingDataGridView.Rows[i].Cells["SensorName"].Value?.ToString();
                string fID = mMappingDataGridView.Rows[i].Cells["DetectedControl"].Value?.ToString();
                var res = mCurrentResults.FirstOrDefault(r => r.TempName == sName);
                if (res != null) res.ControlID = fID;
            }

            mOptimizeAIButton.Enabled = false;
            AddLog("Запуск AI анализа...");
            
            try
            {
                // Run in a separate thread to keep UI responsive
                await System.Threading.Tasks.Task.Run(async () =>
                {
                    string appDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    bool isCudaLoaded = false;
                    try
                    {
                        var config = NativeLibraryConfig.Instance;
                        config.WithCuda(); // Automatically falls back to Vulkan if CUDA is unavailable
                        config.WithVulkan(); // Falls back to CPU if Vulkan is unavailable
                        config.WithLogCallback((level, message) => {  
                            System.Diagnostics.Debug.WriteLine($"[LLama {level}] {message}");
                            if (message.Contains("ggml_cuda") || message.Contains("ggml_vulkan") || message.Contains("CUDA") || message.Contains("Vulkan")) 
                                isCudaLoaded = true;
                        });
                    }
                    catch { /* Might already be initialized */ }

                    string modelRelPath = @"src\models\phi-3.5-mini-instruct-q4.gguf";
                    string absPath = System.IO.Path.Combine(appDir, modelRelPath);

                    if (!System.IO.File.Exists(absPath) && !System.IO.File.Exists(modelRelPath))
                    {
                        this.BeginInvoke(new Action(() => AddLog("Файл модели не найден.")));
                        return;
                    }

                    // Workaround for Cyrillic paths: pass a relative ASCII path to llama.cpp
                    System.Environment.CurrentDirectory = appDir;
                    string modelPath = modelRelPath;

                    var parameters = new LLama.Common.ModelParams(modelPath) 
                    { 
                        ContextSize = 4096, 
                        GpuLayerCount = 99 // Offloads to GPU if CUDA is active, ignored otherwise
                    };

                    using (var weights = LLama.LLamaWeights.LoadFromFile(parameters))
                    {
                        var executor = new LLama.StatelessExecutor(weights, parameters);
                        
                        this.BeginInvoke(new Action(() => {
                            if (isCudaLoaded)
                            {
                                AddLog("AI Инфо: Запуск с аппаратным ускорением (CUDA/Vulkan).");
                            }
                            else
                            {
                                AddLog("AI Инфо: Аппаратное ускорение недоступно. Используется процессор (CPU).");
                                var hwInst = HardwareManager.getInstance();
                                bool hasGpu = hwInst.TempBaseList.Any(t => t.Name.IndexOf("GPU", StringComparison.OrdinalIgnoreCase) >= 0 || t.ID.IndexOf("GPU", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                              hwInst.ControlBaseList.Any(c => c.Name.IndexOf("GPU", StringComparison.OrdinalIgnoreCase) >= 0 || c.ID.IndexOf("GPU", StringComparison.OrdinalIgnoreCase) >= 0);
                                
                                if (hasGpu)
                                {
                                    AddLog("AI Внимание: Обнаружена видеокарта, но CUDA/Vulkan не загружен. Установите драйверы для предотвращения перегрева CPU!");
                                }
                            }
                        }));
                        
                        var hw = HardwareManager.getInstance();
                        var allControls = hw.ControlBaseList.Select(c => new { ID = c.ID, Name = c.Name }).ToList();
                        
                        var diagnosticData = mCurrentResults.Select(r => new { 
                            Sensor = r.TempName,
                            AssignedFan = r.ControlName,
                            AssignedFanID = r.ControlID,
                            Delta = double.IsInfinity(r.Delta) || double.IsNaN(r.Delta) ? 0 : r.Delta,
                            RPM = r.RPM,
                            Confidence = double.IsInfinity(r.Confidence) || double.IsNaN(r.Confidence) ? 0 : r.Confidence
                        }).ToList();
                        
                        var jsonOptions = new System.Text.Json.JsonSerializerOptions {
                            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
                        };

                        string prompt = "You are an expert PC hardware AI. The user ran a fan-control stress test.\n" +
                                        $"Available Fans: {System.Text.Json.JsonSerializer.Serialize(allControls, jsonOptions)}\n" +
                                        $"Heuristic Results: {System.Text.Json.JsonSerializer.Serialize(diagnosticData, jsonOptions)}\n\n" +
                                        "TASK: Map EACH Sensor from the Heuristic Results to the correct FanID from the Available Fans list.\n" +
                                        "CRITICAL RULES:\n" +
                                        "1. The 'AssignedFanID' in the Heuristic Results is often WRONG. You MUST use semantic matching based on the sensor and fan names.\n" +
                                        "2. If a Sensor is a GPU (e.g. NVIDIA), its CorrectedFanID MUST be the GPU fan (e.g. gpu-nvidia), NOT a motherboard chassis fan (lpc/nct...), regardless of the heuristic Delta.\n" +
                                        "3. If a Sensor is a CPU, its CorrectedFanID should ideally be a CPU fan.\n" +
                                        "4. Output a JSON array containing EVERY sensor with its 'SensorName' and 'CorrectedFanID'.\n" +
                                        "5. CorrectedFanID MUST EXACTLY match the literal 'ID' field from the Available Fans list (e.g. 'LHM/Control/...'). DO NOT use the 'Name' field as the ID!";

                        string response = "";
                        var inferenceParams = new LLama.Common.InferenceParams() { MaxTokens = 1024 };
                        LLama.Sampling.Grammar grammar = null;
                        string gbnfPath = System.IO.Path.Combine(appDir, @"src\models\json.gbnf");
                        if (System.IO.File.Exists(gbnfPath))
                        {
                            var gbnf = System.IO.File.ReadAllText(gbnfPath).Trim();
                            grammar = new LLama.Sampling.Grammar(gbnf, "root");
                        }

                        var pipeline = new LLama.Sampling.DefaultSamplingPipeline() { Temperature = 0.1f, Grammar = grammar };
                        inferenceParams.SamplingPipeline = pipeline;
                        // Use InferAsync with await foreach for LLamaSharp 0.10.0
                        await foreach (var text in executor.InferAsync(prompt, inferenceParams)) 
                        {
                            response += text;
                        }

                        System.Console.WriteLine("\n[AI RAW RESPONSE START]\n" + response + "\n[AI RAW RESPONSE END]\n");

                        if (!string.IsNullOrEmpty(response))
                        {
                            try
                            {
                                int startIdx = response.IndexOf('[');
                                int endIdx = response.LastIndexOf(']');
                                if (startIdx >= 0 && endIdx >= startIdx)
                                {
                                    var corrected = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(response.Substring(startIdx, endIdx - startIdx + 1));
                                    this.BeginInvoke(new Action(() => {
                                        var changes = new List<string>();
                                        foreach (var item in corrected)
                                        {
                                            if (item.ValueKind != System.Text.Json.JsonValueKind.Object) continue;
                                            
                                            string sName = null;
                                            string fID = null;
                                            
                                            if (item.TryGetProperty("SensorName", out var sNameProp)) sName = sNameProp.GetString();
                                            if (item.TryGetProperty("CorrectedFanID", out var fIDProp)) fID = fIDProp.GetString();
                                            
                                            if (sName == null || fID == null) continue;

                                            var res = mCurrentResults.FirstOrDefault(r => r.TempName == sName);
                                            if (res != null) 
                                            {
                                                var hw = HardwareManager.getInstance();
                                                bool isValidId = hw.ControlBaseList.Any(c => c.ID == fID);
                                                if (isValidId && res.ControlID != fID)
                                                {
                                                    changes.Add($"{sName}: {res.ControlID} -> {fID}");
                                                    res.ControlID = fID;
                                                }
                                                else if (!isValidId && res.ControlID != fID)
                                                {
                                                    changes.Add($"[Error] AI suggested invalid ID for {sName}: '{fID}'");
                                                }
                                            }
                                        }
                                        ShowResults(mCurrentResults);
                                        AddLog("AI Анализ применен.");
                                        foreach (var c in changes) AddLog(" > " + c);
                                        if (changes.Count == 0) AddLog(" > Изменения не потребовались.");
                                    }));
                                }
                                else
                                {
                                    this.BeginInvoke(new Action(() => AddLog("AI Ошибка: Модель не вернула корректный JSON массив.")));
                                }
                            }
                            catch (Exception ex) { 
                                System.Diagnostics.Debug.WriteLine(ex.ToString());
                                System.Console.WriteLine("AI Parse Error: " + ex.ToString());
                                this.BeginInvoke(new Action(() => AddLog("AI Ошибка обработки: " + ex.Message)));
                            }
                        }
                        else
                        {
                            this.BeginInvoke(new Action(() => AddLog("AI Ошибка: Модель вернула пустой ответ.")));
                        }
                    }
                });
            }
            catch (Exception ex) { 
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                System.Console.WriteLine("AI Error: " + ex.ToString());
                this.BeginInvoke(new Action(() => AddLog("AI Ошибка: " + ex.Message)));
            }
            finally { this.BeginInvoke(new Action(() => mOptimizeAIButton.Enabled = true)); }
        }

        private void AcceptMapping()
        {
            // Sync any manual edits from DataGridView back to mCurrentResults
            foreach (DataGridViewRow row in mMappingDataGridView.Rows)
            {
                if (row.IsNewRow) continue;
                string sName = row.Cells["SensorName"].Value?.ToString();
                string fID = row.Cells["DetectedControl"].Value?.ToString();
                
                var res = mCurrentResults.FirstOrDefault(r => r.TempName == sName);
                if (res != null) res.ControlID = fID;
            }
            
            try 
            {
                string appDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string mappingPath = System.IO.Path.Combine(appDir, "IntelligentMapping.json");
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals };
                System.IO.File.WriteAllText(mappingPath, System.Text.Json.JsonSerializer.Serialize(mCurrentResults, jsonOptions));
                MessageBox.Show("Маппинг принят и сохранен!\n\nТеперь вы можете перейти в меню 'Автоматическое управление' и нажать 'Импорт из умного режима' для настройки кривых этих вентиляторов.", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении маппинга: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
