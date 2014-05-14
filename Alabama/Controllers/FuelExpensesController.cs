﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using Alabama.Models;
namespace Alabama.Controllers
{
    public class FuelExpensesController : Controller
    {
        public ActionResult Index(int? page)
        {
            if (page == null) page = 0;
            var db = DB.Entities;
            return View(db.Fuel___Expenses.OrderBy(m => m.ID).ToPagedList(page.Value, 30));
        }

        public ActionResult Create()
        {
            var lst1 = new List<string> { "FUEL", "ADVANCE", "REPAIR" };
            string type = "";
            foreach (var item in lst1)
            {
                type += "<option value='" + item + "'>" + item + "</option>";
            }
            ViewBag.Type = type;

            List<Driver_Info> lst = new List<Driver_Info>();
            lst.AddRange(DB.Entities.Driver_Info);
            var driver = new SelectList(lst, "ID", "First_name");
            ViewBag.Driver = driver;

            return View();
        }

        [HttpPost]
        public ActionResult Create(Fuel___Expenses model, FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here
                var db = DB.Entities;
                model.Type = collection["cboType"];
                if (collection["Driver_Info.ID"] != null)
                {
                    model.Driver = int.Parse(collection["Driver_Info.ID"]);
                }
                db.Fuel___Expenses.AddObject(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            var db = DB.Entities;
            var obj = db.Fuel___Expenses.FirstOrDefault(m => m.ID == id);
            var lst1 = new List<string> { "FUEL", "ADVANCE", "REPAIR" };
            string type = "";
            foreach (var item in lst1)
            {
                string x = "";
                if (item == obj.Type)
                    x = "selected='selected'";
                type += "<option value='" + item + "' " + x + ">" + item + "</option>";
            }
            ViewBag.Type = type;

            List<Driver_Info> lst = new List<Driver_Info>();
            lst.AddRange(db.Driver_Info);
            var driver = new SelectList(lst, "ID", "First_name", obj.Driver);
            ViewBag.Driver = driver;

            return View(obj);
        }

        [HttpPost]
        public ActionResult Edit(Fuel___Expenses model, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
                model.Type=collection["cboType"];
                if (collection["Driver_Info.ID"] != null)
                {
                    model.Driver = int.Parse(collection["Driver_Info.ID"]);
                }
                var db = DB.Entities;
               // db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                db.AttachTo("Fuel___Expenses", model);
                db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
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
                        var obj = db.Fuel___Expenses.FirstOrDefault(m => m.ID == tmpID);
                        db.Fuel___Expenses.DeleteObject(obj);
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }

        }
    }
}
