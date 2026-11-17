using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public class FanComboItem
        {
            public string ID { get; set; }
            public string DisplayName { get; set; }

            public FanComboItem(string id, string displayName)
            {
                ID = id;
                DisplayName = displayName;
            }

            public override string ToString() => DisplayName;
        }

        private List<MappingResult> mCurrentResults = new List<MappingResult>();
        private BindingList<FanComboItem> mFanComboList = new BindingList<FanComboItem>();

        private static readonly object sNativeLock = new object();
        private static bool sNativeConfigured = false;
        private static bool sIsGpuBackend = false;
        private static string sBackendName = "CPU";

        private readonly object mModelLock = new object();
        private LLamaWeights mCachedWeights = null;
        private ModelParams mCachedParams = null;
        private string mCachedModelPath = null;

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

                lock (mModelLock)
                {
                    mCachedWeights?.Dispose();
                    mCachedWeights = null;
                    mCachedParams = null;
                    mCachedModelPath = null;
                }
            };

            SetupDataGridView();
            SetButtonsState(isTesting: false, isAiRunning: false);
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

        private string GetControlDisplayName(BaseControl control)
        {
            if (control == null) return "";
            var hw = HardwareManager.getInstance();
            string parentDevice = null;
            foreach (var hwTypeGroup in hw.ControlList)
            {
                foreach (var hwDevice in hwTypeGroup)
                {
                    if (hwDevice.DeviceList.Contains(control))
                    {
                        parentDevice = hwDevice.Name;
                        break;
                    }
                }
                if (parentDevice != null) break;
            }

            if (!string.IsNullOrEmpty(parentDevice) && !string.Equals(parentDevice, control.Name, StringComparison.OrdinalIgnoreCase))
            {
                return $"{control.Name} ({parentDevice})";
            }
            return control.Name ?? control.ID ?? "";
        }

        private void SetupDataGridView()
        {
            bool isDark = OptionManager.getInstance().getNowTheme() == THEME_TYPE.DARK;

            // Setup ListBox styles
            if (isDark)
            {
                mResultListBox.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
                mResultListBox.ForeColor = System.Drawing.Color.Gainsboro;
            }
            else
            {
                mResultListBox.BackColor = System.Drawing.SystemColors.Window;
                mResultListBox.ForeColor = System.Drawing.SystemColors.WindowText;
            }
            mResultListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // Setup DataGridView styles
            if (isDark)
            {
                mMappingDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(60, 63, 65);
                mMappingDataGridView.ForeColor = System.Drawing.Color.Gainsboro;
                mMappingDataGridView.GridColor = System.Drawing.Color.FromArgb(81, 81, 81);
                mMappingDataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(57, 60, 62);
                mMappingDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.Gainsboro;
                mMappingDataGridView.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
                mMappingDataGridView.DefaultCellStyle.ForeColor = System.Drawing.Color.Gainsboro;
                mMappingDataGridView.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(92, 92, 92);
            }
            else
            {
                mMappingDataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
                mMappingDataGridView.ForeColor = System.Drawing.SystemColors.WindowText;
                mMappingDataGridView.GridColor = System.Drawing.SystemColors.ControlDark;
                mMappingDataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
                mMappingDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
                mMappingDataGridView.DefaultCellStyle.BackColor = System.Drawing.SystemColors.Window;
                mMappingDataGridView.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
                mMappingDataGridView.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            }
            mMappingDataGridView.EnableHeadersVisualStyles = false;

            // Populate fan combo items
            mFanComboList.Clear();
            var hw = HardwareManager.getInstance();
            foreach (var control in hw.ControlBaseList)
            {
                mFanComboList.Add(new FanComboItem(control.ID, GetControlDisplayName(control)));
            }

            mMappingDataGridView.Columns.Clear();

            // Sensor Name
            var colSensor = new DataGridViewTextBoxColumn();
            colSensor.Name = "SensorName";
            colSensor.HeaderText = "Датчик температуры";
            colSensor.ReadOnly = true;
            colSensor.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colSensor.FillWeight = 50;
            colSensor.MinimumWidth = 200;
            mMappingDataGridView.Columns.Add(colSensor);

            // Base Temp
            var colBase = new DataGridViewTextBoxColumn();
            colBase.Name = "BaseTemp";
            colBase.HeaderText = "База °C";
            colBase.Width = 65;
            colBase.ReadOnly = true;
            colBase.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            mMappingDataGridView.Columns.Add(colBase);

            // Test Temp
            var colTest = new DataGridViewTextBoxColumn();
            colTest.Name = "TestTemp";
            colTest.HeaderText = "Тест °C";
            colTest.Width = 65;
            colTest.ReadOnly = true;
            colTest.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            mMappingDataGridView.Columns.Add(colTest);

            // Delta
            var colDelta = new DataGridViewTextBoxColumn();
            colDelta.Name = "Delta";
            colDelta.HeaderText = "Дельта °C";
            colDelta.Width = 70;
            colDelta.ReadOnly = true;
            colDelta.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            mMappingDataGridView.Columns.Add(colDelta);

            // RPM
            var colRPM = new DataGridViewTextBoxColumn();
            colRPM.Name = "RPM";
            colRPM.HeaderText = "Макс. RPM";
            colRPM.Width = 75;
            colRPM.ReadOnly = true;
            colRPM.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            mMappingDataGridView.Columns.Add(colRPM);

            // Detected Control
            var colDetectedControl = new DataGridViewComboBoxColumn();
            colDetectedControl.Name = "DetectedControl";
            colDetectedControl.HeaderText = "Вентилятор";
            colDetectedControl.DataSource = mFanComboList;
            colDetectedControl.ValueMember = "ID";
            colDetectedControl.DisplayMember = "DisplayName";
            colDetectedControl.DropDownWidth = 380;
            colDetectedControl.FlatStyle = FlatStyle.Flat;
            colDetectedControl.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDetectedControl.FillWeight = 50;
            colDetectedControl.MinimumWidth = 220;
            mMappingDataGridView.Columns.Add(colDetectedControl);

            // Confidence
            var colConfidence = new DataGridViewTextBoxColumn();
            colConfidence.Name = "Confidence";
            colConfidence.HeaderText = "Уверенность %";
            colConfidence.Width = 95;
            colConfidence.ReadOnly = true;
            colConfidence.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            mMappingDataGridView.Columns.Add(colConfidence);

            mMappingDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            mMappingDataGridView.DataError += (s, e) => { e.Cancel = true; };
        }

        private void SetButtonsState(bool isTesting, bool isAiRunning)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetButtonsState(isTesting, isAiRunning)));
                return;
            }

            if (isTesting)
            {
                mStartButton.Enabled = false;
                mStopButton.Enabled = true;
                mAcceptButton.Enabled = false;
                mOptimizeAIButton.Enabled = false;
            }
            else if (isAiRunning)
            {
                mStartButton.Enabled = false;
                mStopButton.Enabled = false;
                mAcceptButton.Enabled = false;
                mOptimizeAIButton.Enabled = false;
            }
            else
            {
                bool hasResults = mCurrentResults != null && mCurrentResults.Count > 0;
                mStartButton.Enabled = true;
                mStopButton.Enabled = false;
                mAcceptButton.Enabled = hasResults;
                mOptimizeAIButton.Enabled = hasResults;
            }
        }

        private void Start()
        {
            if (IntelligentManager.getInstance().IsRunning) return;

            mResultListBox.Items.Clear();
            mResultListBox.HorizontalExtent = 0;
            mMappingDataGridView.Rows.Clear();
            mCurrentResults.Clear();
            mProgressBar.Value = 0;
            SetButtonsState(isTesting: true, isAiRunning: false);

            var _ = IntelligentManager.getInstance().StartMappingAsync();
        }

        private void Stop()
        {
            IntelligentManager.getInstance().Stop();
            mStopButton.Enabled = false;
            mStartButton.Enabled = false;
            AddLog("Остановка теста, возврат исходных настроек...");
        }

        private void AddLog(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => AddLog(msg)));
                return;
            }
            mResultListBox.Items.Add(msg);
            mResultListBox.TopIndex = mResultListBox.Items.Count - 1;

            try
            {
                using (var g = mResultListBox.CreateGraphics())
                {
                    int textWidth = (int)g.MeasureString(msg, mResultListBox.Font).Width + 30;
                    if (textWidth > mResultListBox.HorizontalExtent)
                    {
                        mResultListBox.HorizontalExtent = textWidth;
                    }
                }
            }
            catch { }
        }

        private void ShowResults(List<MappingResult> list)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ShowResults(list)));
                return;
            }

            mCurrentResults = new List<MappingResult>(list ?? new List<MappingResult>());
            mMappingDataGridView.Rows.Clear();

            var hw = HardwareManager.getInstance();

            foreach (var r in mCurrentResults)
            {
                if (!string.IsNullOrEmpty(r.ControlID) && !mFanComboList.Any(item => item.ID == r.ControlID))
                {
                    var matching = hw.ControlBaseList.FirstOrDefault(c => c.ID == r.ControlID);
                    string disp = matching != null ? GetControlDisplayName(matching) : (r.ControlName ?? r.ControlID);
                    mFanComboList.Add(new FanComboItem(r.ControlID, disp));
                }

                int rowIndex = mMappingDataGridView.Rows.Add();
                var row = mMappingDataGridView.Rows[rowIndex];
                row.Cells["SensorName"].Value = r.TempName ?? r.TempID;
                row.Cells["BaseTemp"].Value = $"{r.BaseTemp:F1}";
                row.Cells["TestTemp"].Value = $"{r.TestTemp:F1}";
                row.Cells["Delta"].Value = $"{r.Delta:F1}";
                row.Cells["RPM"].Value = r.RPM;
                row.Cells["DetectedControl"].Value = !string.IsNullOrEmpty(r.ControlID) ? r.ControlID : null;
                row.Cells["Confidence"].Value = $"{r.Confidence:F0}%";
            }

            SetButtonsState(isTesting: false, isAiRunning: false);
        }

        private async void OptimizeWithAI()
        {
            if (mCurrentResults == null || mCurrentResults.Count == 0) return;
            if (IntelligentManager.getInstance().IsRunning) return;

            // Sync any manual edits from the DataGridView first
            for (int i = 0; i < mMappingDataGridView.Rows.Count && i < mCurrentResults.Count; i++)
            {
                string sName = mMappingDataGridView.Rows[i].Cells["SensorName"].Value?.ToString();
                string fID = mMappingDataGridView.Rows[i].Cells["DetectedControl"].Value?.ToString();
                var res = mCurrentResults.FirstOrDefault(r => r.TempName == sName || r.TempID == sName);
                if (res != null)
                {
                    res.ControlID = fID;
                    var ctl = HardwareManager.getInstance().ControlBaseList.FirstOrDefault(c => c.ID == fID);
                    if (ctl != null) res.ControlName = ctl.Name;
                }
            }

            SetButtonsState(isTesting: false, isAiRunning: true);
            AddLog("Запуск AI анализа...");

            try
            {
                // Run in a separate thread to keep UI responsive
                await System.Threading.Tasks.Task.Run(async () =>
                {
                    string appDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                    lock (sNativeLock)
                    {
                        if (!sNativeConfigured)
                        {
                            try
                            {
                                var config = NativeLibraryConfig.All;
                                config.WithCuda(); // Automatically falls back to Vulkan if CUDA is unavailable
                                config.WithVulkan(); // Falls back to CPU if Vulkan is unavailable
                                config.WithLogCallback((level, message) =>
                                {
                                    System.Diagnostics.Debug.WriteLine($"[LLama {level}] {message}");
                                    if (message.Contains("ggml_cuda") || message.Contains("CUDA") || message.Contains("cuda"))
                                    {
                                        sIsGpuBackend = true;
                                        sBackendName = "CUDA";
                                    }
                                    else if (message.Contains("ggml_vulkan") || message.Contains("Vulkan") || message.Contains("vulkan"))
                                    {
                                        sIsGpuBackend = true;
                                        sBackendName = "Vulkan";
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"LLama native config: {ex.Message}");
                            }
                            sNativeConfigured = true;
                        }
                    }

                    string modelRelPath = @"src\models\phi-3.5-mini-instruct-q4.gguf";
                    string absPath = System.IO.Path.Combine(appDir, modelRelPath);

                    if (!System.IO.File.Exists(absPath) && !System.IO.File.Exists(modelRelPath))
                    {
                        this.BeginInvoke(new Action(() => AddLog("Файл модели не найден: " + modelRelPath)));
                        return;
                    }

                    // Workaround for Cyrillic paths: pass a relative ASCII path to llama.cpp
                    System.Environment.CurrentDirectory = appDir;
                    string modelPath = modelRelPath;

                    lock (mModelLock)
                    {
                        if (mCachedWeights == null || mCachedModelPath != modelPath)
                        {
                            mCachedWeights?.Dispose();
                            mCachedWeights = null;

                            var parameters = new LLama.Common.ModelParams(modelPath)
                            {
                                ContextSize = 4096,
                                GpuLayerCount = 99 // Offloads to GPU if CUDA is active, ignored otherwise
                            };
                            mCachedParams = parameters;
                            mCachedWeights = LLama.LLamaWeights.LoadFromFile(parameters);
                            mCachedModelPath = modelPath;
                        }
                    }

                    var executor = new LLama.StatelessExecutor(mCachedWeights, mCachedParams);

                    this.BeginInvoke(new Action(() =>
                    {
                        if (sIsGpuBackend)
                        {
                            AddLog($"AI Инфо: Запуск с аппаратным ускорением ({sBackendName}).");
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

                    var diagnosticData = mCurrentResults.Select(r => new
                    {
                        Sensor = r.TempName,
                        AssignedFan = r.ControlName,
                        AssignedFanID = r.ControlID,
                        Delta = double.IsInfinity(r.Delta) || double.IsNaN(r.Delta) ? 0 : r.Delta,
                        RPM = r.RPM,
                        Confidence = double.IsInfinity(r.Confidence) || double.IsNaN(r.Confidence) ? 0 : r.Confidence
                    }).ToList();

                    var jsonOptions = new System.Text.Json.JsonSerializerOptions
                    {
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
                                this.BeginInvoke(new Action(() =>
                                {
                                    var changes = new List<string>();
                                    foreach (var item in corrected)
                                    {
                                        if (item.ValueKind != System.Text.Json.JsonValueKind.Object) continue;

                                        string sName = null;
                                        string fID = null;

                                        if (item.TryGetProperty("SensorName", out var sNameProp)) sName = sNameProp.GetString();
                                        if (item.TryGetProperty("CorrectedFanID", out var fIDProp)) fID = fIDProp.GetString();

                                        if (sName == null || fID == null) continue;

                                        var res = mCurrentResults.FirstOrDefault(r => r.TempName == sName || r.TempID == sName);
                                        if (res != null)
                                        {
                                            var hwInst = HardwareManager.getInstance();
                                            var matchingControl = hwInst.ControlBaseList.FirstOrDefault(c => c.ID == fID);
                                            if (matchingControl != null && res.ControlID != fID)
                                            {
                                                string oldFan = string.IsNullOrEmpty(res.ControlName) ? res.ControlID : res.ControlName;
                                                changes.Add($"{sName}: {oldFan} -> {matchingControl.Name}");
                                                res.ControlID = fID;
                                                res.ControlName = matchingControl.Name;
                                            }
                                            else if (matchingControl == null && res.ControlID != fID)
                                            {
                                                changes.Add($"[Внимание] AI предложил неизвестный ID вентилятора для {sName}: '{fID}'");
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
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                            System.Console.WriteLine("AI Parse Error: " + ex.ToString());
                            this.BeginInvoke(new Action(() => AddLog("AI Ошибка обработки: " + ex.Message)));
                        }
                    }
                    else
                    {
                        this.BeginInvoke(new Action(() => AddLog("AI Ошибка: Модель вернула пустой ответ.")));
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                System.Console.WriteLine("AI Error: " + ex.ToString());
                this.BeginInvoke(new Action(() => AddLog("AI Ошибка: " + ex.Message)));
            }
            finally
            {
                SetButtonsState(isTesting: false, isAiRunning: false);
            }
        }

        private void AcceptMapping()
        {
            // Sync any manual edits from DataGridView back to mCurrentResults
            foreach (DataGridViewRow row in mMappingDataGridView.Rows)
            {
                if (row.IsNewRow) continue;
                string sName = row.Cells["SensorName"].Value?.ToString();
                string fID = row.Cells["DetectedControl"].Value?.ToString();

                var res = mCurrentResults.FirstOrDefault(r => r.TempName == sName || r.TempID == sName);
                if (res != null)
                {
                    res.ControlID = fID;
                    var ctl = HardwareManager.getInstance().ControlBaseList.FirstOrDefault(c => c.ID == fID);
                    if (ctl != null) res.ControlName = ctl.Name;
                }
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
