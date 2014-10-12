using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NationalIT.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {
        [ValidationFunction(ActionName.ViewAllReport)]
        public ActionResult Index()
        {
            #region SELECT OPTION
            var db = DB.Entities;
            ViewBag.dataDriver = CommonFunction.BuildDropdown(db.Driver_Info.Select(m => m.ID).ToArray(),
                db.Driver_Info.Select(m => m.Last_name + "" + m.First_name).ToArray(), null, "--All Driver--");

            ViewBag.dataDispatchers = CommonFunction.BuildDropdown(db.Dispatchers.Select(m => m.ID).ToArray(),
                db.Dispatchers.Select(m => m.Last_name + "" + m.First_name).ToArray(), null, "--All Dispatcher--");

            var lstCompany = db.Company.ToList();
            ViewBag.ListCompany = CommonFunction.BuildDropdown(lstCompany.Select(m => m.ID.ToString()).ToArray(),
                lstCompany.Select(m => m.Name).ToArray(), null, "--All Company--");
            #endregion
            return View();
        }
    }
}
