using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using System.Data.Objects.SqlClient;

namespace Alabama.Controllers
{
    public class TripInfoController : Controller
    {
        int pageSize = 20;
        //
        // GET: /Owner/

        public ActionResult Index(int? page)
        {
            return View(DB.Entities.Trip_Info.OrderByDescending(m => m.Trip_ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }
        //
        // GET: /Owner/Edit/5

        public ActionResult NewOrEdit(int? driverID = 0, int? id = 0)
        {
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

            string dataDriver = "[";
            foreach (var item in Alabama.DB.Entities.Driver_Info)
            {
                if (dataDriver.Equals("["))
                {
                    dataDriver += "{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                }
                else
                {
                    dataDriver += ",{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                }
            }
            ViewBag.dataDriver = dataDriver + "]";


            var obj = DB.Entities.Trip_Info.FirstOrDefault(m => m.Trip_ID == id);
            if (obj == null)
            {
                obj = new Trip_Info() { Picked = true, Deliverd = false, Customer_Invoiced = false, Current_Payroll = false, Driver_paid = false, Driver = driverID.Value };
            }
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        public ActionResult NewOrEdit(Trip_Info model)
        {
            try
            {
                var db = DB.Entities;

                if (model.Trip_ID == 0)
                {
                    // Edit                    
                    db.Trip_Info.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("Trip_Info", model);
                    db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
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

                string dataDriver = "[";
                foreach (var item in Alabama.DB.Entities.Driver_Info)
                {
                    if (dataDriver.Equals("["))
                    {
                        dataDriver += "{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                    }
                    else
                    {
                        dataDriver += ",{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                    }
                }
                ViewBag.dataDriver = dataDriver + "]";
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
                        var obj = db.Trip_Info.FirstOrDefault(m => m.Trip_ID == tmpID);
                        db.Trip_Info.DeleteObject(obj);
                    }
                    db.SaveChanges();
                }
            }
            catch
            {

            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult GetCutomerInfoByKey(int? page, string query = "")
        {
            page = page.HasValue ? page.Value : 1;
            int id = 0;
            int.TryParse(query, out id);
            var db = Alabama.DB.Entities;
            var list = db.Customer_Info.Where(m =>
                m.Customer_Name.Contains(query) || (id == 0) ? true : m.Customer_ID == id);
            return PartialView(list.OrderByDescending(m => m.Customer_ID).ToPagedList(page.Value, 5));
        }
    }
}
