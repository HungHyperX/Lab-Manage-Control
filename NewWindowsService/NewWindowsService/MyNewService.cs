﻿using System;
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
using Newtonsoft.Json;
using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

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
        private readonly string _broker; // = "broker.hivemq.com";
        private readonly int _port = 1883;
        private string _pubTopic; // Topic động: {roomNumber}/{comNumber}/thongtin
        private string _subTopic; // Topic động: {roomNumber}/{comNumber}/subTerminal
        private string _processesTopic; // Topic động: {roomNumber}/{comNumber}/processes
        private string _killProcessTopic; // Topic động: {roomNumber}/{comNumber}/killProcess
        private string _studentTagTopic; // topic gửi mã thẻ: {roomNumber}/{comNumber}/studentTagId 

        public MyNewService()
        {
            InitializeComponent();
            _broker = "broker.hivemq.com";
            InitializeMQTT();
        }

        //private void InitializeMQTT()
        //{
        //    var factory = new MqttFactory();
        //    _client = factory.CreateMqttClient();

        //    _options = new MqttClientOptionsBuilder()
        //        .WithTcpServer(_broker, _port)
        //        .Build();

        //    _client.ApplicationMessageReceived += (s, e) =>
        //    {
        //        string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        //        log.Info($"Received MQTT Message from {e.ApplicationMessage.Topic}: {message}");

        //        if (e.ApplicationMessage.Topic == _subTopic)
        //        {
        //            if (message.Trim().ToUpper() == "ON")
        //            {
        //                log.Info("Turning Firewall ON");
        //                SetFirewallState(true);
        //            }
        //            else if (message.Trim().ToUpper() == "OFF")
        //            {
        //                log.Info("Turning Firewall OFF");
        //                SetFirewallState(false);
        //            }
        //            else if (message.Trim().ToUpper() == "SHUTDOWN")
        //            {
        //                log.Info("Received shutdown command");
        //                ShutdownComputer();
        //            }
        //            else if (message.Trim().ToUpper() == "LIGHT_ON")
        //            {
        //                log.Info("Received light command");
        //                ChangeLedState();
        //            }
        //        }
        //        else if (e.ApplicationMessage.Topic == _killProcessTopic && !string.IsNullOrEmpty(message))
        //        {
        //            log.Info($"Received kill process command: {message}");
        //            KillProcess(message);
        //        }
        //    };
        //}

        //protected override void OnStart(string[] args)
        //{
        //    log.Info("Terminal Service Started.");

        //    if (!IsUserConfigCreated())
        //    {
        //        Properties.Settings.Default.roomNumber = "B1-221";
        //        Properties.Settings.Default.comNumber = "24";
        //        Properties.Settings.Default.COM_PORT = "COM7";
        //        Properties.Settings.Default.MQTT_broker = "broker.hivemq.com";
        //        Properties.Settings.Default.Save();
        //        log.Info("Ghi cấu hình mặc định vì user.config chưa tồn tại.");
        //    }
        //    else
        //    {
        //        log.Info($"Đọc cấu hình từ user.config: Room = {Properties.Settings.Default.roomNumber}, Com = {Properties.Settings.Default.comNumber}");
        //    }

        //    // Khởi tạo các topic động
        //    string roomNumber = Properties.Settings.Default.roomNumber;
        //    string comNumber = Properties.Settings.Default.comNumber;
        //    _pubTopic = $"{roomNumber}/{comNumber}/thongtin";
        //    _subTopic = $"{roomNumber}/{comNumber}/subTerminal";
        //    _processesTopic = $"{roomNumber}/{comNumber}/processes";
        //    _killProcessTopic = $"{roomNumber}/{comNumber}/killProcess";
        //    _studentTagTopic = $"{roomNumber}/{comNumber}/studentTagId";

        //    // Khởi tạo Serial Port
        //    string com_port = Properties.Settings.Default.COM_PORT;
        //    _serialPort = new SerialPort(com_port, 115200);
        //    _serialPort.Encoding = Encoding.UTF8;
        //    _serialPort.NewLine = "\n";



        //    try
        //    {
        //        if (!_serialPort.IsOpen)
        //            _serialPort.Open();
        //        log.Info("Serial port opened successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Error opening serial port", ex);
        //    }

        //    _serialPort.DataReceived += SerialPort_DataReceived;

        //    timer = new System.Timers.Timer(20000); // 20 giây
        //    timer.Elapsed += async (sender, e) => await OnElapsedTime();
        //    timer.Start();

        //    Task.Run(() => ConnectAndSubscribe());
        //}

        //protected override void OnStop()
        //{
        //    log.Info("Terminal Service Stopped.");
        //    timer.Stop();

        //    if (_serialPort != null && _serialPort.IsOpen)
        //    {
        //        _serialPort.Close();
        //        log.Info("Serial port closed.");
        //    }
        //}

        private async Task SendStatusToMQTT(string status)
        {
            string statusTopic = $"{Properties.Settings.Default.roomNumber}/{Properties.Settings.Default.comNumber}/hunghyper/LocalCenter/status";
            string statusMessage = $"{{ \"status\": \"{status}\" }}";
            log.Info($"Sending status {Properties.Settings.Default.roomNumber}/{Properties.Settings.Default.comNumber} from to MQTT: {statusMessage}");
            await SendToMQTT(statusMessage, statusTopic);
        }

        private void InitializeMQTT()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            // Cấu hình LWT
            var willMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"{Properties.Settings.Default.roomNumber}/{Properties.Settings.Default.comNumber}/hunghyper/LocalCenter/status")
                .WithPayload(Encoding.UTF8.GetBytes("{\"status\": \"offline\"}"))
                .WithRetainFlag(true)
                .Build();

            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(_broker, _port)
                .WithWillMessage(willMessage)
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
                    else if (message.Trim().ToUpper() == "LIGHT_ON")
                    {
                        log.Info("Received light command");
                        ChangeLedState();
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
                Properties.Settings.Default.MQTT_broker = "broker.hivemq.com";
                Properties.Settings.Default.Save();
                log.Info("Ghi cấu hình mặc định vì user.config chưa tồn tại.");
            }
            else
            {
                log.Info($"Đọc cấu hình từ user.config: Room = {Properties.Settings.Default.roomNumber}, Com = {Properties.Settings.Default.comNumber}");
            }

            // Khởi tạo các topic động
            string roomNumber = Properties.Settings.Default.roomNumber;
            string comNumber = Properties.Settings.Default.comNumber;
            _pubTopic = $"{roomNumber}/{comNumber}/thongtin";
            _subTopic = $"{roomNumber}/{comNumber}/subTerminal";
            _processesTopic = $"{roomNumber}/{comNumber}/processes";
            _killProcessTopic = $"{roomNumber}/{comNumber}/killProcess";
            _studentTagTopic = $"{roomNumber}/{comNumber}/studentTagId";

            
            // Khởi tạo Serial Port
            string com_port = Properties.Settings.Default.COM_PORT;
            _serialPort = new SerialPort(com_port, 115200);
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

            _serialPort.DataReceived += SerialPort_DataReceived;

            timer = new System.Timers.Timer(20000); // 20 giây
            timer.Elapsed += async (sender, e) => await OnElapsedTime();
            timer.Start();

            //Task.Run(() => ConnectAndSubscribe());
            //// Gửi trạng thái online khi khởi động
            //Task.Run(async () => await SendStatusToMQTT("online"));

            Task.Run(async () =>
            {
                await ConnectAndSubscribe();
                await SendStatusToMQTT("online");
            });

        }

        protected override void OnStop()
        {
            log.Info("Terminal Service Stopped.");

            Task.Run(async () => await SendStatusToMQTT("offline")).Wait();

            timer.Stop();

            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                log.Info("Serial port closed.");
            }

            // Ngắt kết nối MQTT để kích hoạt LWT
            if (_client != null && _client.IsConnected)
            {
                Task.Run(async () => await _client.DisconnectAsync()).Wait();
                log.Info("Disconnected from MQTT Broker to trigger LWT.");
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
            log.Info("Timer ticked, collecting and sending system info.");
            await SendSystemInfo();

            // Gửi danh sách tiến trình
            string processesJson = GetRunningProcesses();
            log.Info($"Sending processes to MQTT: {processesJson}");
            await SendToMQTT(processesJson, _processesTopic);
        }

        private async Task SendSystemInfo()
        {
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
            log.Info($"[Auto] Sending system info to MQTT: {message}");
            await SendToMQTT(message);

            // Gửi qua Serial nếu cần
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine(message);
                    log.Info("Sent system info over Serial: " + message);
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
                    .WithTopic(topic ?? _pubTopic)
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

                string json = JsonConvert.SerializeObject(processList);
                return json;
            }
            catch (Exception ex)
            {
                log.Error("Error getting running processes", ex);
                return "[]";
            }
        }

        private void KillProcess(string processInfo)
        {
            try
            {
                bool killed = false;

                if (int.TryParse(processInfo, out int processId))
                {
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

                // Nếu có tiến trình đã bị kill thì gửi lại thông tin
                if (killed)
                {
                    _ = SendSystemInfo(); // Gọi bất đồng bộ
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error terminating process {processInfo}", ex);
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

                    // Gửi thông tin hệ thống sau khi thay đổi firewall
                    _ = SendSystemInfo(); // Gọi bất đồng bộ
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

        private void ChangeLedState()
        {
            string ledCommand = "{\"led_ring\": \"ring_on\"}";
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine(ledCommand);
                    log.Info($"Sent LED ring command over Serial: {ledCommand}");
                }
                else
                {
                    log.Warn("Serial port is not open. Cannot send LED ring command.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error sending LED ring command over Serial", ex);
            }
        }

        private async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _serialPort.ReadLine().Trim();
                log.Info($"Received data from Serial: {data}");

                // Kiểm tra xem dữ liệu có phải JSON không
                if (IsValidJson(data))
                {
                    var json = JsonConvert.DeserializeObject<dynamic>(data);
                    if (json.rfid != null)
                    {
                        string rfid = json.rfid.ToString();
                        log.Info($"Valid RFID JSON received: {rfid}");

                        // Tạo topic studentTagId
                        string roomNumber = Properties.Settings.Default.roomNumber;
                        string comNumber = Properties.Settings.Default.comNumber;
                        string studentTagIdTopic = $"{roomNumber}/{comNumber}/studentTagId";

                        // Gửi lên MQTT
                        await SendToMQTT(data, studentTagIdTopic);
                        log.Info($"Sent RFID data to MQTT topic {studentTagIdTopic}: {data}");
                    }
                    else
                    {
                        log.Warn("JSON received does not contain 'rfid' key.");
                    }
                }
                else
                {
                    log.Warn("Received data is not valid JSON.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error processing Serial data", ex);
            }
        }

        private bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || // object
                (strInput.StartsWith("[") && strInput.EndsWith("]")))   // array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    log.Warn("Invalid JSON: " + jex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    log.Error("Error in IsValidJson", ex);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


    }
}