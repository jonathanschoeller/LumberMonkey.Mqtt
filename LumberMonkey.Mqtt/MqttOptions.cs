using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace LumberMonkey.Mqtt
{
    public class MqttOptions
    {
        public string BrokerHostName { get; set; }
        public int BrokerPort { get; set; } = 8883;
        public bool Secure { get; set; } = true;
        public X509Certificate CaCert { get; set; }
        public X509Certificate2 ClientCert { get; set; }
        public SslProtocols SslProtocol { get; set; } = SslProtocols.Tls12;
    }
}
