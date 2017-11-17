using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace WebSiteFinal
{
    [HubName("ioTSignalRHub")]
    public class IoTSignalRHub : Hub
    {
        private readonly HumidityPusher _pusher;

        public IoTSignalRHub() : this(HumidityPusher.Instance) { }

        public IoTSignalRHub(HumidityPusher pusher)
        {
            _pusher = pusher;
        }

        /*
        public HumidityInfo GetAllHumidityInfos()
        {
            return _pusher.GetAllHumidityInfos();
        }
        */
    }
}