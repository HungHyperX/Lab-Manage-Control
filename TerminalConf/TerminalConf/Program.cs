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
            if (args.Length != 6 || args[0] != "-r" || args[2] != "-i" || args[4] != "-l")
            {
                Console.WriteLine("Cách dùng: TerminalConf.exe -r <Phòng> -i <Máy số> -l <COM_PORT>");
                return;
            }

            string room = args[1];
            string index = args[3];
            string comPort = args[5];

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

            var roomSetting = settings.FirstOrDefault(x => (string)x.Attribute("name") == "roomNumber");
            var indexSetting = settings.FirstOrDefault(x => (string)x.Attribute("name") == "comNumber");
            var comPortSetting = settings.FirstOrDefault(x => (string)x.Attribute("name") == "COM_PORT");

            if (roomSetting != null)
            {
                roomSetting.Element("value").Value = room;
                Console.WriteLine($"Đã cập nhật Room = {room}");
            }
            else
            {
                Console.WriteLine("Không tìm thấy setting Room.");
            }

            if (indexSetting != null)
            {
                indexSetting.Element("value").Value = index;
                Console.WriteLine($"Đã cập nhật Index = {index}");
            }
            else
            {
                Console.WriteLine("Không tìm thấy setting Index.");
            }

            if (comPortSetting != null)
            {
                comPortSetting.Element("value").Value = comPort;
                Console.WriteLine($"Đã cập nhật COM_PORT = {comPort}");
            }
            else
            {
                Console.WriteLine("Không tìm thấy setting COM_PORT.");
            }

            // Bước 4: Save lại
            doc.Save(userConfigPath);
            Console.WriteLine("Đã lưu user.config thành công.");
        }
    }
}
