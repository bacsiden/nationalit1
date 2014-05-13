using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
namespace Alabama.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index(int? page)
        {
            if (page == null) page = 0;
            var db = DB.Entities;
            var lst = db.Trip_Info;
            return View(lst.OrderBy(m=>m.Trip_ID).ToPagedList(page.Value, 30));
        }

    }
}
