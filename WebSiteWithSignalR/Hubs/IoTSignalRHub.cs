using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace WebSiteWithSignalR.Hubs
{
    public class IoTSignalRHub : Hub
    {
        public void Send(string device, string humidity)
        {
            Clients.All.addNewMessageToPage(device, humidity);
        }
    }
}