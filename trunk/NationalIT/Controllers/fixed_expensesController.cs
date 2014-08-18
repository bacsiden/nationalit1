using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    public class fixed_expensesController : BaseController
    {
        int pageSize = 20;
        //
        // GET: /Owner/

        [ValidationFunction(ActionName.ViewListFixedCharges)]
        public ActionResult Index(int? page)
        {
            return View(DB.Entities.fixed_charges.OrderByDescending(m => m.Id).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }
        //
        // GET: /Owner/Edit/5
        [ValidationFunction(ActionName.ViewListFixedCharges)]
        public ActionResult NewOrEdit(int? driverID, int? id = 0)
        {
            var obj = DB.Entities.fixed_charges.FirstOrDefault(m => m.Id == id);
            if (obj == null)
            {
                obj = new fixed_charges() { DriverId = driverID.HasValue ? driverID.Value : 0 };
            }
            SelectOption(obj);
            return View(obj);
        }
        void SelectOption(fixed_charges obj)
        {
            #region SELECT OPTION
            string dataDriver_Info = "<option >--Select Driver_Info--</option>";
            foreach (var item in NationalIT.DB.Entities.Driver_Info)
            {
                if (obj.DriverId.HasValue && item.ID == obj.DriverId.Value)
                {
                    dataDriver_Info += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
                else
                {
                    dataDriver_Info += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }                
            }
            ViewBag.dataDriver_Info = dataDriver_Info;
            #endregion
        }

        //
        // POST: /Owner/Edit/5
        [ValidationFunction(ActionName.NewOrEditItem)]
        [HttpPost]
        public ActionResult NewOrEdit(fixed_charges model)
        {
            try
            {
                var db = DB.Entities;

                if (model.Id == 0)
                {
                    // Edit                    
                    db.fixed_charges.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("fixed_charges", model);
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
                        var obj = db.fixed_charges.FirstOrDefault(m => m.Id == tmpID);
                        db.fixed_charges.DeleteObject(obj);
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
