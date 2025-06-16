using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TerminalConf
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string room = null;
            string index = null;
            string comPort = null;
            string mqttBroker = null;

            for (int i = 0; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "-r":
                        room = args[++i];
                        break;
                    case "-i":
                        index = args[++i];
                        break;
                    case "-l":
                        comPort = args[++i];
                        break;
                    case "-f":
                        mqttBroker = args[++i];
                        break;
                }
            }

            if (room == null && index == null && comPort == null && mqttBroker == null)
            {
                PrintUsage();
                return;
            }

            // Bước 1: Xác định các khả năng đường dẫn
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string[] possiblePaths = new string[]
            {
                @"C:\Windows\System32\config\systemprofile\AppData\Local\NewWindowsService",
                Path.Combine(localAppData, "NewWindowsService"),
                @"C:\Windows\System32\config\systemprofile\AppData\Roaming\NewWindowsService",
                Path.Combine(roamingAppData, "NewWindowsService")
            };

            string finalConfigPath = possiblePaths.FirstOrDefault(Directory.Exists);
            if (string.IsNullOrEmpty(finalConfigPath))
            {
                Console.WriteLine("Không tìm thấy thư mục chứa NewWindowsService.");
                return;
            }

            // Bước 2: Tìm file user.config
            string[] subDirs = Directory.GetDirectories(finalConfigPath);
            if (subDirs.Length == 0)
            {
                Console.WriteLine("Không tìm thấy subfolder trong cấu hình NewWindowsService.");
                return;
            }

            string versionDir = Path.Combine(subDirs[0], "1.0.0.0");
            if (!Directory.Exists(versionDir))
            {
                Console.WriteLine("Không tìm thấy thư mục version 1.0.0.0.");
                return;
            }

            string userConfigPath = Path.Combine(versionDir, "user.config");
            if (!File.Exists(userConfigPath))
            {
                Console.WriteLine($"Không tìm thấy file user.config tại {userConfigPath}.");
                return;
            }

            Console.WriteLine($"Đã tìm thấy user.config tại: {userConfigPath}");

            // Bước 3: Load và chỉnh sửa XML
            XDocument doc = XDocument.Load(userConfigPath);
            var settings = doc.Descendants("setting").ToList();

            UpdateSetting(settings, "roomNumber", room);
            UpdateSetting(settings, "comNumber", index);
            UpdateSetting(settings, "COM_PORT", comPort);
            UpdateSetting(settings, "MQTT_broker", mqttBroker);

            // Bước 4: Save lại
            doc.Save(userConfigPath);
            Console.WriteLine("Đã lưu user.config thành công.");
        }

        static void UpdateSetting(System.Collections.Generic.List<XElement> settings, string name, string value)
        {
            if (value == null) return;

            var setting = settings.FirstOrDefault(x => (string)x.Attribute("name") == name);
            if (setting != null)
            {
                setting.Element("value").Value = value;
                Console.WriteLine($"Đã cập nhật {name} = {value}");
            }
            else
            {
                Console.WriteLine($"Không tìm thấy setting {name}.");
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Cách dùng:");
            Console.WriteLine("  TerminalConf.exe [-r <Phòng>] [-i <Máy số>] [-l <COM_PORT>] [-f <MQTT_Broker>]");
        }
    }
}
