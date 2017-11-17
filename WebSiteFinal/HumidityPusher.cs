using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace WebSiteFinal
{
    public class HumidityPusher
    {
        // Singleton instance
        private readonly static Lazy<HumidityPusher> _instance = new Lazy<HumidityPusher>(() => new HumidityPusher(GlobalHost.ConnectionManager.GetHubContext<IoTSignalRHub>().Clients));

        public static HumidityPusher Instance { get { return _instance.Value; } }

        private IHubConnectionContext<dynamic> Clients { get; set; }

        // nur zum testen
        //private readonly ConcurrentDictionary<string, HumidityInfo> _humidityInfos = new ConcurrentDictionary<string, HumidityInfo>();

        static string connectionString = "HostName=VtIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=Hc7T95hsBsesDH/SaRBXJa2yNYzLc7qa4ptoq3q1aPE=";

        static EventHubClient eventHubClient;

        private HumidityPusher(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

            /*
            _humidityInfos.Clear();
            var stocks = new List<HumidityInfo>
            {
                new HumidityInfo { Device = "GOOG", Humidity = "570.30" }
            };
            stocks.ForEach(info => _humidityInfos.TryAdd(info.Device, info));
            */

            // TODO: hier IoTHubListener implementieren
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, "messages/events");
            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            List<Task> tasks = new List<Task>();
            foreach (string partition in d2cPartitions)
            {
                //tasks.Add(ReceiveMessagesFromDeviceAsync(partition));
                Task.Run(() => ReceiveMessagesFromDeviceAsync(partition));
            }
            //Task.WhenAll(tasks.ToArray());
        }

        private async Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
            while (true)
            {
                try
                {
                    EventData eventData = await eventHubReceiver.ReceiveAsync();
                    if (eventData == null) continue;
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());
                    HumidityInfo info = JsonConvert.DeserializeObject<HumidityInfo>(data);

                    Clients.All.updateHumidity(info);
                } catch { }
            }
        }

        /*
        public HumidityInfo GetAllHumidityInfos()
        {
            return _humidityInfos["GOOG"];
        }
        */
    }
}