using System;
using System.Collections.Generic;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NvAPIWrapper;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native.GPU;
using FanCtrl.Resources;

namespace FanCtrl
{
    public class HardwareManager
    {
        public string mHardwareFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Hardware.json";

        // Singletone
        private HardwareManager() { }
        private static HardwareManager sManager = new HardwareManager();
        public static HardwareManager getInstance() { return sManager; }

        // Start state
        private bool mIsStart = false;

        // lock
        private object mLock = new object();

        // Mutex
        private Mutex mISABusMutex = null;
        private Mutex mPCIMutex = null;

        // LibreHardwareMonitor
        private LHM mLHM = null;
        // Temperature sensor
        public List<List<HardwareDevice>> TempList { get; } = new List<List<HardwareDevice>>();
        public List<BaseSensor> TempBaseList { get; } = new List<BaseSensor>();
        public Dictionary<string, BaseSensor> TempBaseMap { get; } = new Dictionary<string, BaseSensor>();

        // Fan
        public List<List<HardwareDevice>> FanList { get; } = new List<List<HardwareDevice>>();
        public List<BaseSensor> FanBaseList { get; } = new List<BaseSensor>();
        public Dictionary<string, BaseSensor> FanBaseMap { get; } = new Dictionary<string, BaseSensor>();

        // Control
        public List<List<HardwareDevice>> ControlList { get; } = new List<List<HardwareDevice>>();
        public List<BaseControl> ControlBaseList { get; } = new List<BaseControl>();
        public Dictionary<string, BaseControl> ControlBaseMap { get; } = new Dictionary<string, BaseControl>();

        // OSD sensor List
        public List<OSDSensor> OSDSensorList { get; } = new List<OSDSensor>();
        public Dictionary<string, OSDSensor> OSDSensorMap { get; } = new Dictionary<string, OSDSensor>();

        // next tick change value
        private List<int> mChangeValueList = new List<int>();
        private List<BaseControl> mChangeControlList = new List<BaseControl>();

        // manual, auto control calc value
        private Dictionary<string, BaseControl> mManualControlDictionary = new Dictionary<string, BaseControl>();
        private Dictionary<string, BaseControl> mAutoControlDictionary = new Dictionary<string, BaseControl>();

        // update timer
        private System.Timers.Timer mUpdateTimer = null;

        public event UpdateTimerEventHandler onUpdateCallback;
        public delegate void UpdateTimerEventHandler();        

#if MY_DEBUG
        private int mDebugUpdateCount = 10;
#endif

