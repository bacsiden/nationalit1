using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    public class FixedChargesController : BaseController
    {
        int pageSize = 50;
        //
        // GET: /Owner/

        [ValidationFunction(ActionName.ViewListFixedCharges)]
        public ActionResult Index(int? page, int driverID = 0)
        {
            var lst = DB.Entities.FixedCharges.Where(m => driverID == 0 ? true : m.DriverID == driverID).OrderBy(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize);
            if (driverID != 0)
            {
                var objDriver = DB.Entities.Driver_Info.FirstOrDefault(m => m.ID == driverID);
                ViewBag.DriverName = objDriver != null ? objDriver.Last_name + " " + objDriver.First_name : "";
            }
            return View(lst);
        }
        //
        // GET: /Owner/Edit/5
        [ValidationFunction(ActionName.ViewListFixedCharges)]
        public ActionResult NewOrEdit(int? driverID, int? id = 0)
        {
            var obj = DB.Entities.FixedCharges.FirstOrDefault(m => m.ID == id);
            if (obj == null)
            {
                obj = new FixedCharges() { DriverID = driverID.HasValue ? driverID.Value : 0 };
            }
            SelectOption(obj);
            return View(obj);
        }
        void SelectOption(FixedCharges obj)
        {

            #region SELECT OPTION
            string dataDriver_Info = "<option value=''>--Select Driver_Info--</option>";
            foreach (var item in NationalIT.DB.Entities.Driver_Info)
            {
                if (obj.DriverID.HasValue && item.ID == obj.DriverID.Value)
                {
                    dataDriver_Info += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
                else
                {
                    dataDriver_Info += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
            }

            ViewBag.dataDriver_Info = dataDriver_Info;
            string dataFrequency = "<option value='0'>-:-</option>";
            foreach (var item in NationalIT.DB.Entities.Frequency)
            {
                if (obj.Frequency != 0)
                {
                    dataFrequency += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.ID, item.Title);
                }
                else
                {
                    dataFrequency += string.Format("<option value='{0}'>{1}</option>", item.ID, item.Title);
                }
            }

            ViewBag.dataFrequency = dataFrequency;

            #endregion
        }

        //
        // POST: /Owner/Edit/5
        [ValidationFunction(ActionName.NewOrEditItem)]
        [HttpPost]
        public ActionResult NewOrEdit(FixedCharges model)
        {
            try
            {
                var db = DB.Entities;

                if (model.ID == 0)
                {
                    // Edit                    
                    db.FixedCharges.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("FixedCharges", model);
                    db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {

            }
            SelectOption(model);
            return View(model);
        }

        //
        // GET: /Owner/Delete/5
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
                        var obj = db.FixedCharges.FirstOrDefault(m => m.ID == tmpID);
                        db.FixedCharges.DeleteObject(obj);
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
