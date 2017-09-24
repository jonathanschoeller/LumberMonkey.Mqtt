using CommandMessenger;
using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;

namespace LumberMonkey.Mqtt
{
    public class CommandMessengerSendBroker : CommandMessengerBroker
    {
        private readonly string m_TopicPrefix;
        private readonly string m_UnknownCommandTopic;

        public CommandMessengerSendBroker(
            CmdMessenger cmdMessenger, MqttOptions mqttOptions, 
            string topicPrefix = "", string unknownCommandTopic = "unknown-cmd", 
            Encoding encoding = null) 
            : base(cmdMessenger, mqttOptions, encoding)
        {
            m_TopicPrefix = topicPrefix;
            m_UnknownCommandTopic = unknownCommandTopic;
        }

        public void Connect()
        {
            // Connect MQTT clients
            ConnectMqttForever($"sender-{Guid.NewGuid()}");

            // Attach Commands to CmdMessenger
            CmdMessenger.Attach(OnUnknownCommand);
            CmdMessenger.Attach((int)Commands.TopicMessage, OnTopicMessageCommand);
        }

        private void OnTopicMessageCommand(ReceivedCommand arguments)
        {
            var topic = arguments.ReadStringArg();
            var message = arguments.ReadStringArg();
            MqttClient.Publish($"{m_TopicPrefix}/{topic}", Encoding.GetBytes(message));
        }

        private void OnUnknownCommand(ReceivedCommand arguments)
        {
            MqttClient.Publish($"{m_TopicPrefix}/{m_UnknownCommandTopic}", Encoding.GetBytes(arguments.RawString));
        }
    }
}
