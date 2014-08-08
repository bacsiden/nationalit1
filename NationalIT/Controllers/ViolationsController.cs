﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    [Authorize]    
    public class ViolationsController : BaseController
    {
        int pageSize = 20;
        //
        // GET: /Owner/
        [ValidationFunction(ActionName.ViewListVolations)]
        public ActionResult Index(int? page)
        {
            return View(DB.Entities.Violations.OrderByDescending(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }
        //
        // GET: /Owner/Edit/5
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.Violations.FirstOrDefault(m => m.ID == id);
            #region SELECT OPTION
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
            ViewBag.dataDriver_Info = dataDriver_Info;

            string dataEquipment = "<option >--Select Equiment--</option>";
            foreach (var item in NationalIT.DB.Entities.Equipment)
            {
                if (obj != null && obj.Equipment == item.ID)
                {
                    dataEquipment += string.Format("<option value='{0}' selected='selected'>{1} - {2}</option>", item.ID, item.Equipment_number, item.Equipment_Type);
                }
                else
                {
                    dataEquipment += string.Format("<option value='{0}'>{1} - {2}</option>", item.ID, item.Equipment_number, item.Equipment_Type);
                }
            }
            ViewBag.dataEquipment = dataEquipment;
            #endregion
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(Violations model, FormCollection frm)
        {
            try
            {
                var db = DB.Entities;
                model.Date_occurred = CommonFunction.ChangeFormatDate(frm["Date_occurred"]);
                model.Date_resolved = CommonFunction.ChangeFormatDate(frm["Date_resolved"]);
                if (model.ID == 0)
                {
                    // Edit    

                    db.Violations.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("Violations", model);
                    db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                #region SELECT OPTION
                string dataDriver_Info = "<option >--Select Driver_Info--</option>";
                foreach (var item in NationalIT.DB.Entities.Driver_Info)
                {
                    if (model != null && model.Driver == item.ID)
                    {
                        dataDriver_Info += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                    }
                    else
                    {
                        dataDriver_Info += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                    }
                }
                ViewBag.dataDriver_Info = dataDriver_Info;

                string dataEquipment = "<option >--Select Equiment--</option>";
                foreach (var item in NationalIT.DB.Entities.Equipment)
                {
                    if (model != null && model.Equipment == item.ID)
                    {
                        dataEquipment += string.Format("<option value='{0}' selected='selected'>{1} - {2}</option>", item.ID, item.Equipment_number, item.Equipment_Type);
                    }
                    else
                    {
                        dataEquipment += string.Format("<option value='{0}'>{1} - {2}</option>", item.ID, item.Equipment_number, item.Equipment_Type);
                    }
                }
                ViewBag.dataEquipment = dataEquipment;
                #endregion
                return View(model);
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
                        var obj = db.Violations.FirstOrDefault(m => m.ID == tmpID);
                        db.Violations.DeleteObject(obj);
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
