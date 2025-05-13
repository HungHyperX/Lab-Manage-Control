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
            if (args.Length != 4 || args[0] != "-r" || args[2] != "-i")
            {
                Console.WriteLine("Cách dùng: TerminalConf.exe -r <Phòng> -i <Máy số>");
                return;
            }

            string room = args[1];
            string index = args[3];

            // Bước 1: Xác định các khả năng đường dẫn
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // Roaming

            string[] possiblePaths = new string[]
            {
                @"C:\Windows\System32\config\systemprofile\AppData\Local\NewWindowsService",
                //@"C:\Windows\System32\config\systemprofile\AppData\Local\LocalCenter",
                Path.Combine(localAppData, "NewWindowsService"),
                //Path.Combine(localAppData, "LocalCenter"),
                @"C:\Windows\System32\config\systemprofile\AppData\Roaming\NewWindowsService",
                //@"C:\Windows\System32\config\systemprofile\AppData\Roaming\LocalCenter",
                Path.Combine(roamingAppData, "NewWindowsService")
                //Path.Combine(roamingAppData, "LocalCenter")
            };

            string finalConfigPath = null;

            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    finalConfigPath = path;
                    break;
                }
            }

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

            // Bước 4: Save lại
            doc.Save(userConfigPath);

            Console.WriteLine("Đã lưu user.config thành công.");
        }
    }
}
