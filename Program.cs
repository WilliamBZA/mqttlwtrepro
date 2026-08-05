using System;
using System.Device.Wifi;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.Networking;

namespace MqttLWTRepro
{
    public class Program
    {
        private const string WifiSsid = "";
        private const string WifiPassword = "";

        private const string BrokerAddress = "";
        private const int BrokerPort = 1883;
        private const string BrokerUsername = "";
        private const string BrokerPassword = "";

        private const ushort KeepAlivePeriodSeconds = 60;

        private const string WillTopic = "mqttlwtrepro/status";
        private const string WillMessage = "offline";
        private const string BirthMessage = "online";
        private const MqttQoSLevel WillQosLevel = MqttQoSLevel.AtLeastOnce;

        private const string PayloadTopic = "mqttlwtrepro/payload";
        private const int PayloadIntervalMs = 10000;

        public static void Main()
        {
            var clientId = "MqttLWTRepro" + Guid.NewGuid().ToString(); // uncomment this to use a dynamic clientId instead of a static one
            // var clientId = "StaticMqttLWTRepro"; // uncomment this line to use a static clientId instead of a random one

            WifiNetworkHelper.ConnectDhcp(
                WifiSsid,
                WifiPassword,
                WifiReconnectionKind.Automatic,
                true,
                0,
                new CancellationTokenSource(60000).Token);

            string ipAddress = NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address;
            Debug.WriteLine("IP address=" + ipAddress);

            MqttClient client = new MqttClient(BrokerAddress, BrokerPort, false, null, null, MqttSslProtocols.None);

            MqttReasonCode result = client.Connect(
                clientId,
                BrokerUsername,
                BrokerPassword,
                true,
                WillQosLevel,
                true,
                WillTopic,
                WillMessage,
                true,
                KeepAlivePeriodSeconds);

            Debug.WriteLine("CONNACK=" + (int)result + " clientId=" + clientId);

            client.Publish(WillTopic, Encoding.UTF8.GetBytes(BirthMessage), null, null, WillQosLevel, true);

            var random = new Random();

            while (true)
            {
                string payload = random.Next().ToString();
                client.Publish(PayloadTopic, Encoding.UTF8.GetBytes(payload));
                Debug.WriteLine("Published " + payload + " to " + PayloadTopic);

                Thread.Sleep(PayloadIntervalMs);
            }
        }
    }
}
