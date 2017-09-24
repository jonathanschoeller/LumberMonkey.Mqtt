using CommandMessenger;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Concurrent;
using System;

namespace LumberMonkey.Mqtt
{
    /// <summary>
    /// This <see cref="CommandMessengerReceiveBroker"/> will receive MQTT messages and buffer them until it is instructed
    /// to send them.
    /// </summary>
    public class CommandMessengerReceiveBrokerBuffered : CommandMessengerReceiveBroker
    {
        private readonly ConcurrentQueue<SendCommand> m_CommandQueue = new ConcurrentQueue<SendCommand>();

        public CommandMessengerReceiveBrokerBuffered(CmdMessenger cmdMessenger, MqttOptions mqttOptions, Encoding encoding = null)
            : base(cmdMessenger, mqttOptions, encoding)
        {
        }

        public override void Connect(params string[] topics)
        {
            base.Connect(topics);

            CmdMessenger.Attach((int)Commands.SendBatch, OnSendBatchCommand);
        }

        protected override void MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            SendCommand topicMessageCommand = null;

            try
            {
                topicMessageCommand = BuildCommandForMqttMessage(e);
            }
            catch(InvalidOperationException ex)
            {
                // TODO: Log it.
                throw;
            }

            if (topicMessageCommand != null)
            {
                m_CommandQueue.Enqueue(topicMessageCommand);
            }
        }

        private void OnSendBatchCommand(ReceivedCommand arguments)
        {
            SendAll();
        }

        private void SendAll()
        {
            while (m_CommandQueue.TryDequeue(out SendCommand topicMessageCommand))
            {
                CmdMessenger.SendCommand(topicMessageCommand);
            }

            CmdMessenger.SendCommand(new SendCommand((int)Commands.BatchDone));
        }
    }
}
