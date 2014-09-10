using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    [Authorize]
    public class FixedExpensesController : BaseController
    {
        int pageSize = 20;
        [ValidationFunction(ActionName.ViewListFixedExpenses)]
        public ActionResult Index(int? page, int? driverID)
        {
            var list = DB.Entities.FixedExpenses.Where(m => (driverID == null ? true : m.ID == driverID.Value)).OrderByDescending(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_IndexPartial", list);
            }
            SelectOption(new FixedExpenses());
            return View(list);
        }

        void SelectOption(FixedExpenses obj)
        {
            #region SELECT OPTION
            string[] lst = new string[] { "Daily", "Weekly", "Monthly" };
            ViewBag.dataFrequency = CommonFunction.BuildDropdown(lst, lst, obj.Frequency, null);
            #endregion

            var lstfix = DB.Entities.FixedExpenses.ToList();
            ViewBag.dataFilterFixed = CommonFunction.BuildDropdown(lstfix.Select(m=>m.ID.ToString()).ToArray(),
                lstfix.Select(m=>m.Expenses).ToArray(), null, "--Select fixed expenses--");
        }
        [ValidationFunction(ActionName.ViewListFixedExpenses)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = new FixedExpenses();
            if (id != 0) obj = DB.Entities.FixedExpenses.FirstOrDefault(m => m.ID == id);
            SelectOption(obj);
            return View(obj);
        }

        [HttpPost]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(FixedExpenses model, FormCollection frm)
        {
            try
            {
                var db = DB.Entities;
                if (model.ID == 0)
                {
                    // Edit                    
                    db.FixedExpenses.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("FixedExpenses", model);
                    db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                SelectOption(model);
                return View(model);
            }
        }
        [ValidationFunction(ActionName.DeleteItem)]
        public ActionResult Delete(string arrayID = "")
        {
            try
            {
                // TODO: Add delete logic here
                var lstID = arrayID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var db = DB.Entities;
                if (lstID.Length > 0)
                {
                    foreach (var item in lstID)
                    {
                        int tmpID = int.Parse(item);
                        var obj = db.FixedExpenses.FirstOrDefault(m => m.ID == tmpID);
                        db.FixedExpenses.DeleteObject(obj);
                    }
                    db.SaveChanges();
                }
            }
            catch
            {

            }
            return RedirectToAction("Index");
        }
    }
}
