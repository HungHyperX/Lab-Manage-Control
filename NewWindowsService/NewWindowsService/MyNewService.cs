using System;
using System.Management;
using System.ServiceProcess;
using System.Timers;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
using System.Text;
using System.Configuration;
using NewWindowsService.Properties;
using System.IO;
using System.IO.Ports;
using System.Linq;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace NewWindowsService
{
    public partial class MyNewService : ServiceBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private System.Timers.Timer timer;
        private IMqttClient _client;
        private IMqttClientOptions _options;
        private SerialPort _serialPort;
        private readonly string _broker = "broker.hivemq.com";//"test.mosquitto.org";
        private readonly int _port = 1883;
        private readonly string _pubTopic = "may1/thongtin";
        private readonly string _subTopic = "may1/subTerminal";
        private readonly string _processesTopic = "may1/processes"; // Topic để gửi danh sách tiến trình
        private readonly string _killProcessTopic = "may1/killProcess"; // Topic để nhận lệnh kill tiến trình

        public MyNewService()
        {
            InitializeComponent();
            InitializeMQTT();
        }

        private void InitializeMQTT()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(_broker, _port)
                .Build();

            _client.ApplicationMessageReceived += (s, e) =>
            {
                string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                log.Info($"Received MQTT Message from {e.ApplicationMessage.Topic}: {message}");

                if (e.ApplicationMessage.Topic == _subTopic)
                {
                    if (message.Trim().ToUpper() == "ON")
                    {
                        log.Info("Turning Firewall ON");
                        SetFirewallState(true);
                    }
                    else if (message.Trim().ToUpper() == "OFF")
                    {
                        log.Info("Turning Firewall OFF");
                        SetFirewallState(false);
                    }
                    else if (message.Trim().ToUpper() == "SHUTDOWN")
                    {
                        log.Info("Received shutdown command");
                        ShutdownComputer();
                    }
                }
                else if (e.ApplicationMessage.Topic == _killProcessTopic && !string.IsNullOrEmpty(message))
                {
                    log.Info($"Received kill process command: {message}");
                    KillProcess(message);
                }
            };
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Terminal Service Started.");

            if (!IsUserConfigCreated())
            {
                Properties.Settings.Default.roomNumber = "B1-221";
                Properties.Settings.Default.comNumber = "24";
                Properties.Settings.Default.COM_PORT = "COM7";
                Properties.Settings.Default.Save();
                log.Info("Ghi cấu hình mặc định vì user.config chưa tồn tại.");
            }
            else
            {
                log.Info($"Đọc cấu hình từ user.config: Room = {Properties.Settings.Default.roomNumber}, Com = {Properties.Settings.Default.comNumber}");
            }

            // Khởi tạo Serial Port
            string com_port = Properties.Settings.Default.COM_PORT;
            _serialPort = new SerialPort(com_port, 115200); // Cập nhật COM port thực tế nếu khác
            _serialPort.Encoding = Encoding.UTF8;
            _serialPort.NewLine = "\n";

            try
            {
                if (!_serialPort.IsOpen)
                    _serialPort.Open();
                log.Info("Serial port opened successfully.");
            }
            catch (Exception ex)
            {
                log.Error("Error opening serial port", ex);
            }

            timer = new System.Timers.Timer(20000); // 20 giây
            timer.Elapsed += async (sender, e) => await OnElapsedTime();
            timer.Start();

            Task.Run(() => ConnectAndSubscribe());
        }

        protected override void OnStop()
        {
            log.Info("Terminal Service Stopped.");
            timer.Stop();

            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                log.Info("Serial port closed.");
            }
        }

        private bool IsUserConfigCreated()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            return File.Exists(config.FilePath);
        }

        private async Task ConnectAndSubscribe()
        {
            try
            {
                if (!_client.IsConnected)
                {
                    await _client.ConnectAsync(_options);
                    log.Info("Connected to MQTT Broker.");
                }

                // Subscribe vào các topic
                await _client.SubscribeAsync(new TopicFilterBuilder().WithTopic(_subTopic).Build());
                await _client.SubscribeAsync(new TopicFilterBuilder().WithTopic(_killProcessTopic).Build());
                log.Info($"Subscribed to topics: {_subTopic}, {_killProcessTopic}");
            }
            catch (Exception ex)
            {
                log.Error("Error connecting to MQTT or subscribing to topic", ex);
            }
        }

        private async Task OnElapsedTime()
        {
            log.Info("Service is running...");

            // Lấy thông tin hệ thống
            string ipAddress = GetLocalIPAddress();
            string macAddress = GetMACAddress();
            string cpuInfo = GetCPUInfo();
            string ramInfo = GetRAMInfo();
            string diskInfo = GetDiskInfo();
            string hostname = GetHostname();
            string firewallStatus = GetFirewallStatusNetsh();
            string roomNumber = Properties.Settings.Default.roomNumber;
            string comNumber = Properties.Settings.Default.comNumber;

            string message = $"{{ \"Room\": \"{roomNumber}\", \"Com\": \"{comNumber}\", \"IP\": \"{ipAddress}\", \"MAC\": \"{macAddress}\", \"CPU\": \"{cpuInfo}\", \"RAM\": \"{ramInfo}\", \"Disk\": \"{diskInfo}\", \"Hostname\": \"{hostname}\", \"Firewall\": \"{firewallStatus}\" }}";
            log.Info($"Sending data to MQTT: {message}");

            await SendToMQTT(message);

            // Gửi danh sách tiến trình
            string processesJson = GetRunningProcesses();
            log.Info($"Sending processes to MQTT: {processesJson}");
            await SendToMQTT(processesJson, _processesTopic);

            // Gửi qua Serial
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine(message);
                    log.Info("Sent data over Serial: " + message);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error sending data over Serial", ex);
            }
        }

        private async Task SendToMQTT(string message, string topic = null)
        {
            try
            {
                if (!_client.IsConnected)
                {
                    await _client.ConnectAsync(_options);
                }

                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic ?? _pubTopic) // Sử dụng topic mặc định nếu không chỉ định
                    .WithPayload(message)
                    .WithRetainFlag()
                    .Build();

                await _client.PublishAsync(mqttMessage);
            }
            catch (Exception ex)
            {
                log.Error("Error sending data to MQTT", ex);
            }
        }

        private void SetFirewallState(bool enable)
        {
            try
            {
                string command = enable ? "netsh advfirewall set allprofiles state on"
                                        : "netsh advfirewall set allprofiles state off";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = new Process { StartInfo = psi };
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    log.Error($"Firewall command error: {error}");
                }
                else
                {
                    log.Info($"Firewall command executed: {output}");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error executing firewall command", ex);
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string[] addresses = (string[])queryObj["IPAddress"];
                    if (addresses != null && addresses.Length > 0)
                        return addresses[0];
                }
            }
            catch (Exception ex)
            {
                log.Error("Error getting IP Address", ex);
            }
            return "Unknown";
        }

        private string GetMACAddress()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE MACAddress IS NOT NULL");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    return queryObj["MACAddress"].ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error getting MAC Address", ex);
            }
            return "Unknown";
        }

        private string GetCPUInfo()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    return queryObj["Name"].ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error getting CPU Info", ex);
            }
            return "Unknown";
        }

        private string GetRAMInfo()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    double totalRam = Convert.ToDouble(queryObj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024);
                    return $"{totalRam:F2} GB";
                }
            }
            catch (Exception ex)
            {
                log.Error("Error getting RAM Info", ex);
            }
            return "Unknown";
        }

        private string GetDiskInfo()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    return queryObj["Model"].ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error getting Disk Info", ex);
            }
            return "Unknown";
        }

        private string GetHostname()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    return queryObj["Name"].ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error getting Hostname", ex);
            }
            return "Unknown";
        }

        private string GetFirewallStatusNetsh()
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "netsh";
                process.StartInfo.Arguments = "advfirewall show allprofiles state";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (output.Contains("ON"))
                    return "ON";
                else if (output.Contains("OFF"))
                    return "OFF";
            }
            catch (Exception ex)
            {
                log.Error("Error checking Firewall status via netsh", ex);
            }
            return "Unknown";
        }

        private void ShutdownComputer()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("shutdown", "/s /t 0")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi);
                log.Info("Shutdown command issued.");
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to shutdown the computer", ex);
            }
        }

        private string GetRunningProcesses()
        {
            try
            {
                var processes = Process.GetProcesses();
                var processList = processes.Select(p => new
                {
                    Name = p.ProcessName,
                    Id = p.Id
                }).ToList();

                string json = System.Text.Json.JsonSerializer.Serialize(processList);
                return json;
            }
            catch (Exception ex)
            {
                log.Error("Error getting running processes", ex);
                return "[]"; // Trả về mảng rỗng nếu có lỗi
            }
        }

        private void KillProcess(string processInfo)
        {
            try
            {
                // Giả sử processInfo là tên tiến trình hoặc ID
                if (int.TryParse(processInfo, out int processId))
                {
                    // Tắt tiến trình bằng ID
                    var process = Process.GetProcessById(processId);
                    if (process != null)
                    {
                        process.Kill();
                        log.Info($"Process with ID {processId} terminated.");
                    }
                    else
                    {
                        log.Warn($"No process found with ID {processId}.");
                    }
                }
                else
                {
                    // Tắt tiến trình bằng tên
                    var processes = Process.GetProcessesByName(processInfo);
                    if (processes.Length > 0)
                    {
                        foreach (var process in processes)
                        {
                            process.Kill();
                            log.Info($"Process {processInfo} (ID: {process.Id}) terminated.");
                        }
                    }
                    else
                    {
                        log.Warn($"No process found with name {processInfo}.");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error terminating process {processInfo}", ex);
            }
        }

    }


}
