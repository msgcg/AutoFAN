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
        public string ControlID { get; set; }
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
        private const int MAX_PWM_DURING_TEST = 70;  // Safety limit for test: max 70%
        private const int TEST_PWM_DELTA = 25;       // Increment PWM by 25% for test
        private const int STABILIZATION_SAMPLES = 10; // Number of samples to average
        private const int SAMPLE_INTERVAL_MS = 300;   // Wait between samples (300ms)
        private const double CONFIDENCE_THRESHOLD = 1.0; // Minimum delta to consider valid

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

                    // snapshot controls and sensors
                    var tempList = hw.TempBaseList.ToList();
                    var controlList = hw.ControlBaseList.ToList();

                    if (tempList.Count == 0 || controlList.Count == 0)
                    {
                        onLog?.Invoke("No sensors or controls detected.");
                        return;
                    }

                    onLog?.Invoke($"Found {controlList.Count} controls and {tempList.Count} temperature sensors.");

                    // Store original control values
                    var originalValues = new Dictionary<string, int>();
                    foreach (var control in controlList)
                    {
                        originalValues[control.ID] = control.Value;
                    }

                    // Establish baseline: sample temps multiple times with controls at original speed
                    onLog?.Invoke("Establishing baseline temperatures...");
                    var baselineTemps = EstablishBaseline(tempList, 5); // 5 samples
                    if (baselineTemps == null)
                    {
                        onLog?.Invoke("Failed to establish baseline.");
                        return;
                    }

                    int total = controlList.Count;
                    for (int i = 0; i < controlList.Count; i++)
                    {
                        if (token.IsCancellationRequested) break;

                        var control = controlList[i];
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

                                    if (delta >= CONFIDENCE_THRESHOLD)
                                    {
                                        results.Add(new MappingResult()
                                        {
                                            TempID = tempID,
                                            ControlID = control.ID,
                                            Delta = delta,
                                            Confidence = 0 // Will be calculated after aggregation
                                        });
                                        onLog?.Invoke($"    sensor {tempID}: avg delta={delta:F2}°C");
                                    }
                                }

                                Thread.Sleep(1000); // Cool-down between levels
                            }

                            // Restore original control value
                            try { control.setSpeedWithTimer(original); }
                            catch { try { control.setSpeed(original); } catch { } }

                            Thread.Sleep(2000); // Wait for temps to return to baseline before next control test
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
                    ControlID = best.ControlID,
                    Delta = best.Delta,
                    Confidence = confidence
                });
            }

            return grouped.OrderBy(r => r.Confidence).Reverse().ToList();
        }

        public void Stop()
        {
            if (!IsRunning) return;
            mCts?.Cancel();
            IsRunning = false;
        }
    }
}
