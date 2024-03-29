﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
namespace NationalIT.Controllers
{
    [Authorize]
    public class FuelExpensesController : BaseController
    {
        public string Test()
        {
            string filepath = "";
            try
            {
                
                Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
                filepath = "http://naccounting.net" + @"/Content/FUELEXCL.xls";
                Microsoft.Office.Interop.Excel.Workbook wbook = null;
                Microsoft.Office.Interop.Excel.Worksheet wsheet = null;
                Range range = null;
                app.Visible = false;
                //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                
                wbook = app.Workbooks.Open(filepath, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value);

                string currentSheet = "FUELEXCL";
                wsheet = (Worksheet)wbook.Worksheets.get_Item(currentSheet);
                range = wsheet.get_Range("A1", "A7");
                System.Array myvalues = (System.Array)range.Cells.Value2;
                return filepath + "---" + myvalues.GetLength(1).ToString();
            }
            catch (Exception ex) { return ex.ToString() + "\n\n\r" + filepath; }
        }
        int pageSize = 20;
        [ValidationFunction(ActionName.ViewListFuelExpenses)]
        public ActionResult Index(int? page, int? driverID)
        {
            var db = DB.Entities;
            var list = db.Fuel___Expenses.Where(m => ((driverID == null ? true : m.Driver == driverID.Value)))
                    .OrderByDescending(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_IndexPartial", list);
            }
            SelectOption(new Fuel___Expenses());
            return View(list);
        }

        void SelectOption(Fuel___Expenses obj)
        {
            //#region SELECT OPTION
            //string dataFuel_Expenses = "<option value=''>--Select Fuel_Expenses--</option>";
            //foreach (var item in NationalIT.DB.Entities.Fuel___Expenses)
            //{
            //    dataFuel_Expenses += string.Format("<option value='{0}'>{0}</option>", item.ID);
            //}
            //ViewBag.dataFuel_Expenses = dataFuel_Expenses;
            //#endregion


            var lst1 = new List<string> { "FUEL", "ADVANCE", "REPAIR" };
            string type = "";
            foreach (var item in lst1)
            {
                string selected = "";
                if (!string.IsNullOrEmpty(obj.Type) && obj.Type.Equals(item, StringComparison.OrdinalIgnoreCase))
                {
                    selected = "selected='selected'";
                }
                type += "<option value='" + item + "' " + selected + ">" + item + "</option>";
            }
            ViewBag.Type = type;

            #region SELECT Driver info
            string dataDriver_Info = "<option value=''>--Select Driver_Info--</option>";
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
            ViewBag.dataDriver = dataDriver_Info;
            #endregion
        }
        [ValidationFunction(ActionName.ViewListFuelExpenses)]
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
        [ValidationFunction(ActionName.NewOrEditItem)]
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
        [ValidationFunction(ActionName.ViewListFuelExpenses)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.Fuel___Expenses.FirstOrDefault(m => m.ID == id);
            if (obj == null)
            {
                obj = new Fuel___Expenses() { Current_Payroll = true, fee_charged = true };
            }
            SelectOption(obj);
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(Fuel___Expenses model, FormCollection frm)
        {
            try
            {
                var db = DB.Entities;
                model.Date = CommonFunction.ChangeFormatDate(frm["Date"]);
                model.Type = frm["cboType"];
                if (model.ID == 0)
                {
                    // Edit                    
                    db.Fuel___Expenses.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("Fuel___Expenses", model);
                    db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                SelectOption(model);
                return View(model);
            }
        }
        [ValidationFunction(ActionName.ViewListFuelExpenses)]
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
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult Edit(Fuel___Expenses model, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
                model.Type = collection["cboType"];
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
