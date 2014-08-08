using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    [Authorize]    
    public class OperatingExpensesController : BaseController
    {
        int pageSize = 20;
        [ValidationFunction(ActionName.ViewListOperatingExpenses)]
        public ActionResult Index(int? page, int? driverID)
        {
            var list = DB.Entities.Operating_Expenses.Where(m => (driverID == null ? true : m.ID == driverID.Value)).OrderByDescending(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_IndexPartial", list);
            }
            SelectOption(new Operating_Expenses());
            return View(list);
        }

        void SelectOption(Operating_Expenses obj)
        {
            #region SELECT OPTION
            string dataOperating_Expenses = "<option >--Select Operating_Expenses--</option>";
            foreach (var item in NationalIT.DB.Entities.Operating_Expenses)
            {
                dataOperating_Expenses += string.Format("<option value='{0}'>{0}</option>", item.ID);
            }
            ViewBag.dataOperating_Expenses = dataOperating_Expenses;
            #endregion

            #region SELECT Driver info
            string dataDriver_Info = "<option >--Select Driver_Info--</option>";
            foreach (var item in NationalIT.DB.Entities.Driver_Info)
            {
                if (obj != null && obj.Driver == item.ID)
                {
                    dataDriver_Info += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
                else
                {
                    dataDriver_Info += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
            }
            ViewBag.dataDriver = dataDriver_Info;
            #endregion
        }
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = new Operating_Expenses() { Current_Payroll = true };
            if (id != 0) obj = DB.Entities.Operating_Expenses.FirstOrDefault(m => m.ID == id);
            SelectOption(obj);
            return View(obj);
        }

        [HttpPost]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(Operating_Expenses model, FormCollection frm)
        {
            try
            {
                var db = DB.Entities;
                model.Date = CommonFunction.ChangeFormatDate(frm["Date"]);
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
