using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    public class DriverInfoController : Controller
    {
        int pageSize = 20;

        public ActionResult Index(int? page, int? driverID)
        {
            var db = DB.Entities;
            var list = db.Driver_Info.Where(m => (driverID == null ? true : m.ID == driverID.Value))
                    .OrderByDescending(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize);
            SelectOption();
            if (Request.IsAjaxRequest())
            {
                return PartialView("_IndexPartial", list);
            }
            return View(list);
        }
        void SelectOption()
        {
            #region SELECT OPTION
            string dataDriver_Info = "<option >--Select Driver_Info--</option>";
            foreach (var item in NationalIT.DB.Entities.Driver_Info)
            {
                dataDriver_Info += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
            }
            ViewBag.dataDriver_Info = dataDriver_Info;
            #endregion
        }
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.Driver_Info.FirstOrDefault(m => m.ID == id);
            if (obj == null) obj = new Driver_Info() {Expiration_Date=DateTime.Now};

            #region SELECT OPTION
            string dataOwners = "<option >--Select Owners--</option>";
            foreach (var item in NationalIT.DB.Entities.Owners)
            {
                if (obj != null && obj.Owner_Name == item.OwnerID)
                {
                    //dataDispatchers += "{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                    dataOwners += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.OwnerID, item.Name);
                }
                else
                {
                    dataOwners += string.Format("<option value='{0}'>{1}</option>", item.OwnerID, item.Name);
                }
            }
            ViewBag.dataOwners = dataOwners;

            string dataDispatchers = "<option >--Select Dispatcher--</option>";
            foreach (var item in NationalIT.DB.Entities.Dispatchers)
            {
                if (obj != null && obj.Dispatcher == item.ID)
                {
                    //dataDispatchers += "{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                    dataDispatchers += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
                else
                {
                    dataDispatchers += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
            }
            ViewBag.dataDispatchers = dataDispatchers;

            string dataTruck = "<option >--Select Truck--</option>";
            foreach (var item in NationalIT.DB.Entities.Equipment.Where(m => m.Equipment_Type.Equals("TRUCK")))
            {
                if (obj != null && obj.Truck == item.Equipment_number)
                {
                    dataTruck += string.Format("<option value='{0}' selected='selected'>{0}</option>", item.Equipment_number);
                }
                else
                {
                    dataTruck += string.Format("<option value='{0}'>{0}</option>", item.Equipment_number);
                }
            }
            ViewBag.dataTruck = dataTruck;

            string dataTrailer = "<option >--Select Trailer--</option>";
            foreach (var item in NationalIT.DB.Entities.Equipment.Where(m => m.Equipment_Type.Equals("TRAILER")))
            {
                if (obj != null && obj.Trailer == item.Equipment_number)
                {
                    dataTrailer += string.Format("<option value='{0}' selected='selected'>{0}</option>", item.Equipment_number);
                }
                else
                {
                    dataTrailer += string.Format("<option value='{0}'>{0}</option>", item.Equipment_number);
                }
            }
            ViewBag.dataTrailer = dataTrailer;
            #endregion

            return View(obj);
        }

        [HttpPost]
        public ActionResult NewOrEdit(Driver_Info model,FormCollection frm)
        {
            try
            {
                var db = DB.Entities;
                model.Expiration_Date = CommonFunction.ChangeFormatDate(frm["Expiration_Date"]).Value;
                model.Date_Issued = CommonFunction.ChangeFormatDate(frm["Date_Issued"]);
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
                #region SELECT OPTION
                string dataOwners = "<option >--Select Owners--</option>";
                foreach (var item in NationalIT.DB.Entities.Owners)
                {
                    if (model != null && model.Owner_Name == item.OwnerID)
                    {
                        //dataDispatchers += "{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                        dataOwners += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.OwnerID, item.Name);
                    }
                    else
                    {
                        dataOwners += string.Format("<option value='{0}'>{1}</option>", item.OwnerID, item.Name);
                    }
                }
                ViewBag.dataOwners = dataOwners;

                string dataDispatchers = "<option >--Select Dispatcher--</option>";
                foreach (var item in NationalIT.DB.Entities.Dispatchers)
                {
                    if (model != null && model.Dispatcher == item.ID)
                    {
                        //dataDispatchers += "{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                        dataDispatchers += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                    }
                    else
                    {
                        dataDispatchers += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                    }
                }
                ViewBag.dataDispatchers = dataDispatchers;

                string dataTruck = "<option >--Select Truck--</option>";
                foreach (var item in NationalIT.DB.Entities.Equipment.Where(m => m.Equipment_Type.Equals("TRUCK")))
                {
                    if (model != null && model.Truck == item.Equipment_number)
                    {
                        dataTruck += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.ID, item.Equipment_number);
                    }
                    else
                    {
                        dataTruck += string.Format("<option value='{0}'>{1}</option>", item.ID, item.Equipment_number);
                    }
                }
                ViewBag.dataTruck = dataTruck;

                string dataTrailer = "<option >--Select Trailer--</option>";
                foreach (var item in NationalIT.DB.Entities.Equipment.Where(m => m.Equipment_Type.Equals("TRAILER")))
                {
                    if (model != null && model.Trailer == item.Equipment_number)
                    {
                        dataTrailer += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.ID, item.Equipment_number);
                    }
                    else
                    {
                        dataTrailer += string.Format("<option value='{0}'>{1}</option>", item.ID, item.Equipment_number);
                    }
                }
                ViewBag.dataTrailer = dataTrailer;
                #endregion
                return View(model);
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


        public ActionResult DriverPayroll(int id)
        {
            var obj = DB.Entities.Driver_Info.FirstOrDefault(m => m.ID == id);
            if (obj == null) obj = new Driver_Info();
            return View(obj);
        }
        #region Report ----------------------
        public ActionResult Printreport(string listIDTripInfo = "", string listIDFuelExpenses = "", string listIDOperatingExpenses = "")
        {
            try
            {
                // TODO: Add delete logic here
                string[] listTripID = GetArrayByString(listIDTripInfo);
                string[] listFuelExpensesID = GetArrayByString(listIDFuelExpenses);
                string[] listOperatingExpensesID = GetArrayByString(listIDOperatingExpenses);

            }
            catch
            {

            }
            return RedirectToAction("Index");
        }

        public ActionResult PDFreport(string listIDTripInfo = "", string listIDFuelExpenses = "", string listIDOperatingExpenses = "")
        {
            try
            {
                // TODO: Add delete logic here
                string[] listTripID = GetArrayByString(listIDTripInfo);
                string[] listFuelExpensesID = GetArrayByString(listIDFuelExpenses);
                string[] listOperatingExpensesID = GetArrayByString(listIDOperatingExpenses);

            }
            catch
            {

            }
            return RedirectToAction("Index");
        }

        public string[] GetArrayByString(string arrayID = "")
        {
            return arrayID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion
    }
}
