using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebSiteWithSignalR.Hubs;

namespace WebSiteWithSignalR.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public string Add(string message)
        {
            IoTSignalRHub hub = new IoTSignalRHub();
            hub.Send("Von außen", message);
            return "irgendwas";
        }
    }
}