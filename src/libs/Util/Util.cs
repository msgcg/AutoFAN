using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FanCtrl
{
    public class Util
    {
        public static void printHex(string hexString)
        {
            var dataArray = Util.getHexBytes(hexString);
            if (dataArray == null)
                return;
            Util.printHex(dataArray, dataArray.Length);
        }

        public static void printHex(byte[] dataArray)
        {
            if (dataArray == null)
                return;
            Util.printHex(dataArray, dataArray.Length);
        }

        public static void printHex(byte[] dataArray, int dataSize)
        {
            if (dataArray == null || dataArray.Length < dataSize)
                return;

            for (int i = 0; i < dataSize; i++)
            {
                Console.Write("{0:X2} ", dataArray[i]);
                if (i == 0)
                    continue;
                else if ((i + 1) % 16 == 0)
                {
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }        

        public static byte[] getHexBytes(string hexString)
        {
            try
            {
                int length = hexString.Length;
                var bytes = new byte[length / 2];
                for (int i = 0; i < length; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                }
                return bytes;
            }
            catch { }
            return null;
        }

        public static string getHexString(byte[] datas)
        {
            try
            {
                string hexString = string.Empty;
                hexString = string.Concat(Array.ConvertAll(datas, byt => byt.ToString("X2")));
                return hexString;
            }
            catch { }
            return "";
        }

        public static string getHexString(byte[] datas, int dataSize)
        {
            try
            {
                var array = new byte[dataSize];
                for (int i = 0; i < dataSize; i++)
                {
                    array[i] = datas[i];
                }
                return Util.getHexString(array);
            }
            catch { }
            return "";
        }

        public static bool isHex(char value)
        {
            if ((value >= 48 && value <= 57) ||
                (value >= 65 && value <= 70) ||
                (value >= 97 && value <= 102))
            {
                return true;
            }
            return false;
        }

        public static long getNowMS()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static int getFahrenheit(int celsius)
        {
            return (int)Math.Round(((double)celsius * 9 / 5) + 32);
        }

        public static int getCelsius(int fahrenheit)
        {
            return (int)Math.Round(((double)fahrenheit - 32) * 5 / 9);
        }

        public static void sleep(int ms)
        {
            try
            {
                Thread.Sleep(ms);
            }
            catch { }
        }

        public static void sleep(ref bool isEnd, int ms)
        {
            try
            {
                if (ms <= 0)
                    return;

                if (ms < 10)
                {
                    Thread.Sleep(ms);
                    return;
                }

                while (isEnd == true)
                {
                    ms = ms - 10;
                    if (ms == 0)
                    {
                        break;
                    }
                    else if (ms < 10)
                    {
                        Thread.Sleep(ms);
                        break;
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch { }
        }

        public static void setLanguage()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            }
            catch { }
        }

        public static void InstallPawnIO()
        {
            try
            {
                string path = ExtractPawnIO();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try
                    {
                        var startInfo = new ProcessStartInfo(path, "-install")
                        {
                            UseShellExecute = true
                        };
                        var process = Process.Start(startInfo);
                        process?.WaitForExit();
                    }
                    finally
                    {
                        try
                        {
                            if (File.Exists(path))
                                File.Delete(path);
                        }
                        catch { }
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось извлечь или найти установщик PawnIO (PawnIO_setup.exe).", "AutoFAN", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке PawnIO: {ex.Message}", "AutoFAN", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string ExtractPawnIO()
        {
            string destination = Path.Combine(Path.GetTempPath(), "PawnIO_setup.exe");
            try
            {
                Stream resourceStream = typeof(MainForm).Assembly.GetManifestResourceStream("FanCtrl.Resources.PawnIO_setup.exe");
                if (resourceStream != null)
                {
                    using (resourceStream)
                    using (FileStream fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                    return destination;
                }

                // Fallback: search on disk if not embedded
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] possiblePaths = new[]
                {
                    Path.Combine(appDir, "Resources", "PawnIO_setup.exe"),
                    Path.Combine(appDir, "PawnIO_setup.exe"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Resources", "PawnIO_setup.exe"),
                    Path.Combine(Directory.GetCurrentDirectory(), "PawnIO_setup.exe")
                };

                foreach (string candidate in possiblePaths)
                {
                    if (File.Exists(candidate))
                    {
                        File.Copy(candidate, destination, true);
                        return destination;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
