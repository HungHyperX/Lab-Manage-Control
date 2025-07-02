using System;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace LocalCenterForm
{
    public partial class MainForm : Form
    {
        private IMqttClient _client;
        private MqttClientOptions _options;
        private readonly int _port = 1883;
        private string _topic;
        private string _subTopic;
        private string _processesTopic;
        private string _studentTagTopic;
        private string _killProcessTopic;

        private readonly SemaphoreSlim _connectLock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, string> _machines = new Dictionary<string, string>(); // room/com -> status

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
            UpdateCurrentRoomAndComputerLabels();
            InitializeTopics();
            InitializeMQTT();
            _ = AttemptInitialConnection();
        }

        private void InitializeTopics()
        {
            string roomNumber = Properties.Settings.Default.Room;
            string comNumber = Properties.Settings.Default.Computer;
            _topic = $"{roomNumber}/{comNumber}/subTerminal";
            _subTopic = $"{roomNumber}/{comNumber}/thongtin";
            _processesTopic = $"{roomNumber}/{comNumber}/processes";
            _studentTagTopic = $"{roomNumber}/{comNumber}/studentTagId";
            _killProcessTopic = $"{roomNumber}/{comNumber}/killProcess";
        }

        private void UpdateCurrentRoomAndComputerLabels()
        {
            lblCurrentRoom.Text = $"Current Room: {Properties.Settings.Default.Room}";
            lblCurrentComputer.Text = $"Current Computer: {Properties.Settings.Default.Computer}";
        }

        private string GetBrokerFromUI() =>
            string.IsNullOrWhiteSpace(txtHost.Text) ? "broker.hivemq.com" : txtHost.Text.Trim();

        private void InitializeMQTT()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(GetBrokerFromUI(), _port)
                .WithClientId($"Client-{Guid.NewGuid()}")
                .Build();

            _client.ConnectedAsync += async e =>
            {
                Invoke((MethodInvoker)delegate
                {
                    Log("Connected to MQTT broker successfully");
                    btnConnect.Enabled = false;
                    lblStatus.Text = "MQTT Connected";
                });

                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_subTopic).Build());
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_processesTopic).Build());
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_studentTagTopic).Build());
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("+/+/hunghyper/LocalCenter/status").Build()); // Sử dụng +/+/status

                Invoke((MethodInvoker)delegate
                {
                    Log($"Subscribed to {_subTopic}, {_processesTopic}, {_studentTagTopic}, +/+/hunghyper/LocalCenter/status");
                });
            };

            _client.DisconnectedAsync += async e =>
            {
                Invoke((MethodInvoker)delegate
                {
                    Log("Disconnected from MQTT broker");
                    btnConnect.Enabled = true;
                    lblStatus.Text = "MQTT Disconnected";
                });
                await AttemptReconnect();
            };

            _client.ApplicationMessageReceivedAsync += e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload?.ToArray() ?? Array.Empty<byte>());

                Invoke((MethodInvoker)delegate
                {
                    if (topic == _subTopic)
                    {
                        Log($"Message received on {topic}: {payload}");
                        UpdateThongTinTable(payload);
                    }
                    else if (topic == _processesTopic)
                    {
                        Log($"Processes received on {topic}: {payload}");
                        UpdateProcessList(payload);
                    }
                    else if (topic == _studentTagTopic)
                    {
                        Log($"RFID Tag received on {topic}: {payload}");
                        UpdateRfidDisplay(payload);
                    }
                    else if (topic.EndsWith("/status"))
                    {
                        string[] parts = topic.Split('/');
                        Log($"Received status topic: {topic}, parts: {string.Join(", ", parts)}");
                        if (parts.Length == 5) // Đảm bảo có đủ 3 phần: roomNumber, comNumber, status
                        {
                            string machineId = $"{parts[0]}/{parts[1]}";
                            dynamic json = JsonConvert.DeserializeObject(payload);
                            string status = json?.status?.ToString() ?? "Unknown";
                            Log($"Processing machine {machineId} with status: {status}");
                            if (!_machines.ContainsKey(machineId))
                            {
                                _machines[machineId] = status;
                                UpdateMachineComboBox();
                            }
                            else if (_machines[machineId] != status)
                            {
                                _machines[machineId] = status;
                                UpdateMachineComboBox();
                            }
                        }
                        else
                        {
                            Log($"Invalid topic structure: {topic}, expected 3 parts");
                        }
                    }
                });

                return Task.CompletedTask;
            };
        }

        private void UpdateMachineComboBox()
        {
            if (comboBoxMachines.InvokeRequired)
            {
                comboBoxMachines.Invoke(new Action(UpdateMachineComboBox));
            }
            else
            {
                string selectedMachine = comboBoxMachines.SelectedItem?.ToString();
                comboBoxMachines.Items.Clear();
                Log($"Updating comboBox with machines: {string.Join(", ", _machines.Where(m => m.Value == "online").Select(m => m.Key))}");
                foreach (var machine in _machines.Where(m => m.Value == "online"))
                {
                    comboBoxMachines.Items.Add(machine.Key);
                }
                if (!string.IsNullOrEmpty(selectedMachine) && comboBoxMachines.Items.Contains(selectedMachine))
                {
                    comboBoxMachines.SelectedItem = selectedMachine;
                }
                else if (comboBoxMachines.Items.Count > 0)
                {
                    comboBoxMachines.SelectedIndex = 0;
                }
            }
        }

        private void UpdateProcessList(string json)
        {
            try
            {
                var processes = JsonConvert.DeserializeObject<List<ProcessInfo>>(json);
                if (processes == null) return;

                processes = processes.OrderBy(p => p.Name).ToList();
                lvProcesses.Items.Clear();

                foreach (var process in processes)
                {
                    var item = new ListViewItem(process.Name);
                    item.SubItems.Add(process.Id.ToString());
                    lvProcesses.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Log($"Error parsing processes: {ex.Message}");
            }
        }

        private async void UpdateRfidDisplay(string json)
        {
            try
            {
                var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (jsonObj != null && jsonObj.ContainsKey("rfid"))
                {
                    var rfidValue = jsonObj["rfid"];
                    txtRfid.Text = rfidValue;

                    Log($"Đang truy vấn thông tin sinh viên với thẻ: {rfidValue}");
                    if (rfidValue != " ")
                    {
                        var student = await GetStudentInfoFromApi(rfidValue);
                        if (student != null)
                        {
                            dataGridView1.Rows.Clear();
                            dataGridView1.Rows.Add("Uid", student.Uid);
                            dataGridView1.Rows.Add("Họ tên", student.Name);
                            dataGridView1.Rows.Add("Ngày sinh", student.dob);
                            dataGridView1.Rows.Add("Ngành học/Khóa", student.Department);
                            dataGridView1.Rows.Add("MSSV", student.mssv);

                            Log($"Đã hiển thị thông tin sinh viên: {student.Name}");
                        }
                        else
                        {
                            Log($"Không tìm thấy thông tin sinh viên với mã thẻ: {rfidValue}");
                        }
                    }
                    else
                    {
                        dataGridView1.Rows.Clear();
                    }
                }
                else
                {
                    Log("Dữ liệu RFID không hợp lệ");
                }
            }
            catch (Exception ex)
            {
                Log($"Lỗi xử lý RFID JSON: {ex.Message}");
            }
        }

        private void UpdateThongTinTable(string json)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (data == null) return;

                infoDataGrid.Rows.Clear();

                foreach (var kvp in data)
                {
                    int rowIndex = infoDataGrid.Rows.Add();
                    infoDataGrid.Rows[rowIndex].Cells[0].Value = kvp.Key;
                    infoDataGrid.Rows[rowIndex].Cells[1].Value = kvp.Value?.ToString();
                }
            }
            catch (Exception ex)
            {
                Log($"Error parsing thongtin JSON: {ex.Message}");
            }
        }

        private class ProcessInfo
        {
            public string Name { get; set; }
            public int Id { get; set; }
        }

        private async Task AttemptInitialConnection() => await ConnectMqtt();

        private async Task AttemptReconnect()
        {
            await Task.Delay(5000);
            await ConnectMqtt();
        }

        private async Task ConnectMqtt()
        {
            await _connectLock.WaitAsync();
            try
            {
                if (!_client.IsConnected)
                {
                    await _client.ConnectAsync(_options);
                }
            }
            catch (Exception ex)
            {
                Invoke((MethodInvoker)delegate
                {
                    Log($"Connection failed: {ex.Message}");
                });
                _ = AttemptReconnect();
            }
            finally
            {
                _connectLock.Release();
            }
        }

        private async Task PublishCommand(string command)
        {
            try
            {
                if (!_client.IsConnected)
                {
                    Log("Not connected to broker. Attempting to reconnect...");
                    await ConnectMqtt();
                }

                if (_client.IsConnected)
                {
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic(_topic)
                        .WithPayload(Encoding.UTF8.GetBytes(command))
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build();

                    await _client.PublishAsync(message);
                    Log($"Published command: {command} to topic: {_topic}");
                }
                else
                {
                    Log("Failed to publish: Not connected to broker");
                }
            }
            catch (Exception ex)
            {
                Log($"Publish failed: {ex.Message}");
            }
        }

        private async void btnConnect_Click(object sender, EventArgs e) => await ConnectMqtt();
        private async void btnEnableFirewall_Click(object sender, EventArgs e) => await PublishCommand("ON");
        private async void btnDisableFirewall_Click(object sender, EventArgs e) => await PublishCommand("OFF");
        private async void btnShutdown_Click(object sender, EventArgs e) => await PublishCommand("shutdown");
        private async void btnLightOn_Click(object sender, EventArgs e) => await PublishCommand("light_on");

        private async void btnKillProcess_Click(object sender, EventArgs e)
        {
            if (lvProcesses.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a process to kill.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = lvProcesses.SelectedItems[0];
            var processId = selectedItem.SubItems[1].Text; // ID process ở cột thứ 2

            try
            {
                if (!_client.IsConnected)
                {
                    Log("Not connected to broker. Attempting to reconnect...");
                    await ConnectMqtt();
                }

                if (_client.IsConnected)
                {
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic(_killProcessTopic)
                        .WithPayload(Encoding.UTF8.GetBytes(processId))
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build();

                    await _client.PublishAsync(message);
                    Log($"Sent kill process command for Process ID: {processId} to topic: {_killProcessTopic}");
                }
                else
                {
                    Log("Failed to send kill process command: Not connected to broker");
                }
            }
            catch (Exception ex)
            {
                Log($"Error sending kill process command: {ex.Message}");
            }
        }

        private async void btnUpdateTopics_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Room = txtRoom.Text.Trim();
            Properties.Settings.Default.Computer = txtComputer.Text.Trim();
            Properties.Settings.Default.Save();
            Log("Room and Computer settings saved");

            UpdateCurrentRoomAndComputerLabels();
            InitializeTopics();
            Log($"Updated topics: {_subTopic}, {_processesTopic}, {_studentTagTopic}");

            if (_client.IsConnected)
            {
                try
                {
                    await _client.UnsubscribeAsync(_subTopic);
                    await _client.UnsubscribeAsync(_processesTopic);
                    await _client.UnsubscribeAsync(_studentTagTopic);
                    Log($"Unsubscribed from old topics");

                    await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_subTopic).Build());
                    await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_processesTopic).Build());
                    await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_studentTagTopic).Build());
                    Log($"Subscribed to new topics: {_subTopic}, {_processesTopic}, {_studentTagTopic}");
                }
                catch (Exception ex)
                {
                    Log($"Error updating subscriptions: {ex.Message}");
                }
            }

            // Check máy hiện tại có online không
            string currentKey = $"{Properties.Settings.Default.Room}/{Properties.Settings.Default.Computer}";
            if (!_machines.ContainsKey(currentKey) || _machines[currentKey] != "online")
            {
                Log($"Máy {currentKey} không hoạt động hoặc không online.");

                // Xóa thông tin ở các bảng
                infoDataGrid.Rows.Clear();
                lvProcesses.Items.Clear();
                txtRfid.Text = "";
                dataGridView1.Rows.Clear();

                // Thêm thông báo
                int rowIndex = infoDataGrid.Rows.Add();
                infoDataGrid.Rows[rowIndex].Cells[0].Value = "Trạng thái";
                infoDataGrid.Rows[rowIndex].Cells[1].Value = "Máy không hoạt động";
            }
            else
            {
                Log($"Máy {currentKey} đang hoạt động.");
            }
        }

        private async void btnShowMachines_Click(object sender, EventArgs e)
        {
            // Cập nhật danh sách máy từ _machines
            UpdateMachineComboBox();

            if (comboBoxMachines.Items.Count > 0)
            {
                comboBoxMachines.Visible = true;
                Log("Machine list updated and displayed.");
            }
            else
            {
                Log("No active machines found.");
                MessageBox.Show("No active machines found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void comboBoxMachines_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxMachines.SelectedItem != null)
            {
                string selectedMachine = comboBoxMachines.SelectedItem.ToString();
                string[] parts = selectedMachine.Split('/');
                if (parts.Length == 2)
                {
                    Properties.Settings.Default.Room = parts[0].Trim();
                    Properties.Settings.Default.Computer = parts[1].Trim();
                    Properties.Settings.Default.Save();

                    UpdateCurrentRoomAndComputerLabels();
                    InitializeTopics();
                    Log($"Selected machine: {selectedMachine}. Topics updated to {_subTopic}, {_processesTopic}, {_studentTagTopic}");
                }
            }
        }

        private void SaveSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.MqttHost = txtHost.Text.Trim();
            Properties.Settings.Default.Save();
            Log("MQTT Host settings saved");
        }

        private void LoadSettings()
        {
            txtHost.Text = Properties.Settings.Default.MqttHost;
            txtRoom.Text = Properties.Settings.Default.Room;
            txtComputer.Text = Properties.Settings.Default.Computer;
        }

        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        private async Task<Student> GetStudentInfoFromApi(string rfid)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = "https://apisinhvienfake.onrender.com/api/student";

                    var response = await client.GetAsync($"{apiUrl}/{rfid}");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<Student>(json);
                    }
                    else
                    {
                        Log($"API trả về lỗi: {(int)response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Lỗi gọi API: {ex.Message}");
                return null;
            }
        }
    }

    public class Student
    {
        public string Id { get; set; }
        public string Uid { get; set; }
        public string Name { get; set; }
        public string dob { get; set; }
        public string Department { get; set; }
        public string mssv { get; set; }
    }
}