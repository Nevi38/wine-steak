using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wine_steak.Models;

namespace wine_steak.Controllers
{
    public class FoodsDayController : Controller
    {
        private dbDataContext db = new dbDataContext();
       
        // GET: FoodsDay
        [HttpGet]
        public ActionResult Index(int? currentFood)
        {
            int countFood = db.MonAns.ToList().Count;
            if (currentFood == null)
            {
                currentFood = 1;
            }
            if (currentFood > countFood)
            {
                currentFood = 1;
            }
            var result = (from food in db.MonAns where food.id == currentFood select food).SingleOrDefault();
            Session["nextFood"] = currentFood + 1;
            return View(result);
        }
    }
}