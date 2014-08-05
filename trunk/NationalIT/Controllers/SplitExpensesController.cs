using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    public class SplitExpensesController : Controller
    {
        int pageSize = 20;
        //
        // GET: /Owner/

        public ActionResult Index(int? page, int? driverID)
        {
            var list = DB.Entities.split_expenses.Where(m => (driverID == null ? true : m.Id == driverID.Value)).OrderByDescending(m => m.Id).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_IndexPartial", list);
            }
            SelectOption(new split_expenses());
            return View(list);
        }

        void SelectOption(split_expenses obj)
        {
            #region SELECT OPTION
            string dataSplit_Expenses = "<option >--Select Split_Expenses--</option>";
            foreach (var item in NationalIT.DB.Entities.split_expenses)
            {
                dataSplit_Expenses += string.Format("<option value='{0}'>{0}</option>", item.Id);
            }
            ViewBag.dataSplit_Expenses = dataSplit_Expenses;
            #endregion

            #region SELECT Driver info
            string dataDriver_Info = "<option >--Select Driver_Info--</option>";
            foreach (var item in NationalIT.DB.Entities.Driver_Info)
            {
                if (obj != null && obj.Idndex == 1 && obj.OwnerDriver == item.ID)
                {
                    dataDriver_Info += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
                else
                {
                    dataDriver_Info += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
            }
            ViewBag.dataDriver = dataDriver_Info;

            string dataOwner_Info = "<option >--Select Owner_Info--</option>";
            foreach (var item in NationalIT.DB.Entities.Owners)
            {
                if (obj != null && obj.Idndex == 2 && obj.OwnerDriver == item.OwnerID)
                {
                    dataOwner_Info += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.OwnerID, item.Name);
                }
                else
                {
                    dataOwner_Info += string.Format("<option value='{0}'>{1}</option>", item.OwnerID, item.Name);
                }
            }
            ViewBag.dataOwner = dataOwner_Info;
            #endregion
        }
        //
        // GET: /Owner/Edit/5

        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.split_expenses.FirstOrDefault(m => m.Id == id);
            SelectOption(obj);
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        public ActionResult NewOrEdit(split_expenses model, FormCollection frm)
        {
            try
            {
                var db = DB.Entities;
                model.Date = CommonFunction.ChangeFormatDate(frm["Date"]);
                if (model.Idndex == 2)
                {
                    model.OwnerDriver = int.Parse(frm["OwnerDriver2"]);
                }
                if (model.Id == 0)
                {
                    // Edit                    
                    db.split_expenses.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("split_expenses", model);
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

        //
        // GET: /Owner/Delete/5

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
                        var obj = db.split_expenses.FirstOrDefault(m => m.Id == tmpID);
                        db.split_expenses.DeleteObject(obj);
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
