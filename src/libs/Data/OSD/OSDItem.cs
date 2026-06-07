using System.Drawing;
using System.Text;

namespace FanCtrl
{
    public class OSDItem
    {
        public OSDUnitType UnitType { get; set; }

        public string ID { get; set; }

        public bool IsColor { get; set; } = false;

        public Color Color { get; set; } = Color.White;

        public string getOSDString(int digit)
        {
            try
            {
                var osdString = new StringBuilder();

                // Space prefix
                osdString.Append(" ");

                // Value
                var hardwareManager = HardwareManager.getInstance();

                if (UnitType == OSDUnitType.FPS)
                {
                    osdString.Append("FPS");
                }
                else if (UnitType == OSDUnitType.Blank)
                {
                    osdString.Append(" ");
                }
                else
                {
                    var tempBaseMap = hardwareManager.TempBaseMap;
                    var fanBaseMap = hardwareManager.FanBaseMap;
                    var controlBaseMap = hardwareManager.ControlBaseMap;
                    var osdMap = hardwareManager.OSDSensorMap;
                    
                    if (tempBaseMap.ContainsKey(ID) == true)
                    {
                        var device = tempBaseMap[ID];
                        int value = device.Value;
                        value = (OptionManager.getInstance().IsFahrenheit == true) ? Util.getFahrenheit(value) : value;
                        osdString.Append(value.ToString());
                    }

                    else if (fanBaseMap.ContainsKey(ID) == true)
                    {
                        var device = fanBaseMap[ID];
                        int value = device.Value;
                        osdString.Append(value.ToString());
                    }

                    else if (controlBaseMap.ContainsKey(ID) == true)
                    {
                        var device = controlBaseMap[ID];
                        int value = device.Value;
                        osdString.Append(value.ToString());
                    }

                    else if (osdMap.ContainsKey(ID) == true)
                    {
                        var sensor = osdMap[ID];
                        osdString.Append(sensor.getString());
                    }

                    else
                    {
                        return "";
                    }
                }

                // Unit
                osdString.Append(this.getUnitString());

                return osdString.ToString();
            }
            catch { }
            return "";            
        }

        public OSDItem clone()
        {
            var item = new OSDItem();
            item.UnitType = this.UnitType;
            item.ID = this.ID;
            item.IsColor = this.IsColor;
            item.Color = Color.FromArgb(this.Color.R, this.Color.G, this.Color.B);
            return item;
        }

        public string getUnitString()
        {
            switch (UnitType)
            {
                case OSDUnitType.Temperature:
                    return (OptionManager.getInstance().IsFahrenheit == false) ? " °C" : " °F";

                case OSDUnitType.RPM:
                    return " RPM";

                case OSDUnitType.Percent:
                    return " %";

                case OSDUnitType.MHz:
                case OSDUnitType.kHz:
                    return " MHz";

                case OSDUnitType.KB:
                case OSDUnitType.GB:
                case OSDUnitType.MB:
                    return " MB";

                case OSDUnitType.MBPerSec:
                    return " MB/s";

                case OSDUnitType.Voltage:
                    return " V";

                case OSDUnitType.Power:
                    return " W";

                case OSDUnitType.FPS:
                    return " FPS";



                default:
                    return " ";
            }
        }
    }
}

