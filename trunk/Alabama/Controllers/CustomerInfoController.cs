﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace Alabama.Controllers
{
    public class CustomerInfoController : Controller
    {
        int pageSize = 20;
        //
        // GET: /Owner/

        public ActionResult Index(int? page)
        {
            return View(DB.Entities.Customer_Info.OrderByDescending(m => m.Customer_ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }
        //
        // GET: /Owner/Edit/5

        public ActionResult NewOrEdit(int? id = 0)
        {
            if (id == 0) return View(new Customer_Info());
            var obj = DB.Entities.Customer_Info.FirstOrDefault(m => m.Customer_ID == id);
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        public ActionResult NewOrEdit(Customer_Info model)
        {
            try
            {
                var db = DB.Entities;

                if (model.Customer_ID == 0)
                {
                    // Edit                    
                    db.Customer_Info.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("Customer_Info", model);
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
                        var obj = db.Customer_Info.FirstOrDefault(m => m.Customer_ID == tmpID);
                        db.Customer_Info.DeleteObject(obj);
                    }
                    db.SaveChanges();
                }
            }
            catch
            {

            }
            return RedirectToAction("Index");
        }

        public ActionResult _Trip_Info_Partial(int id, int? page)
        {
            if (page == null) page = 0;
            var db = DB.Entities;
            return PartialView(db.Trip_Info.Where(m => m.Customer_Info.Customer_ID == id).OrderByDescending(m => m.Trip_ID).ToPagedList(page.Value, pageSize));
        }

    }
}
