using CommandMessenger;
using System;
using System.Text;
using System.Linq;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using CommandMessenger.Queue;
using System.Globalization;
using System.Collections.Generic;

namespace LumberMonkey.Mqtt
{
    /// <summary>
    /// This CommandMessageReceiveBroker will listen for MQTT messages. When it receives one, it will first send a
    /// <see cref="CommandMessengerBroker.Commands.WakeUp"/> command, followed by the <see cref="CommandMessengerBroker.Commands.TopicMessage"/>
    /// </summary>
    public class CommandMessengerReceiveBroker : CommandMessengerBroker
    {
        public CommandMessengerReceiveBroker(CmdMessenger cmdMessenger, MqttOptions mqttOptions, Encoding encoding = null) 
            : base(cmdMessenger, mqttOptions, encoding)
        {
        }

        public virtual void Connect(params string[] topics)
        {
            byte[] qosLevels = topics.AsEnumerable().Select(topic => MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE).ToArray();

            MqttClient.MqttMsgPublishReceived += MqttMsgPublishReceived;

            ConnectMqttForever(
                $"receiver-{Guid.NewGuid()}",
                60000,
                () =>
                {
                    MqttClient.Subscribe(topics, qosLevels);
                });
        }

        protected virtual void MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // This is just to wake up any sleeping device.
            CmdMessenger.SendCommand(new SendCommand((int)Commands.WakeUp));
            System.Threading.Thread.Sleep(50);

            var topicMessageCommand = BuildCommandForMqttMessage(e);
            CmdMessenger.SendCommand(topicMessageCommand);
        }

        protected SendCommand BuildCommandForMqttMessage(MqttMsgPublishEventArgs mqttEventArgs)
        {
            var topicMessageCommand = new SendCommandMeasurable((int)Commands.TopicMessage);

            topicMessageCommand.AddArgument(mqttEventArgs.Topic);
            topicMessageCommand.AddArgument(Encoding.GetString(mqttEventArgs.Message));

            if (IsCommandTooBig(topicMessageCommand))
                throw new InvalidOperationException("Command too big");

            return topicMessageCommand;
        }

        /// <summary>
        /// This is unfortunate, but it looks like the arduino C++ lib
        /// limits the size of commands.
        /// </summary>
        private bool IsCommandTooBig(SendCommandMeasurable command)
        {
            return command.CommandStringLength >= 64;
        }

        /// <summary>
        /// This is a hack to let us detect when we're creating a command that's too big to be sent
        /// using CmdMessenger. It gets us access to the protected property CmdArgs which we can
        /// use to calculate the serialized size of the command.
        /// </summary>
        private class SendCommandMeasurable : SendCommand
        {
            public SendCommandMeasurable(int cmdId) : base(cmdId)
            {
            }

            public new void AddArgument(string arg)
            {
                base.AddArgument(arg);
                CmdArgs.Add(arg);
            }

            public int CommandStringLength
            {
                get
                {
                    var commandString = new StringBuilder(CmdId.ToString(CultureInfo.InvariantCulture));

                    foreach (var argument in Arguments)
                    {
                        commandString.Append(' ').Append(argument);
                    }
                    commandString.Append(' ');

                    return commandString.ToString().Length;
                }
            }
        }
    }
}
