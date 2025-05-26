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
        private readonly SemaphoreSlim _connectLock = new SemaphoreSlim(1, 1);

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
                    lblStatus.Text = "Connected";
                });

                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_subTopic).Build());
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_processesTopic).Build());
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_studentTagTopic).Build());

                Invoke((MethodInvoker)delegate
                {
                    Log($"Subscribed to {_subTopic}, {_processesTopic}, {_studentTagTopic}");
                });
            };

            _client.DisconnectedAsync += async e =>
            {
                Invoke((MethodInvoker)delegate
                {
                    Log("Disconnected from MQTT broker");
                    btnConnect.Enabled = true;
                    lblStatus.Text = "Disconnected";
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
                        //txtMqttMessages.AppendText($"[{DateTime.Now:HH:mm:ss}] [{topic}] {payload}\r\n");
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
                });

                return Task.CompletedTask;
            };
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

        private void UpdateRfidDisplay(string json)
        {
            try
            {
                var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (jsonObj != null && jsonObj.ContainsKey("rfid"))
                {
                    var rfidValue = jsonObj["rfid"];
                    txtRfid.Text = rfidValue;
                }
                else
                {
                    Log("Invalid RFID JSON received");
                }
            }
            catch (Exception ex)
            {
                Log($"Error parsing RFID JSON: {ex.Message}");
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
    }
}
