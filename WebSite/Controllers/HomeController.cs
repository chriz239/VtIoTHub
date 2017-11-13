using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace WebSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public HttpResponseMessage RegisterWebSocket()
        {
            return new HttpResponseMessage();
        }

        public class MyWebSocketHandler 
        {
            


        }
    }
}