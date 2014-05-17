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

            string dataOwners = "[";
            foreach (var item in Alabama.DB.Entities.Owners)
            {
                if (dataOwners.Equals("["))
                {
                    dataOwners += "{ \"id\": " + item.OwnerID + ", \"label\": \"" + item.Name+ "\" }";
                }
                else
                {
                    dataOwners += ",{ \"id\": " + item.OwnerID + ", \"label\": \"" + item.Name + "\" }";
                }
            }
            ViewBag.dataOwners = dataOwners + "]";

            string dataDispatchers = "[";
            foreach (var item in Alabama.DB.Entities.Dispatchers)
            {
                if (dataDispatchers.Equals("["))
                {
                    dataDispatchers += "{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                }
                else
                {
                    dataDispatchers += ",{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                }
            }
            ViewBag.dataDispatchers = dataDispatchers + "]";

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

        public ActionResult _Fuel_ExpensesPartial(int id, int? page)
        {
            if (page == null) page = 0;
            var db = DB.Entities;
            return PartialView(db.Fuel___Expenses.Where(m => m.Driver_Info.ID == id).OrderByDescending(m => m.ID).ToPagedList(page.Value, pageSize));
        }
        public ActionResult _Operating_ExpensesPartial(int id, int? page)
        {
            if (page == null) page = 0;
            var db = DB.Entities;
            return PartialView(db.Operating_Expenses.Where(m => m.Driver_Info.ID == id).OrderByDescending(m => m.ID).ToPagedList(page.Value, pageSize));
        }
    }
}
