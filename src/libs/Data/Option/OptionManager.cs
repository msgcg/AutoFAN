using System.IO;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Win32;

namespace FanCtrl
{
    public class OptionManager
    {
        private string mOptionFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Option.json";

        private static OptionManager sManager = new OptionManager();
        public static OptionManager getInstance() { return sManager; }

        private StartupControl mStartupControl = new StartupControl();

        private OptionManager()
        {
            this.reset();
            if (read() == false)
            {
                write();
            }
        }

        // LibreHardwareMonitor
        public bool IsLHM { get; set; }
        public bool IsLHMCpu { get; set; }
        public bool IsLHMMotherboard { get; set; }
        public bool IsLHMGpu { get; set; }
        public bool IsLHMContolled { get; set; }
        public bool IsLHMStorage { get; set; }
        public bool IsLHMMemory { get; set; }

        // NvApiWrapper
        public bool IsNvAPIWrapper { get; set; }

        // NZXT Kraken X2, X3

        // EVGA CLC

        // NZXT Fan&Contoller

        // HWiNFO

        // liquidctl

        // Plugin
        public int Interval { get; set; } = 1000;

        // Other options


        public THEME_TYPE Theme { get; set; }

        public bool IsAnimation { get; set; }

        public bool IsFahrenheit { get; set; }

        public bool IsMinimized { get; set; }

        public int DelayTime
        {
            get
            {
                return mStartupControl.DelayTime;
            }
            set
            {

                mStartupControl.DelayTime = value;
            }
        }

        public bool IsStartUp
        {
            get
            {
                return mStartupControl.Startup;
            }
            set
            {

                mStartupControl.Startup = value;
            }
        }

        public void reset()
        {

            IsLHM = true;
            IsLHMCpu = true;
            IsLHMMotherboard = true;
            IsLHMGpu = true;
            IsLHMContolled = true;
            IsLHMStorage = true;
            IsLHMMemory = true;

            IsNvAPIWrapper = false;

            Theme = 0;
            IsAnimation = true;
            IsFahrenheit = false;
            IsMinimized = false;
        }

        public bool read()
        {
            try 
            {
                var jsonString = File.ReadAllText(mOptionFileName);                
                var rootObject = JObject.Parse(jsonString);

                IsLHM = (rootObject.ContainsKey("IsLHM") == true) ? rootObject.Value<bool>("IsLHM") : true;
                IsLHMCpu = (rootObject.ContainsKey("IsLHMCpu") == true) ? rootObject.Value<bool>("IsLHMCpu") : true;
                IsLHMMotherboard = (rootObject.ContainsKey("IsLHMMotherboard") == true) ? rootObject.Value<bool>("IsLHMMotherboard") : true;
                IsLHMGpu = (rootObject.ContainsKey("IsLHMGpu") == true) ? rootObject.Value<bool>("IsLHMGpu") : true;
                IsLHMContolled = (rootObject.ContainsKey("IsLHMContolled") == true) ? rootObject.Value<bool>("IsLHMContolled") : true;
                IsLHMStorage = (rootObject.ContainsKey("IsLHMStorage") == true) ? rootObject.Value<bool>("IsLHMStorage") : true;
                IsLHMMemory = (rootObject.ContainsKey("IsLHMMemory") == true) ? rootObject.Value<bool>("IsLHMMemory") : true;

                IsNvAPIWrapper = (rootObject.ContainsKey("IsNvAPIWrapper") == true) ? rootObject.Value<bool>("IsNvAPIWrapper") : false;

                Theme = (rootObject.ContainsKey("Theme") == true) ? (THEME_TYPE)rootObject.Value<int>("Theme") : THEME_TYPE.SYSTEM;
                IsAnimation = (rootObject.ContainsKey("IsAnimation") == true) ? rootObject.Value<bool>("IsAnimation") : true;
                IsFahrenheit = (rootObject.ContainsKey("IsFahrenheit") == true) ? rootObject.Value<bool>("IsFahrenheit") : false;
                IsMinimized = (rootObject.ContainsKey("IsMinimized") == true) ? rootObject.Value<bool>("IsMinimized") : false;

                DelayTime = (rootObject.ContainsKey("DelayTime") == true) ? rootObject.Value<int>("DelayTime") : 0;
                Interval = (rootObject.ContainsKey("Interval") == true) ? rootObject.Value<int>("Interval") : 1000;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void write()
        {
            try
            {
                var rootObject = new JObject();
                
                rootObject["IsLHM"] = IsLHM;
                rootObject["IsLHMCpu"] = IsLHMCpu;
                rootObject["IsLHMMotherboard"] = IsLHMMotherboard;
                rootObject["IsLHMGpu"] = IsLHMGpu;
                rootObject["IsLHMContolled"] = IsLHMContolled;
                rootObject["IsLHMStorage"] = IsLHMStorage;
                rootObject["IsLHMMemory"] = IsLHMMemory;

                rootObject["IsNvAPIWrapper"] = IsNvAPIWrapper;

                rootObject["Theme"] = (int)Theme;
                rootObject["IsAnimation"] = IsAnimation;
                rootObject["IsFahrenheit"] = IsFahrenheit;
                rootObject["IsMinimized"] = IsMinimized;

                rootObject["DelayTime"] = DelayTime;
            rootObject["Interval"] = Interval;

                File.WriteAllText(mOptionFileName, rootObject.ToString());
            }
            catch {}
        }



        public THEME_TYPE getNowTheme()
        {
            if (this.Theme == THEME_TYPE.SYSTEM)
            {
                var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
                var registryValueObject = key?.GetValue("AppsUseLightTheme");
                if (registryValueObject == null)
                {
                    return THEME_TYPE.LIGHT;
                }
                var registryValue = (int)registryValueObject;
                return (registryValue > 0) ? THEME_TYPE.LIGHT : THEME_TYPE.DARK;
            }
            return this.Theme;
        }
    }
}