        public void start()
        {
            Monitor.Enter(mLock);
            if (mIsStart == true)
            {
                Monitor.Exit(mLock);
                return;
            }
            mIsStart = true;

            string mutexName = "Global\\Access_ISABUS.HTP.Method";
            this.createBusMutex(mutexName, ref mISABusMutex);

            mutexName = "Global\\Access_PCI";
            this.createBusMutex(mutexName, ref mPCIMutex);

            // create list
            for (int i = 0; i < (int)LIBRARY_TYPE.MAX; i++)
            {
                TempList.Add(new List<HardwareDevice>());
                FanList.Add(new List<HardwareDevice>());
                ControlList.Add(new List<HardwareDevice>());
            }

            // LHM
            if (OptionManager.getInstance().IsLHM == true)
            {
                mLHM = new LHM();
                mLHM.start();

                var tempList = TempList[(int)LIBRARY_TYPE.LHM];
                mLHM.createTemp(ref tempList);

                var fanList = FanList[(int)LIBRARY_TYPE.LHM];
                mLHM.createFan(ref fanList);

                var controlList = ControlList[(int)LIBRARY_TYPE.LHM];
                mLHM.createControl(ref controlList);
            }

            // NvAPIWrapper
            if(OptionManager.getInstance().IsNvAPIWrapper == true)
            {
                this.lockBus();
                try
                {
                    NVIDIA.Initialize();
                }
                catch { }

                try
                {
                    var gpuArray = PhysicalGPU.GetPhysicalGPUs();
                    for (int i = 0; i < gpuArray.Length; i++)
                    {
                        var gpu = gpuArray[i];
                        var hardwareName = gpu.FullName;

                        // temperature
                        var id = string.Format("NvAPIWrapper/{0}/{1}/Temp", hardwareName, gpu.GPUId);
                        var name = "GPU Core";
                        var temp = new NvAPITemp(id, name, i, NvAPITemp.TEMPERATURE_TYPE.CORE);
                        temp.LockBus += lockBus;
                        temp.UnlockBus += unlockBus;

                        var tempDevice = new HardwareDevice(hardwareName);
                        tempDevice.addDevice(temp);

                        if (gpu.ThermalInformation.HasAnyThermalSensor == true)
                        {
                            if (gpu.ThermalInformation.HotSpotTemperature != 0)
                            {
                                id = string.Format("NvAPIWrapper/{0}/{1}/HotSpotTemp", hardwareName, gpu.GPUId);
                                name = "GPU Hot Spot";
                                temp = new NvAPITemp(id, name, i, NvAPITemp.TEMPERATURE_TYPE.HOTSPOT);
                                temp.LockBus += lockBus;
                                temp.UnlockBus += unlockBus;
                                tempDevice.addDevice(temp);
                            }

                            if (gpu.ThermalInformation.MemoryJunctionTemperature != 0)
                            {
                                id = string.Format("NvAPIWrapper/{0}/{1}/MemoryJunctionTemp", hardwareName, gpu.GPUId);
                                name = "GPU Memory Junction";
                                temp = new NvAPITemp(id, name, i, NvAPITemp.TEMPERATURE_TYPE.MEMORY);
                                temp.LockBus += lockBus;
                                temp.UnlockBus += unlockBus;
                                tempDevice.addDevice(temp);
                            }
                        }

                        var tempList = TempList[(int)LIBRARY_TYPE.NvAPIWrapper];
                        tempList.Add(tempDevice);

                        var fanDevice = new HardwareDevice(hardwareName);
                        var controlDevice = new HardwareDevice(hardwareName);

                        int num = 1;
                        var e = gpuArray[i].CoolerInformation.Coolers.GetEnumerator();
                        while (e.MoveNext())
                        {
                            var value = e.Current;
                            int coolerID = value.CoolerId;
                            int speed = value.CurrentLevel;
                            int minSpeed = value.DefaultMinimumLevel;
                            int maxSpeed = value.DefaultMaximumLevel;
                            CoolerPolicy policy = value.DefaultPolicy;

                            // fan
                            id = string.Format("NvAPIWrapper/{0}/{1}/Fan/{2}", hardwareName, gpu.GPUId, coolerID);
                            name = "GPU Fan #" + num;
                            var fan = new NvAPIFanSpeed(id, name, i, coolerID);
                            fan.LockBus += lockBus;
                            fan.UnlockBus += unlockBus;
                            fanDevice.addDevice(fan);

                            // control
                            id = string.Format("NvAPIWrapper/{0}/{1}/Control/{2}", hardwareName, gpu.GPUId, coolerID);
                            name = "GPU Fan #" + num;
                            var control = new NvAPIFanControl(id, name, i, coolerID, speed, minSpeed, maxSpeed, policy);
                            control.LockBus += lockBus;
                            control.UnlockBus += unlockBus;
                            controlDevice.addDevice(control);
                            num++;
                        }

                        if (fanDevice.DeviceList.Count > 0)
                        {
                            var fanList = FanList[(int)LIBRARY_TYPE.NvAPIWrapper];
                            fanList.Add(fanDevice);
                        }

                        if (controlDevice.DeviceList.Count > 0)
                        {
                            var controlList = ControlList[(int)LIBRARY_TYPE.NvAPIWrapper];
                            controlList.Add(controlDevice);
                        }
                    }
                }
                catch { }
                this.unlockBus();
            }

            // NZXT Kraken            }

            for (int i = 0; i < TempList.Count; i++)
            {
                var deviceList = TempList[i];
                for (int j = 0; j < deviceList.Count; j++)
                {
                    var device = deviceList[j];
                    for (int k = 0; k < device.DeviceList.Count; k++)
                    {
                        var temp = device.DeviceList[k];
                        TempBaseList.Add((BaseSensor)temp);
                        TempBaseMap.Add(temp.ID, (BaseSensor)temp);
                    }
                }
            }
            for (int i = 0; i < FanList.Count; i++)
            {
                var deviceList = FanList[i];
                for (int j = 0; j < deviceList.Count; j++)
                {
                    var device = deviceList[j];
                    for (int k = 0; k < device.DeviceList.Count; k++)
                    {
                        var fan = device.DeviceList[k];
                        FanBaseList.Add((BaseSensor)fan);
                        FanBaseMap.Add(fan.ID, (BaseSensor)fan);
                    }
                }
            }
            for (int i = 0; i < ControlList.Count; i++)
            {
                var deviceList = ControlList[i];
                for (int j = 0; j < deviceList.Count; j++)
                {
                    var device = deviceList[j];
                    for (int k = 0; k < device.DeviceList.Count; k++)
                    {
                        var control = device.DeviceList[k];
                        ControlBaseList.Add((BaseControl)control);
                        ControlBaseMap.Add(control.ID, (BaseControl)control);
                    }
                }
            }

            // osd sensor
            this.createOSDSensor();

            Monitor.Exit(mLock);
        }

