using Topshelf;
using Serilog;

namespace MQTT.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
              .ReadFrom.AppSettings()
              .CreateLogger();

            HostFactory.Run(x =>
            {
                x.Service<MqttBroker>(s =>
                {
                    s.ConstructUsing(name => new MqttBroker());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsNetworkService();

                x.SetDescription("MqttPractice");
                x.SetDisplayName("MqttPractice");
                x.SetServiceName("MqttPractice");
            });
        }
    }
}