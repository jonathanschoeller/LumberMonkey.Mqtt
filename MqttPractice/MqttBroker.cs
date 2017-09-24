using System;
using System.Security.Cryptography.X509Certificates;
using LumberMonkey.Mqtt;
using CommandMessenger.Transport.Serial;
using CommandMessenger;
using Serilog;
using System.Configuration;

namespace MQTT.Sample
{
    public class MqttBroker
    {
        private CmdMessenger m_CmdMessenger;
        private readonly string m_Topic = ConfigurationManager.AppSettings.Get("mqttSubscribeTopic");

        private static readonly MqttOptions s_MqttOptions = new MqttOptions
        {
            BrokerHostName = ConfigurationManager.AppSettings.Get("mqttBrokerHostName"),
            CaCert = X509Certificate.CreateFromSignedFile(ConfigurationManager.AppSettings.Get("mqttCaCertificatePath")),
            ClientCert = new X509Certificate2(ConfigurationManager.AppSettings.Get("mqttClientCertificatePath"), ConfigurationManager.AppSettings.Get("mqttClientCertificatePassword"))
        };

        public void Start()
        {
            var serialTransport = new SerialTransport
            {
                CurrentSerialSettings =
                {
                    PortName = ConfigurationManager.AppSettings.Get("serialPortName"),
                    BaudRate = int.Parse(ConfigurationManager.AppSettings.Get("serialBaudRate")),
                    DtrEnable = false
                }
            };

            m_CmdMessenger = new CmdMessenger(serialTransport, BoardType.Bit16, (char)0x13);

            // TODO: Route messages to/from more than one CmdMessenger (serial port) based on MQTT topic.
            var sendBroker = new CommandMessengerSendBroker(m_CmdMessenger, s_MqttOptions, m_Topic);
            sendBroker.MqttConnectionClosed += SendBroker_MqttConnectionClosed;
            sendBroker.MqttConnectionError += SendBroker_MqttConnectionError;
            sendBroker.Connect();

            var receiveBroker = new CommandMessengerReceiveBrokerBuffered(m_CmdMessenger, s_MqttOptions);
            receiveBroker.MqttConnectionClosed += ReceiveBroker_MqttConnectionClosed;
            receiveBroker.MqttConnectionError += ReceiveBroker_MqttConnectionError;
            receiveBroker.Connect($"{m_Topic}");

            m_CmdMessenger.Connect();

            Log.Information("MQTT Broker Started");
        }

        public void Stop()
        {
            if (m_CmdMessenger != null)
                m_CmdMessenger.Dispose();

            Log.Information("MQTT Broker Stopped");
        }

        private void SendBroker_MqttConnectionClosed(object sender, EventArgs e)
        {
            Log.Warning("Send Broker MQTT Connection closed");
        }

        private void ReceiveBroker_MqttConnectionClosed(object sender, EventArgs e)
        {
            Log.Warning("Receive Broker MQTT Connection closed");
        }

        private void ReceiveBroker_MqttConnectionError(object sender, Exception e)
        {
            Log.Warning(e, "Receive Broker MQTT connection error");
        }

        private void SendBroker_MqttConnectionError(object sender, Exception e)
        {
            Log.Warning(e, "Send Broker MQTT connection error");
        }
    }
}