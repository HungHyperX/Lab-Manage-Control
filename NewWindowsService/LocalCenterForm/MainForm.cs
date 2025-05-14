
using System;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LocalCenterForm
{
    public partial class MainForm : Form
    {
        private IMqttClient _client;
        private MqttClientOptions _options;
        private readonly int _port = 1883;
        private readonly string _topic = "may1/subTerminal";
        private readonly SemaphoreSlim _connectLock = new SemaphoreSlim(1, 1);

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
            InitializeMQTT();
            _ = AttemptInitialConnection();
        }

        private string GetBrokerFromUI() =>
            string.IsNullOrWhiteSpace(txtHost.Text) ? "test.mosquitto.org" : txtHost.Text.Trim();

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
                await Task.CompletedTask;
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
        }

        private async Task AttemptInitialConnection()
        {
            await ConnectMqtt();
        }

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

        private void SaveSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.MqttHost = txtHost.Text.Trim();
            Properties.Settings.Default.Room = txtRoom.Text.Trim();
            Properties.Settings.Default.Computer = txtComputer.Text.Trim();
            Properties.Settings.Default.Save();
            Log("Settings saved");
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
