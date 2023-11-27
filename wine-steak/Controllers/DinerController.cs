using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace wine_steak.Controllers
{
    public class DinerController : Controller
    {
        // GET: Diner
        public ActionResult Index()
        {
            return View();
        }
    }
}