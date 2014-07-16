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
            var list = DB.Entities.split_expenses.Where(m=>(driverID == null ? true : m.Id == driverID.Value)).OrderByDescending(m => m.Id).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize);
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
            string dataSplit_Expenses = "<option >--Select Split_Expenses--</option>";            
            foreach (var item in NationalIT.DB.Entities.split_expenses)
            {
                dataSplit_Expenses += string.Format("<option value='{0}'>{0}</option>", item.Id);
            }
            ViewBag.dataSplit_Expenses = dataSplit_Expenses;
            #endregion
        }
        //
        // GET: /Owner/Edit/5

        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.split_expenses.FirstOrDefault(m => m.Id == id);
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        public ActionResult NewOrEdit(split_expenses model)
        {
            try
            {
                var db = DB.Entities;

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
                return View();
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
