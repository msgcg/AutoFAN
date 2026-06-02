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
        public double BaseTemp { get; set; }
        public double TestTemp { get; set; }
        public double Delta { get; set; }
        public int RPM { get; set; }
        public double Confidence { get; set; } // percent: 0-100
    }

    public class BaselineData
    {
        public double Average { get; set; }
        public double StdDev { get; set; }
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
        private const int MAX_PWM_DURING_TEST = 100; 
        private const int STABILIZATION_WAIT_SEC = 30; // More time for SSDs to react
        private const int SAMPLE_COUNT = 10;
        private const int SAMPLE_INTERVAL_MS = 1000;
        private const double CONFIDENCE_THRESHOLD = 0.3; // High sensitivity for SSDs

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
                    var allTemps = hw.TempBaseList.ToList();
                    var allFans = hw.FanBaseList.ToList();
                    var allControls = hw.ControlBaseList.ToList();

                    // Filter sensors: pick only ONE main sensor per hardware device
                    var tempList = new List<BaseSensor>();
                    foreach (var hwTypeGroup in hw.TempList)
                    {
                        foreach (var hwDevice in hwTypeGroup)
                        {
                            var sensors = hwDevice.DeviceList.Cast<BaseSensor>().ToList();
                            if (sensors.Count == 0) continue;

                            onLog?.Invoke($"Found sensors on {hwDevice.Name}: {string.Join(", ", sensors.Select(s => s.Name))}");

                            // Priority names for "Main" sensor
                            var priorityNames = new[] { "Package", "Core Max", "CPU Core", "GPU Core", "HotSpot", "Junction", "Drive", "Temperature", "CPU", "Core" };
                            BaseSensor mainSensor = null;
                            
                            foreach (var pName in priorityNames)
                            {
                                mainSensor = sensors.FirstOrDefault(s => s.Name.IndexOf(pName, StringComparison.OrdinalIgnoreCase) >= 0);
                                if (mainSensor != null) break;
                            }
                            
                            if (mainSensor == null) mainSensor = sensors.OrderByDescending(s => s.Value).First();
                            
                            tempList.Add(mainSensor);
                            onLog?.Invoke($"  -> Selected as main sensor for {hwDevice.Name}: {mainSensor.Name}");
                        }
                    }

                    // NEW: Spin up all controls to 100% to detect connected fans
                    onLog?.Invoke("Spinning up all controls to detect connected fans...");
                    foreach (var control in allControls)
                    {
                        try { control.setSpeed(100); } catch { }
                    }

                    // Wait 6 seconds for fans to spin up and RPM to register
                    for (int w = 0; w < 6; w++)
                    {
                        if (token.IsCancellationRequested) return;
                        Thread.Sleep(1000);
                    }

                    // Filter controls: STRICT RPM CHECK
                    var activeControls = new List<BaseControl>();
                    foreach (var control in allControls)
                    {
                        string controlDeviceName = GetHardwareDeviceName(control, hw.ControlList);
                        var deviceFans = allFans.Where(f => GetHardwareDeviceName(f, hw.FanList) == controlDeviceName).ToList();
                        
                        bool isActuallyActive = false;
                        
                        // Strict match by name or index
                        var matchingFan = deviceFans.FirstOrDefault(f => 
                            f.Name.Equals(control.Name, StringComparison.OrdinalIgnoreCase) || 
                            (ExtractIndex(f.Name) == ExtractIndex(control.Name) && !string.IsNullOrEmpty(ExtractIndex(f.Name)))
                        );

                        if (matchingFan != null)
                        {
                            isActuallyActive = (matchingFan.Value > 0);
                        }
                        else if (controlDeviceName.Contains("NVIDIA") || controlDeviceName.Contains("Kraken"))
                        {
                            // For complex devices, fallback to any spinning fan
                            isActuallyActive = deviceFans.Any(f => f.Value > 0);
                        }

                        if (isActuallyActive)
                        {
                            activeControls.Add(control);
                            onLog?.Invoke($"Including control: {control.Name} ({controlDeviceName})");
                        }
                        else
                        {
                            onLog?.Invoke($"Skipping control: {control.Name} (No associated RPM found on {controlDeviceName})");
                        }
                    }

                    if (tempList.Count == 0 || activeControls.Count == 0)
                    {
                        onLog?.Invoke("No sensors or active controls detected.");
                        return;
                    }

                    // NEW: Reset all active controls to AUTO before starting baseline
                    onLog?.Invoke("Forcing controls to AUTO mode...");
                    foreach (var control in activeControls)
                    {
                        try 
                        { 
                            control.IsSetSpeed = true; // Force it to attempt a reset
                            control.setAuto(); 
                        } 
                        catch { }
                    }
                    try { Task.Delay(5000, token).Wait(); } catch { }
                    if (token.IsCancellationRequested) return;

                    onLog?.Invoke($"Starting hyper-accurate test: {activeControls.Count} fans vs {tempList.Count} sensors.");

                    // Store original control values
                    var originalValues = new Dictionary<string, int>();
                    foreach (var control in activeControls) originalValues[control.ID] = control.Value;

                    int targetRPM = 0;
                    int total = activeControls.Count;
                    for (int i = 0; i < activeControls.Count; i++)
                    {
                        if (token.IsCancellationRequested) break;

                        var control = activeControls[i];
                        int originalValue = originalValues[control.ID];
                        var associatedFan = GetAssociatedFan(control, hw);
                        int currentPWM = MAX_PWM_DURING_TEST;

                        try
                        {
                            onLog?.Invoke($"[{i + 1}/{total}] Testing: {control.Name}");
                            
                            // 1. MEASURE LOCAL BASELINE AND NOISE
                            onLog?.Invoke("  Measuring thermal noise profile...");
                            var localBaseline = MeasureBaseline(tempList, 15, token);

                            // 2. APPLY TEST SPEED WITH DYNAMIC RPM NORMALIZATION
                            control.setSpeed(currentPWM);
                            onLog?.Invoke($"  Stabilizing... (Target RPM: {(targetRPM > 0 ? targetRPM.ToString() : "Max")})");
                            
                            int checkSteps = STABILIZATION_WAIT_SEC * 5; // 5 checks per second (0.2s interval)
                            for (int s = 0; s < checkSteps; s++)
                            {
                                if (token.IsCancellationRequested) break;
                                
                                // Active RPM correction every 200ms after 5 seconds of initial spin-up
                                if (targetRPM > 0 && associatedFan != null && s > 25)
                                {
                                    // Ensure we have the latest reading
                                    try { associatedFan.update(); } catch { }
                                    
                                    int currentRPM = associatedFan.Value;
                                    int minPWM = control.getMinSpeed();

                                    // Adjustment logic: faster but smaller steps (1-2%) for 0.2s frequency
                                    if (currentRPM > targetRPM + 30 && currentPWM > minPWM)
                                    {
                                        currentPWM -= (currentRPM > targetRPM + 200) ? 3 : 1;
                                        control.setSpeed(Math.Max(minPWM, currentPWM));
                                    }
                                    else if (currentRPM < targetRPM - 30 && currentPWM < 100)
                                    {
                                        currentPWM += (currentRPM < targetRPM - 200) ? 3 : 1;
                                        control.setSpeed(Math.Min(100, currentPWM));
                                    }
                                }
                                Thread.Sleep(200);
                            }

                            // If this is the first fan, record its RPM as target
                            if (i == 0 && associatedFan != null)
                            {
                                targetRPM = associatedFan.Value;
                                onLog?.Invoke($"  Global Target RPM set to {targetRPM}");
                            }

                            // 3. MEASURE TEST DATA
                            var testData = MeasureBaseline(tempList, 10, token);

                            // 4. CALCULATE DELTAS
                            foreach (var sensor in tempList)
                            {
                                var baseline = localBaseline[sensor.ID];
                                double testAvg = testData[sensor.ID].Average;
                                double delta = baseline.Average - testAvg;
                                
                                // MANDATORY STORE: every sensor gets every result
                                results.Add(new MappingResult()
                                {
                                    TempID = sensor.ID,
                                    TempName = GetFullDeviceName(sensor, hw.TempList),
                                    ControlID = control.ID,
                                    ControlName = GetFullDeviceName(control, hw.ControlList),
                                    BaseTemp = baseline.Average,
                                    TestTemp = testAvg,
                                    Delta = delta,
                                    RPM = associatedFan?.Value ?? 0,
                                    Confidence = 0 
                                });

                                double noiseThreshold = Math.Max(CONFIDENCE_THRESHOLD, baseline.StdDev * 2);
                                if (delta >= noiseThreshold)
                                {
                                    onLog?.Invoke($"    -> {sensor.Name}: cooling detected ({delta:F2}°C)");
                                }
                            }

                            // 5. RESTORE AND COOL DOWN
                            control.setSpeed(originalValue);
                            onLog?.Invoke("  Restoring and cooling down (15s)...");
                            try { Task.Delay(15000, token).Wait(); } catch { }
                            token.ThrowIfCancellationRequested();
                        }
                        catch (Exception ex)
                        {
                            if (ex is OperationCanceledException || (ex is AggregateException ae && ae.InnerException is TaskCanceledException))
                            {
                                onLog?.Invoke("Test canceled by user. Restoring control...");
                            }
                            else
                            {
                                onLog?.Invoke($"Error: {ex.Message}");
                            }
                            control.setSpeed(originalValue);
                        }

                        onProgress?.Invoke((int)((i + 1) * 100 / total));
                    }

                    var grouped = AggregateResults(results);
                    onLog?.Invoke($"Mapping complete. Found {grouped.Count} associations.");
                    onFinished?.Invoke(grouped);
                }
                finally
                {
                    onLog?.Invoke("Forcing controls to AUTO mode before exit...");
                    try
                    {
                        var hwFinal = new List<BaseControl>();
                        foreach (var control in hwFinal)
                        {
                            try 
                            { 
                                control.IsSetSpeed = true; 
                                control.setAuto(); 
                            } 
                            catch { }
                        }
                    }
                    catch { }

                    IsRunning = false;
                }
            }, token);
        }

        private void WaitForStability(List<BaseSensor> sensors, CancellationToken token)
        {
            // Method removed as requested
        }

        private Dictionary<string, BaselineData> MeasureBaseline(List<BaseSensor> sensors, int count, CancellationToken token)
        {
            var data = new Dictionary<string, List<double>>();
            foreach (var s in sensors) data[s.ID] = new List<double>();

            for (int i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();
                foreach (var s in sensors) data[s.ID].Add(s.Value);
                try { Task.Delay(SAMPLE_INTERVAL_MS, token).Wait(); } catch { }
            }

            var result = new Dictionary<string, BaselineData>();
            foreach (var kvp in data)
            {
                double avg = kvp.Value.Average();
                double sumOfSquares = kvp.Value.Sum(v => Math.Pow(v - avg, 2));
                double stdDev = Math.Sqrt(sumOfSquares / count);
                
                result[kvp.Key] = new BaselineData { Average = avg, StdDev = stdDev };
            }
            return result;
        }

        private List<MappingResult> AggregateResults(List<MappingResult> allResults)
        {
            // (Previous AggregateResults logic with Hero Logic)
            if (allResults.Count == 0) return new List<MappingResult>();

            var grouped = new List<MappingResult>();
            var byTemp = allResults.GroupBy(r => r.TempID);

            foreach (var group in byTemp)
            {
                var sensorResults = group.OrderByDescending(x => x.Delta).ToList();
                var best = sensorResults.First();
                double confidence = 100.0;
                
                if (sensorResults.Count > 1)
                {
                    var secondBest = sensorResults[1];
                    confidence = ((best.Delta - secondBest.Delta) / best.Delta) * 100.0;
                    if (best.Delta < 1.0) confidence *= (best.Delta / 1.0); // Penalize very small deltas
                }
                else
                {
                    confidence = Math.Min(100.0, (best.Delta / CONFIDENCE_THRESHOLD) * 50.0);
                }

                grouped.Add(new MappingResult()
                {
                    TempID = best.TempID,
                    TempName = best.TempName,
                    ControlID = best.ControlID,
                    ControlName = best.ControlName,
                    BaseTemp = best.BaseTemp,
                    TestTemp = best.TestTemp,
                    Delta = best.Delta,
                    RPM = best.RPM,
                    Confidence = Math.Max(10, Math.Min(100, confidence))
                });
                onLog?.Invoke($"Match: {best.TempName} handled by {best.ControlName} (Confidence: {confidence:F1}%)");
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

        private BaseSensor GetAssociatedFan(BaseControl control, HardwareManager hw)
        {
            string controlDeviceName = GetHardwareDeviceName(control, hw.ControlList);
            // In this project, Fan speeds are of type BaseSensor
            var deviceFans = hw.FanBaseList.Where(f => GetHardwareDeviceName(f, hw.FanList) == controlDeviceName).ToList();

            // Try strict match first by name or index
            var matchingFan = deviceFans.FirstOrDefault(f =>
                f.Name.Equals(control.Name, StringComparison.OrdinalIgnoreCase) ||
                (ExtractIndex(f.Name) == ExtractIndex(control.Name) && !string.IsNullOrEmpty(ExtractIndex(f.Name)))
            );

            // Fallback to first spinning fan sensor on device
            return matchingFan ?? deviceFans.FirstOrDefault(f => f.Value > 0);
        }

        public void Stop()
        {
            if (!IsRunning) return;
            mCts?.Cancel();
            IsRunning = false;
        }
    }
}
