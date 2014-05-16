using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace Alabama.Controllers
{
    public class DriverInfoController : Controller
    {
        int pageSize = 20;

        public ActionResult Index(int? page)
        {
            return View(DB.Entities.Driver_Info.OrderByDescending(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }

        public ActionResult NewOrEdit(int? id = 0)
        {
            if (id == 0) return View(new Driver_Info());

            var obj = DB.Entities.Driver_Info.FirstOrDefault(m => m.ID == id);
            return View(obj);
        }

        [HttpPost]
        public ActionResult NewOrEdit(Driver_Info model)
        {
            try
            {
                var db = DB.Entities;

                if (model.ID == 0)
                {
                    // Edit                    
                    db.Driver_Info.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("Driver_Info", model);
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
                        var obj = db.Driver_Info.FirstOrDefault(m => m.ID == tmpID);
                        db.Driver_Info.DeleteObject(obj);
                    }
                    db.SaveChanges();
                }
            }
            catch
            {

            }
            return RedirectToAction("Index");
        }

        public ActionResult _Fuel_ExpensesPartial(int driver_infoID, int?page)
        {
            if (page == null) page = 0;
            var db = DB.Entities;
            return PartialView(db.Fuel___Expenses.Where(m => m.Driver_Info.ID == driver_infoID).OrderByDescending(m => m.ID).ToPagedList(page.Value, pageSize));
        }
        public ActionResult _Operating_ExpensesPartial(int driver_infoID, int? page)
        {
            if (page == null) page = 0;
            var db = DB.Entities;
            return PartialView(db.Operating_Expenses.Where(m => m.Driver_Info.ID == driver_infoID).OrderByDescending(m => m.ID).ToPagedList(page.Value, pageSize));
        }
    }
}
