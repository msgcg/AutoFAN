using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FanCtrl
{
    public class MappingResult
    {
        public string TempID { get; set; }
        public string TempName { get; set; }
        public string ControlID { get; set; }
        public string ControlName { get; set; }
        public double Delta { get; set; }
        public double Confidence { get; set; } // percent: 0-100
    }

    public class IntelligentManager
    {
        private static IntelligentManager sInstance = new IntelligentManager();
        public static IntelligentManager getInstance() { return sInstance; }

        private CancellationTokenSource mCts = null;

        public event Action<int> onProgress; // percent
        public event Action<string> onLog;
        public event Action<List<MappingResult>> onFinished;

        public bool IsRunning { get; private set; } = false;

        // Configuration constants
        private const int MAX_PWM_DURING_TEST = 100; // Increased to 100% for better deltas
        private const int TEST_PWM_DELTA = 30;       // Increment PWM by 30% for test
        private const int STABILIZATION_SAMPLES = 30; // Increased samples for better averaging
        private const int SAMPLE_INTERVAL_MS = 1000;  // Wait 1s between samples
        private const double CONFIDENCE_THRESHOLD = 1.5; // Increased threshold for validity

        // Start mapping asynchronously
        public async Task StartMappingAsync()
        {
            if (IsRunning) return;
            IsRunning = true;
            mCts = new CancellationTokenSource();
            var token = mCts.Token;

            var results = new List<MappingResult>();

            await Task.Run(() =>
            {
                try
                {
                    var hw = HardwareManager.getInstance();

                    // Snapshot controls and sensors
                    var tempList = hw.TempBaseList.ToList();
                    var allFans = hw.FanBaseList.ToList();
                    var allControls = hw.ControlBaseList.ToList();

                    // Filter controls: only those that have a corresponding fan with RPM > 0
                    var activeControls = new List<BaseControl>();
                    foreach (var control in allControls)
                    {
                        string controlDeviceName = GetHardwareDeviceName(control, hw.ControlList);
                        var deviceFans = allFans.Where(f => GetHardwareDeviceName(f, hw.FanList) == controlDeviceName).ToList();
                        
                        // Try to find if THIS specific control has a spinning fan
                        // We check if there's any fan on the same device that is actually spinning
                        // For GPUs, usually one control affects all fans. 
                        // For motherboards, we try to be more specific if the names match (e.g. "Fan #1" and "Control #1")
                        bool isActuallyActive = false;
                        
                        if (deviceFans.Count > 0)
                        {
                            // If it's a GPU or Kraken, usually any spinning fan on the device is enough
                            if (controlDeviceName.Contains("NVIDIA") || controlDeviceName.Contains("Kraken"))
                            {
                                isActuallyActive = deviceFans.Any(f => f.Value > 0);
                            }
                            else
                            {
                                // For Motherboards/SuperIO, try to find a fan with a matching index or name
                                // If names are like "Fan #1" and "Control #1", match them.
                                // Otherwise, fallback to any spinning fan on the device to be safe but skip if all are 0.
                                var matchingFan = deviceFans.FirstOrDefault(f => ExtractIndex(f.Name) == ExtractIndex(control.Name));
                                if (matchingFan != null)
                                {
                                    isActuallyActive = (matchingFan.Value > 0);
                                }
                                else
                                {
                                    isActuallyActive = deviceFans.Any(f => f.Value > 0);
                                }
                            }
                        }

                        if (isActuallyActive)
                        {
                            activeControls.Add(control);
                            onLog?.Invoke($"Including control: {control.Name} ({controlDeviceName})");
                        }
                        else
                        {
                            onLog?.Invoke($"Skipping control: {control.Name} (No RPM detected on {controlDeviceName})");
                        }
                    }

                    if (tempList.Count == 0 || activeControls.Count == 0)
                    {
                        onLog?.Invoke("No sensors or active controls detected.");
                        return;
                    }

                    onLog?.Invoke($"Starting test for {activeControls.Count} active controls and {tempList.Count} sensors.");

                    // Store original control values
                    var originalValues = new Dictionary<string, int>();
                    foreach (var control in activeControls)
                    {
                        originalValues[control.ID] = control.Value;
                    }

                    // Establish baseline: sample temps multiple times with controls at original speed
                    onLog?.Invoke("Establishing baseline temperatures...");
                    var baselineTemps = EstablishBaseline(tempList, 5);
                    if (baselineTemps == null)
                    {
                        onLog?.Invoke("Failed to establish baseline.");
                        return;
                    }

                    int total = activeControls.Count;
                    for (int i = 0; i < activeControls.Count; i++)
                    {
                        if (token.IsCancellationRequested) 
                        {
                            onLog?.Invoke("Test cancelled by user. Aggregating partial results...");
                            break;
                        }

                        var control = activeControls[i];
                        // ... (rest of the testing logic remains the same)
                        try
                        {
                            onLog?.Invoke($"Testing control: {control.ID} ({i+1}/{total})");

                            int original = originalValues[control.ID];
                            int max = control.getMaxSpeed();
                            int min = Math.Max(0, max / 4); // minimum 25% to avoid stalling

                            // Multi-level testing: test at different PWM levels
                            for (int level = 0; level < 2; level++) // Test at 2 levels
                            {
                                if (token.IsCancellationRequested) break;

                                int testValue = original + (TEST_PWM_DELTA * (level + 1));
                                testValue = Math.Min(MAX_PWM_DURING_TEST, Math.Max(min, testValue));

                                if (testValue == original)
                                    continue; // Skip if test value is same as original

                                onLog?.Invoke($"  Level {level + 1}: setting PWM to {testValue}");

                                // Apply test value
                                try { control.setSpeedWithTimer(testValue); }
                                catch { try { control.setSpeed(testValue); } catch { } }

                                // Wait for system to stabilize and sample temps
                                var deltaData = SampleTempDeltas(tempList, baselineTemps, STABILIZATION_SAMPLES, SAMPLE_INTERVAL_MS);

                                foreach (var kvp in deltaData)
                                {
                                    string tempID = kvp.Key;
                                    double delta = kvp.Value;
                                    var t = tempList.Find(x => x.ID == tempID);

                                    if (delta >= CONFIDENCE_THRESHOLD)
                                    {
                                        string tempFullName = GetFullDeviceName(t, hw.TempList);
                                        string controlFullName = GetFullDeviceName(control, hw.ControlList);
                                        results.Add(new MappingResult()
                                        {
                                            TempID = tempID,
                                            TempName = tempFullName,
                                            ControlID = control.ID,
                                            ControlName = controlFullName,
                                            Delta = delta,
                                            Confidence = 0 // Will be calculated after aggregation
                                        });
                                        onLog?.Invoke($"    sensor {tempFullName}: avg delta={delta:F2}°C");
                                    }
                                }

                                Thread.Sleep(2000); // Cool-down between levels
                            }

                            // Restore original control value
                            try { control.setSpeedWithTimer(original); }
                            catch { try { control.setSpeed(original); } catch { } }

                            Thread.Sleep(5000); // Wait for temps to return to baseline before next control test
                        }
                        catch (Exception ex)
                        {
                            onLog?.Invoke($"Error testing control {control.ID}: {ex.Message}");
                            // Restore on error
                            try { control.setSpeedWithTimer(originalValues[control.ID]); }
                            catch { try { control.setSpeed(originalValues[control.ID]); } catch { } }
                        }

                        int percent = (int)((i + 1) * 100 / total);
                        onProgress?.Invoke(percent);
                    }

                    // Aggregate results: for each temp find the control with max delta
                    var grouped = AggregateResults(results);

                    if (grouped.Count == 0)
                    {
                        onLog?.Invoke("Warning: No significant temperature deltas detected. Check hardware connectivity.");
                    }

                    onLog?.Invoke($"Mapping complete. Found {grouped.Count} sensor-control associations.");
                    onFinished?.Invoke(grouped);
                }
                finally
                {
                    IsRunning = false;
                }
            }, token);
        }

        private Dictionary<string, double> EstablishBaseline(List<BaseSensor> tempList, int samples)
        {
            var baseline = new Dictionary<string, List<int>>();
            foreach (var t in tempList)
            {
                baseline[t.ID] = new List<int>();
            }

            for (int i = 0; i < samples; i++)
            {
                foreach (var t in tempList)
                {
                    baseline[t.ID].Add(t.Value);
                }
                Thread.Sleep(SAMPLE_INTERVAL_MS);
            }

            // Average samples
            var result = new Dictionary<string, double>();
            foreach (var kvp in baseline)
            {
                result[kvp.Key] = kvp.Value.Average();
            }
            return result;
        }

        private Dictionary<string, double> SampleTempDeltas(List<BaseSensor> tempList, Dictionary<string, double> baseline, int samples, int intervalMs)
        {
            var deltas = new Dictionary<string, List<double>>();
            foreach (var t in tempList)
            {
                deltas[t.ID] = new List<double>();
            }

            for (int i = 0; i < samples; i++)
            {
                foreach (var t in tempList)
                {
                    double current = t.Value;
                    double baselineTemp = baseline.ContainsKey(t.ID) ? baseline[t.ID] : current;
                    double delta = baselineTemp - current; // positive if temp dropped
                    deltas[t.ID].Add(delta);
                }
                Thread.Sleep(intervalMs);
            }

            // Average deltas
            var result = new Dictionary<string, double>();
            foreach (var kvp in deltas)
            {
                result[kvp.Key] = kvp.Value.Average();
            }
            return result;
        }

        private List<MappingResult> AggregateResults(List<MappingResult> allResults)
        {
            if (allResults.Count == 0)
                return new List<MappingResult>();

            // Group by TempID and select control with maximum delta
            var grouped = new List<MappingResult>();
            var byTemp = allResults.GroupBy(r => r.TempID);

            double maxDelta = allResults.Max(r => r.Delta);

            foreach (var group in byTemp)
            {
                var best = group.OrderByDescending(x => x.Delta).First();

                // Calculate confidence: (this delta / max delta) * 100
                double confidence = (best.Delta / maxDelta) * 100.0;

                grouped.Add(new MappingResult()
                {
                    TempID = best.TempID,
                    TempName = best.TempName,
                    ControlID = best.ControlID,
                    ControlName = best.ControlName,
                    Delta = best.Delta,
                    Confidence = confidence
                });
            }

            return grouped.OrderBy(r => r.Confidence).Reverse().ToList();
        }

        private string GetFullDeviceName(BaseDevice device, List<List<HardwareDevice>> hwList)
        {
            if (device == null) return "Unknown";
            foreach (var hwTypeGroup in hwList)
            {
                foreach (var hwDevice in hwTypeGroup)
                {
                    if (hwDevice.DeviceList.Contains(device))
                    {
                        return $"{hwDevice.Name} - {device.Name}";
                    }
                }
            }
            return device.Name;
        }

        private string GetHardwareDeviceName(BaseDevice device, List<List<HardwareDevice>> hwList)
        {
            if (device == null) return "Unknown";
            foreach (var hwTypeGroup in hwList)
            {
                foreach (var hwDevice in hwTypeGroup)
                {
                    if (hwDevice.DeviceList.Contains(device))
                    {
                        return hwDevice.Name;
                    }
                }
            }
            return "Unknown";
        }

        private string ExtractIndex(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            var digits = name.Where(char.IsDigit).ToArray();
            return new string(digits);
        }

        public void Stop()
        {
            if (!IsRunning) return;
            mCts?.Cancel();
            IsRunning = false;
        }
    }
}
