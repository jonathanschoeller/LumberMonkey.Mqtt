using System;
using System.Security.Authentication;
using uPLibrary.Networking.M2Mqtt;

namespace LumberMonkey.Mqtt
{
    public static class M2MqttExtensions
    {
        public static MqttSslProtocols ToMqtt(this SslProtocols sslProtocol)
        {
            switch(sslProtocol)
            {
                case SslProtocols.Ssl3: return MqttSslProtocols.SSLv3;
                case SslProtocols.Tls: return MqttSslProtocols.TLSv1_0;
                case SslProtocols.Tls11: return MqttSslProtocols.TLSv1_1;
                case SslProtocols.Tls12: return MqttSslProtocols.TLSv1_2;
                default: throw new NotSupportedException($"Unsupported SSL protocol: {sslProtocol}");
            }
        }

        public static MqttClient CreateMqttClient(this MqttOptions options)
        {
            return new MqttClient(
                options.BrokerHostName,
                options.BrokerPort,
                options.Secure,
                options.CaCert,
                options.ClientCert,
                options.SslProtocol.ToMqtt());
        }
    }
}