        public void stop()
        {
            Monitor.Enter(mLock);
            if (mIsStart == false)
            {
                Monitor.Exit(mLock);
                return;
            }
            mIsStart = false;

            if (mUpdateTimer != null)
            {
                mUpdateTimer.Stop();
                mUpdateTimer.Dispose();
                mUpdateTimer = null;
            }

            // restore fan control
            for (int i = 0; i < ControlBaseList.Count; i++)
            {
                var control = ControlBaseList[i];
                control.stopTimer();
                control.setAuto();
            }

            if (mLHM != null)
            {
                mLHM.stop();
                mLHM = null;
            }
            if (OptionManager.getInstance().IsNvAPIWrapper == true)
            {
                try
                {
                    NVIDIA.Unload();
                }
                catch { }
            }
            mChangeControlList.Clear();
            mChangeValueList.Clear();

            TempList.Clear();
            FanList.Clear();
            ControlList.Clear();

            TempBaseList.Clear();
            FanBaseList.Clear();
            ControlBaseList.Clear();

            TempBaseMap.Clear();
            FanBaseMap.Clear();
            ControlBaseMap.Clear();

            OSDSensorList.Clear();
            OSDSensorMap.Clear();

            if (mISABusMutex != null)
            {
                mISABusMutex.Close();
                mISABusMutex = null;
            }

            if (mPCIMutex != null)
            {
                mPCIMutex.Close();
                mPCIMutex = null;
            }

            OSDController.release();
            WinUSBController.exit();         

            Monitor.Exit(mLock);
        }

        public void startUpdate()
        {
            Monitor.Enter(mLock);
            if (mIsStart == false)
            {
                Monitor.Exit(mLock);
                return;
            }

            mUpdateTimer = new System.Timers.Timer();
            mUpdateTimer.Interval = OptionManager.getInstance().Interval;
            mUpdateTimer.Elapsed += onUpdateTimer;
            mUpdateTimer.Start();

            Monitor.Exit(mLock);
        }

        public void restartTimer()
        {
            Monitor.Enter(mLock);
            if (mIsStart == false)
            {
                Monitor.Exit(mLock);
                return;
            }

            if (mUpdateTimer != null)
            {
                mUpdateTimer.Stop();
                mUpdateTimer.Dispose();
                mUpdateTimer = null;
            }

            mUpdateTimer = new System.Timers.Timer();
            mUpdateTimer.Interval = OptionManager.getInstance().Interval;
            mUpdateTimer.Elapsed += onUpdateTimer;
            mUpdateTimer.Start();
            Monitor.Exit(mLock);
        }

