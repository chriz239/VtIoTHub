using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebJob
{
    public class HumidityInfo
    {
        public string Device { get; set; }
        public string Humidity { get; set; }

        public HumidityInfo() { }

        public HumidityInfo(string device, string humidity)
        {
            this.Device = device;
            this.Humidity = humidity;
        }
    }
}