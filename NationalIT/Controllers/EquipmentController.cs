using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    [Authorize]
    public class EquipmentController : BaseController
    {
        int pageSize = 20;
        //
        // GET: /Owner/
        [ValidationFunction(ActionName.ViewListEquipment)]
        public ActionResult Index(int? page, string message="")
        {
            ViewBag.Error = message;
            return View(DB.Entities.Equipment.OrderBy(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }
        //
        // GET: /Owner/Edit/5
        [ValidationFunction(ActionName.ViewListEquipment)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var db = DB.Entities;
            var obj = db.Equipment.FirstOrDefault(m => m.ID == id);
            if (obj == null) obj = new Equipment() { InspectionFrequency = 30, LastInspected = DateTime.Now};
            var lstDriver = db.Driver_Info.ToList();
            string dataDriver = CommonFunction.BuildDropdown(lstDriver.Select(m => m.ID.ToString()).ToArray(),
                lstDriver.Select(m => m.First_name + " " + m.Last_name).ToArray(), obj.Driver, "--Select Driver--");
            ViewBag.dataDriver = dataDriver;

            var lstOwner = db.Owners.ToList();
            ViewBag.dataOwner = CommonFunction.BuildDropdown(lstOwner.Select(m => m.OwnerID.ToString()).ToArray(),
                lstOwner.Select(m => m.Name).ToArray(), obj.Owner, "--Select Owner--");

            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(Equipment model, FormCollection frm)
        {
            var db = DB.Entities;
            try
            {
                model.Inspection_Expiration = CommonFunction.ChangeFormatDate(frm["Inspection_Expiration"]);
                model.Registration_Expiration = CommonFunction.ChangeFormatDate(frm["Registration_Expiration"]);
                if (model.ID == 0)
                {
                    // Edit                    
                    db.Equipment.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("Equipment", model);
                    db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var lstDriver = db.Driver_Info.ToList();
                string dataDriver = CommonFunction.BuildDropdown(lstDriver.Select(m => m.ID.ToString()).ToArray(),
                    lstDriver.Select(m => m.First_name + " " + m.Last_name).ToArray(), model.Driver, "--Select Driver--");
                ViewBag.dataDriver = dataDriver;

                var lstOwner = db.Owners.ToList();
                ViewBag.dataOwner = CommonFunction.BuildDropdown(lstOwner.Select(m => m.OwnerID.ToString()).ToArray(),
                    lstOwner.Select(m => m.Name).ToArray(), model.Owner, "--Select Owner--");
                return View();
            }
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
                        var obj = db.Equipment.FirstOrDefault(m => m.ID == tmpID);
                        db.Equipment.DeleteObject(obj);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {                
                return RedirectToAction("Index", new {message="Can not delete this equipment because it is related to table: violation, trip_info,..."});
            }
            return RedirectToAction("Index");
        }

    }
}
