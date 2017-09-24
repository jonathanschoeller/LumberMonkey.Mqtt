using CommandMessenger;
using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace LumberMonkey.Mqtt
{
    public abstract class CommandMessengerBroker
    {
        protected enum Commands
        {
            TopicMessage,
            WakeUp,
            SendBatch,  // When received, instructs the CommandMessengerReceiveBrokerBuffered to send a batch.
            BatchDone   // Send when the CommandMessengerReceiveBrokerBuffered has finished sending a batch.
        }

        protected readonly MqttClient MqttClient;
        protected readonly CmdMessenger CmdMessenger;
        protected readonly Encoding Encoding;

        public CommandMessengerBroker(CmdMessenger cmdMessenger, MqttOptions mqttOptions, Encoding encoding = null)
        {
            CmdMessenger = cmdMessenger;
            MqttClient = mqttOptions.CreateMqttClient();
            MqttClient.ConnectionClosed += (sender, e) => MqttConnectionClosed?.Invoke(sender, e);

            Encoding = encoding ?? Encoding.UTF8;
        }

        protected void ConnectMqttForever(string clientId, int millisecondsSleepTimeout = 60000, Action afterConnect = null)
        {
            Action connectMqtt = () =>
            {
                while (true)
                {
                    try
                    {
                        MqttClient.Connect(clientId);
                        afterConnect?.Invoke();

                        break;
                    }
                    catch (MqttCommunicationException ex)
                    {
                        MqttConnectionError?.Invoke(this, ex);
                    }
                    catch (MqttConnectionException ex)
                    {
                        MqttConnectionError?.Invoke(this, ex);
                    }
                    catch (MqttClientException ex)
                    {
                        MqttConnectionError?.Invoke(this, ex);
                        System.Threading.Thread.Sleep(millisecondsSleepTimeout);
                    }
                }
            };

            connectMqtt();

            // Re-connection disconnect
            MqttClient.ConnectionClosed += (sender, e) => connectMqtt();
        }

        public delegate void MqttExceptionEventHandler(object sender, Exception e);

        public event EventHandler MqttConnectionClosed;
        public event MqttExceptionEventHandler MqttConnectionError;
    }
}
