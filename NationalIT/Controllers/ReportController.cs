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
        //
        // GET: /Report/
        [ValidationFunction(ActionName.ViewAllReport)]
        public ActionResult Index()
        {
            SelectOption();
            return View();
        }
        void SelectOption()
        {
            var db = DB.Entities;
            #region SELECT OPTION
            string dataDriver_Info = "<option value='' >--Select Driver_Info--</option>";
            foreach (var item in db.Driver_Info)
            {
                dataDriver_Info += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
            }
            ViewBag.dataDriver = dataDriver_Info;

            string dataDispatchers = "<option value='' >--Select Dispatchers--</option>";
            foreach (var item in db.Dispatchers)
            {
                dataDispatchers += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
            }
            ViewBag.dataDispatchers = dataDispatchers;

            var lstCompany = db.Company.ToList();
            ViewBag.ListCompany = CommonFunction.BuildDropdown(lstCompany.Select(m => m.ID.ToString()).ToArray(),
                lstCompany.Select(m => m.Name).ToArray(), null , "--Select Company--");
            #endregion
        }

    }
}