        private void createBusMutex(string mutexName, ref Mutex mutex)
        {
            try
            {
                // ? Простой конструктор для .NET 8
                mutex = new Mutex(false, mutexName, out _);
            }
            catch (UnauthorizedAccessException)
            {
                try
                {
                    // ? OpenExisting с одним аргументом
                    mutex = Mutex.OpenExisting(mutexName);
                }
                catch { }
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // Мьютекс ещё не создан — это нормально при первом запуске
                try
                {
                    mutex = new Mutex(false, mutexName, out _);
                }
                catch { }
            }
        }

        private void lockBus()
        {
            try
            {
                mISABusMutex.WaitOne();
            }
            catch { }
            try
            {
                mPCIMutex.WaitOne();
            }
            catch { }            
        }

        private void unlockBus()
        {
            try
            {
                mISABusMutex.ReleaseMutex();
            }
            catch { }
            try
            {
                mPCIMutex.ReleaseMutex();
            }
            catch { }
        }
                
        private void createOSDSensor()
        {
            // Temp
            for (int i = 0; i < TempBaseList.Count; i++)
            {
                var device = TempBaseList[i];
                string id = device.ID;
                string prefix = "[" + StringLib.Temperature + "] ";
                string name = device.Name;
                var osdSensor = new OSDSensor(id, prefix, name, OSDUnitType.Temperature);
                OSDSensorList.Add(osdSensor);
                OSDSensorMap.Add(id, osdSensor);
            }

            // Fan
            for (int i = 0; i < FanBaseList.Count; i++)
            {
                var device = FanBaseList[i];
                string id = device.ID;
                string prefix = "[" + StringLib.Fan_speed + "] ";
                string name = device.Name;
                var osdSensor = new OSDSensor(id, prefix, name, OSDUnitType.RPM);
                OSDSensorList.Add(osdSensor);
                OSDSensorMap.Add(id, osdSensor);
            }

            // Control
            for (int i = 0; i < ControlBaseList.Count; i++)
            {
                var device = ControlBaseList[i];
                string id = device.ID;
                string prefix = "[" + StringLib.Fan_control + "] ";
                string name = device.Name;
                var osdSensor = new OSDSensor(id, prefix, name, OSDUnitType.Percent);
                OSDSensorList.Add(osdSensor);
                OSDSensorMap.Add(id, osdSensor);
            }

            // Framerate
            string id2 = "OSD/Framerate";
            string prefix2 = "[" + StringLib.ETC + "] ";
            string name2 = "Framerate";
            var osdSensor2 = new OSDSensor(id2, prefix2, name2,  OSDUnitType.FPS);
            OSDSensorList.Add(osdSensor2);
            OSDSensorMap.Add(id2, osdSensor2);

            // Blank
            id2 = "OSD/Blank";
            name2 = "Blank";
            osdSensor2 = new OSDSensor(id2, prefix2, name2, OSDUnitType.Blank);
            OSDSensorList.Add(osdSensor2);
            OSDSensorMap.Add(id2, osdSensor2);

            //////////////// other sensor ////////////////
            // LHM
            if (OptionManager.getInstance().IsLHM == true && mLHM != null)
            {
                mLHM.createOSDSensor(OSDSensorList, OSDSensorMap);
            }

            // NvAPIWrapper
            if (OptionManager.getInstance().IsNvAPIWrapper == true)
            {
                this.lockBus();
                try
                {
                    string idPrefix = "NvAPIWrapper/OSD";
                    var gpuArray = PhysicalGPU.GetPhysicalGPUs();
                    for (int i = 0; i < gpuArray.Length; i++)
                    {
                        int subIndex = 0;

                        string id = string.Format("{0}/{1}/{2}", idPrefix, gpuArray[i].GPUId, subIndex);
                        string prefix = "[Clock] ";
                        string name = "GPU Graphics";
                        var osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.kHz, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Clock] ";
                        name = "GPU Memory";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.kHz, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Clock] ";
                        name = "GPU Processor";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.kHz, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Clock] ";
                        name = "GPU Video Decoding";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.kHz, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Load] ";
                        name = "GPU Core";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.Percent, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Load] ";
                        name = "GPU Frame Buffer";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.Percent, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Load] ";
                        name = "GPU Video Engine";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.Percent, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Load] ";
                        name = "GPU Bus Interface";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.Percent, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Load] ";
                        name = "GPU Memory";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.Percent, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Data] ";
                        name = "GPU Memory Free";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.KB, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Data] ";
                        name = "GPU Memory Used";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.KB, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);

                        id = string.Format("{0}/{1}/{2}", idPrefix, i, subIndex);
                        prefix = "[Data] ";
                        name = "GPU Memory Total";
                        osdSensor = new NvAPIOSDSensor(id, prefix, name, OSDUnitType.KB, i, subIndex++);
                        osdSensor.LockBus += lockBus;
                        osdSensor.UnlockBus += unlockBus;
                        OSDSensorList.Add(osdSensor);
                        OSDSensorMap.Add(id, osdSensor);
                    }
                }
                catch { }
                this.unlockBus();
            }        }

        private void onUpdateTimer(object sender, EventArgs e)
        {
            if (Monitor.TryEnter(mLock) == false)
                return;

#if MY_DEBUG
            if (ControlManager.getInstance().ModeType == MODE_TYPE.PERFORMANCE)
            {
                if (mDebugUpdateCount <= 0)
                {
                    Monitor.Exit(mLock);
                    return;
                }
                mDebugUpdateCount--;
            }
            else
            {
                mDebugUpdateCount = 10;
            }
#endif

            try
            {
                if (mLHM != null)
                {
                    mLHM.update();
                }
            }
            catch { }

            try
            {
                for (int i = 0; i < (int)LIBRARY_TYPE.MAX; i++)
                {
                    var tempList = TempList[i];
                    for (int j = 0; j < tempList.Count; j++)
                    {
                        var deviceList = tempList[j].DeviceList;
                        for (int k = 0; k < deviceList.Count; k++)
                        {
                            var temp = deviceList[k];
                            temp.update();
                        }
                    }

                    var fanList = FanList[i];
                    for (int j = 0; j < fanList.Count; j++)
                    {
                        var deviceList = fanList[j].DeviceList;
                        for (int k = 0; k < deviceList.Count; k++)
                        {
                            var fan = deviceList[k];
                            fan.update();
                        }
                    }

                    var controlList = ControlList[i];
                    for (int j = 0; j < controlList.Count; j++)
                    {
                        var deviceList = controlList[j].DeviceList;
                        for (int k = 0; k < deviceList.Count; k++)
                        {
                            var control = deviceList[k];
                            control.update();
                        }
                    }
                }
            }
            catch { }

            try
            {
                // change value
                bool isExistChange = false;
                if (mChangeValueList.Count > 0)
                {
                    for (int i = 0; i < mChangeControlList.Count; i++)
                    {
                        isExistChange = true;
                        mChangeControlList[i].setSpeed(mChangeValueList[i]);
                    }
                    mChangeControlList.Clear();
                    mChangeValueList.Clear();
                }

                // Control
                mManualControlDictionary.Clear();
                mAutoControlDictionary.Clear();
                var controlManager = ControlManager.getInstance();
                if (controlManager.IsEnable == true && isExistChange == false)
                {
                    var controlDataList = controlManager.getControlDataList(controlManager.ModeType);
                    for (int i = 0; i < controlDataList.Count; i++)
                    {
                        var controlData = controlDataList[i];
                        if (controlData == null)
                            break;

                        string tempID = controlData.ID;
                        if (TempBaseMap.ContainsKey(tempID) == false)
                            continue;

                        var tempDevice = TempBaseMap[tempID];
                        int temperature = tempDevice.Value;

                        for (int j = 0; j < controlData.FanDataList.Count; j++)
                        {
                            var fanData = controlData.FanDataList[j];

                            string fanID = fanData.ID;
                            if (ControlBaseMap.ContainsKey(fanID) == false)
                                continue;

                            var controlDevice = ControlBaseMap[fanID];

                            bool isAuto = false;
                            int percent = fanData.getValue(temperature, ref isAuto);
                            int delayTime = fanData.DelayTime;

                            // auto mode
                            if (isAuto == true)
                            {
                                mAutoControlDictionary.Add(fanID, controlDevice);
                            }

                            // manual mode
                            else
                            {
                                // remove auto mode control
                                mAutoControlDictionary.Remove(fanID);

                                if (mManualControlDictionary.ContainsKey(fanID) == false)
                                {
                                    mManualControlDictionary.Add(fanID, controlDevice);
                                    controlDevice.NextValue = percent;
                                    controlDevice.Timeout = delayTime;
                                }
                                else
                                {
                                    controlDevice.NextValue = (controlDevice.NextValue >= percent) ? controlDevice.NextValue : percent;
                                    controlDevice.Timeout = delayTime;
                                }
                            }
                        }
                    }

                    foreach (var keyPair in mManualControlDictionary)
                    {
                        var control = keyPair.Value;

                        // remove auto mode control
                        mAutoControlDictionary.Remove(control.ID);

                        //Console.WriteLine("manual mode : name({0}), value({1}), nextvalue({2})", control.Name, control.Value, control.NextValue);

                        if (control.Value == control.NextValue)
                        {
                            control.checkTimer();
                            continue;
                        }

                        control.setSpeedWithTimer(control.NextValue);
                    }

                    foreach (var keyPair in mAutoControlDictionary)
                    {
                        var control = keyPair.Value;
                        //Console.WriteLine("auto mode : name({0})", control.Name);
                        control.stopTimer();
                        control.setAuto();
                    }
                }
            }
            catch { }            

            // onUpdateCallback
            onUpdateCallback?.Invoke();

            var osdManager = OSDManager.getInstance();
            if (osdManager.IsEnable == true)
            {
                

                var osdString = new StringBuilder();
                if (osdManager.IsTime == true)
                {
                    osdString.Append(DateTime.Now.ToString("HH:mm:ss") + "\n");
                }

                int maxNameLength = 0;
                for (int i = 0; i < osdManager.getGroupCount(); i++)
                {
                    var group = osdManager.getGroup(i);
                    if (group == null)
                        break;
                    if (group.Name.Length > maxNameLength)
                        maxNameLength = group.Name.Length;
                }

                var osdElements = new System.Collections.Generic.List<FanCtrl.OSDTextElement>();
                for (int i = 0; i < osdManager.getGroupCount(); i++)
                {
                    var group = osdManager.getGroup(i);
                    if (group == null)
                        break;
                    osdElements.AddRange(group.getOSDElements(maxNameLength));
                }

                if (osdElements.Count > 0)
                {
                    OSDController.update(osdElements);
                    osdManager.IsUpdate = true;
                }
                else if (osdManager.IsUpdate == true)
                {
                    OSDController.release();
                    osdManager.IsUpdate = false;
                }
            }
            else
            {
                if (osdManager.IsUpdate == true)
                {
                    OSDController.release();
                    osdManager.IsUpdate = false;
                }
            }
            Monitor.Exit(mLock);
        }

        public int addChangeValue(int value, BaseControl control, bool isLock = true)
        {
            if (isLock == true)
            {
                Monitor.Enter(mLock);
            }            
            if (value < control.getMinSpeed())
            {
                value = control.getMinSpeed();
            }
            else if(value > control.getMaxSpeed())
            {
                value = control.getMaxSpeed();
            }
            mChangeValueList.Add(value);
            mChangeControlList.Add(control);

            if (isLock == true)
            {
                Monitor.Exit(mLock);
            }
            return value;
        }

        public bool read(ref bool isDifferent)
        {
            Monitor.Enter(mLock);
            string jsonString;
            try
            {
                jsonString = File.ReadAllText(mHardwareFileName);
            }
            catch
            {
                Monitor.Exit(mLock);
                this.write();
                return false;
            }

            try
            {
                var rootObject = JObject.Parse(jsonString);

                // name
                if (rootObject.ContainsKey("name") == true)
                {
                    var nameObject = rootObject.Value<JObject>("name");

                    // temperature name
                    if (nameObject.ContainsKey("temp") == true)
                    {
                        var list = nameObject.Value<JArray>("temp");
                        for (int i = 0; i < list.Count; i++)
                        {
                            var jobject = list[i];
                            string id = jobject.Value<string>("id");
                            string name = jobject.Value<string>("name");

                            if (TempBaseMap.ContainsKey(id) == false)
                            {
                                isDifferent = true;
                                continue;
                            }

                            var device = TempBaseMap[id];
                            device.Name = name;

                            if (OSDSensorMap.ContainsKey(id) == true)
                            {
                                var sensor = OSDSensorMap[id];
                                sensor.Name = name;
                            }
                        }
                    }

                    // fan name
                    if (nameObject.ContainsKey("fan") == true)
                    {
                        var list = nameObject.Value<JArray>("fan");
                        for (int i = 0; i < list.Count; i++)
                        {
                            var jobject = list[i];
                            string id = jobject.Value<string>("id");
                            string name = jobject.Value<string>("name");

                            if (FanBaseMap.ContainsKey(id) == false)
                            {
                                isDifferent = true;
                                continue;
                            }

                            var device = FanBaseMap[id];
                            device.Name = name;

                            if (OSDSensorMap.ContainsKey(id) == true)
                            {
                                var sensor = OSDSensorMap[id];
                                sensor.Name = name;
                            }
                        }
                    }

                    // control name
                    if (nameObject.ContainsKey("control") == true)
                    {
                        var list = nameObject.Value<JArray>("control");
                        for (int i = 0; i < list.Count; i++)
                        {
                            var jobject = list[i];
                            string id = jobject.Value<string>("id");
                            string name = jobject.Value<string>("name");

                            if (ControlBaseMap.ContainsKey(id) == false)
                            {
                                isDifferent = true;
                                continue;
                            }

                            var device = ControlBaseMap[id];
                            device.Name = name;

                            if (OSDSensorMap.ContainsKey(id) == true)
                            {
                                var sensor = OSDSensorMap[id];
                                sensor.Name = name;
                            }
                        }
                    }
                }
            }
            catch
            {
                Monitor.Exit(mLock);
                return false;
            }
            Monitor.Exit(mLock);
            return true;
        }

        public void write()
        {
            Monitor.Enter(mLock);
            try
            {
                var rootObject = new JObject();

                // name
                var nameObject = new JObject();

                // temp name
                var tempList = new JArray();
                for (int i = 0; i < TempBaseList.Count; i++)
                {
                    var device = TempBaseList[i];
                    var jobject = new JObject();
                    jobject["id"] = device.ID;
                    jobject["name"] = device.Name;
                    tempList.Add(jobject);
                }
                nameObject["temp"] = tempList;

                // fan name
                var fanList = new JArray();
                for (int i = 0; i < FanBaseList.Count; i++)
                {
                    var device = FanBaseList[i];
                    var jobject = new JObject();
                    jobject["id"] = device.ID;
                    jobject["name"] = device.Name;
                    fanList.Add(jobject);
                }
                nameObject["fan"] = fanList;

                // control name
                var controlList = new JArray();
                for (int i = 0; i < ControlBaseList.Count; i++)
                {
                    var device = ControlBaseList[i];
                    var jobject = new JObject();
                    jobject["id"] = device.ID;
                    jobject["name"] = device.Name;
                    controlList.Add(jobject);
                }
                nameObject["control"] = controlList;

                rootObject["name"] = nameObject;

                File.WriteAllText(mHardwareFileName, rootObject.ToString());
            }
            catch { }
            Monitor.Exit(mLock);
        }
    }
}











