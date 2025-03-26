using System;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using MQTTnet;
using MQTTnet.Client;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace LocalCenter
{
    public partial class LocalCenter : ServiceBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IMqttClient _client;
        private IMqttClientOptions _options;
        private readonly string _broker = "test.mosquitto.org";
        private readonly int _port = 1883;
        private readonly string _topic = "may1/thongtin";

        public LocalCenter()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log.Info("LocalCenter Service Started.");
            Task.Run(() => ConnectMqtt());
        }

        private async Task ConnectMqtt()
        {
            try
            {
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();

                _options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_broker, _port)
                    .WithClientId("LocalCenterClient")
                    .WithCleanSession()
                    .Build();

                _client.Connected += async (s, e) =>
                {
                    log.Info("Connected to MQTT Broker.");
                    await _client.SubscribeAsync(new TopicFilterBuilder().WithTopic(_topic).Build());
                    log.Info($"Subscribed to topic: {_topic}");
                };

                _client.Disconnected += async (s, e) =>
                {
                    log.Warn("Disconnected from MQTT Broker. Reconnecting in 5s...");
                    await Task.Delay(5000);
                    await _client.ConnectAsync(_options);
                };

                _client.ApplicationMessageReceived += (s, e) =>
                {
                    string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    log.Info($"Received MQTT Message: {message}");
                };

                await _client.ConnectAsync(_options);
            }
            catch (Exception ex)
            {
                log.Error("MQTT connection error", ex);
            }
        }

        protected override void OnStop()
        {
            log.Info("LocalCenter Service Stopped.");
            _client?.DisconnectAsync();
        }
    }
}
