using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace WebJob
{
    class Program
    {
        static string connectionString = "HostName=VtIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=Hc7T95hsBsesDH/SaRBXJa2yNYzLc7qa4ptoq3q1aPE=";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        static void Main(string[] args)
        {
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, "messages/events");

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            var tasks = new List<Task>();
            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static async Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
            while (true)
            {
                try
                {

                    EventData eventData = await eventHubReceiver.ReceiveAsync();
                    if (eventData == null) continue;

                    // Convert to object
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());
                    HumidityInfo info = JsonConvert.DeserializeObject<HumidityInfo>(data);

#if DEBUG
                    var hubConnection = new HubConnection("http://localhost:51943/");
#else
                    var hubConnection = new HubConnection("http://vtiothubwebsite.azurewebsites.net/");
#endif
                    IHubProxy hubProxy = hubConnection.CreateHubProxy("ioTSignalRHub");
                    hubConnection.Start().Wait();
                    hubProxy.Invoke("updateHumidityInfos", info).Wait();
                    

                    /*
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());
                    Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);
                    */

                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
