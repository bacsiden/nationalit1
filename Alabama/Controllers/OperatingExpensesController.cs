using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    public class OperatingExpensesController : Controller
    {
        int pageSize = 20;
        public ActionResult Index(int? page, int? driverID)
        {
            var list = DB.Entities.Operating_Expenses.Where(m => (driverID == null ? true : m.ID == driverID.Value)).OrderByDescending(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_IndexPartial", list);
            }
            SelectOption();
            return View(list);
        }

        void SelectOption()
        {
            #region SELECT OPTION
            string dataOperating_Expenses = "<option >--Select Operating_Expenses--</option>";
            foreach (var item in NationalIT.DB.Entities.Operating_Expenses)
            {
                dataOperating_Expenses += string.Format("<option value='{0}'>{0}</option>", item.ID);
            }
            ViewBag.dataOperating_Expenses = dataOperating_Expenses;
            #endregion
        }
        public ActionResult NewOrEdit(int? id = 0)
        {
            if (id == 0) return View(new Operating_Expenses());

            var obj = DB.Entities.Operating_Expenses.FirstOrDefault(m => m.ID == id);
            return View(obj);
        }

        [HttpPost]
        public ActionResult NewOrEdit(Operating_Expenses model)
        {
            try
            {
                var db = DB.Entities;

                if (model.ID == 0)
                {
                    // Edit                    
                    db.Operating_Expenses.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("Operating_Expenses", model);
                    db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

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
                        var obj = db.Operating_Expenses.FirstOrDefault(m => m.ID == tmpID);
                        db.Operating_Expenses.DeleteObject(obj);
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
