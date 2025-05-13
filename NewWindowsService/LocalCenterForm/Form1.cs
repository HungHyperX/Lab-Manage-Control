using System;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Text;

namespace LocalCenterForm
{
    public partial class MainForm : Form
    {
        private IMqttClient mqttClient;

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectMqtt();
        }

        private async void ConnectMqtt()
        {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(txtHost.Text.Trim())
                .WithCredentials(txtUser.Text.Trim(), txtPassword.Text.Trim())
                .Build();

            mqttClient.UseConnectedHandler(e => Log("MQTT connected"));
            mqttClient.UseDisconnectedHandler(e => Log("MQTT disconnected"));
            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Log($"MQTT Msg: {message}");
            });

            try
            {
                await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message);
            }
        }

        private async void PublishCommand(string command)
        {
            if (mqttClient?.IsConnected == true)
            {
                string topic = $"terminal/{txtRoom.Text}-{txtComputer.Text}/cmd";
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(command)
                    .WithAtLeastOnceQoS()
                    .Build();

                await mqttClient.PublishAsync(message);
                Log($"Command sent: {command}");
            }
            else
            {
                Log("MQTT not connected");
            }
        }

        private void btnEnableFirewall_Click(object sender, EventArgs e)
        {
            PublishCommand("firewall_on");
        }

        private void btnDisableFirewall_Click(object sender, EventArgs e)
        {
            PublishCommand("firewall_off");
        }

        private void btnShutdown_Click(object sender, EventArgs e)
        {
            PublishCommand("shutdown");
        }

        private void btnLightOn_Click(object sender, EventArgs e)
        {
            PublishCommand("light_on");
        }

        private void SaveSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.MqttHost = txtHost.Text.Trim();
            Properties.Settings.Default.MqttUser = txtUser.Text.Trim();
            Properties.Settings.Default.MqttPassword = txtPassword.Text.Trim();
            Properties.Settings.Default.Room = txtRoom.Text.Trim();
            Properties.Settings.Default.Computer = txtComputer.Text.Trim();
            Properties.Settings.Default.Save();
            Log("Settings saved");
        }

        private void LoadSettings()
        {
            txtHost.Text = Properties.Settings.Default.MqttHost;
            txtUser.Text = Properties.Settings.Default.MqttUser;
            txtPassword.Text = Properties.Settings.Default.MqttPassword;
            txtRoom.Text = Properties.Settings.Default.Room;
            txtComputer.Text = Properties.Settings.Default.Computer;
        }

        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }
    }
}
