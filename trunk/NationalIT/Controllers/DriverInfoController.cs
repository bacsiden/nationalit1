using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using System.Data.SqlClient;

namespace NationalIT.Controllers
{
    [Authorize]
    public class DriverInfoController : BaseController
    {
        int pageSize = 20;
        [ValidationFunction(ActionName.ViewListDriver)]
        public ActionResult Index(int? page, int? driverID, int? active)
        {
            // temp 

            //end temp
            var db = DB.Entities;
            var lst = db.Driver_Info.Where(m => (driverID == null ? true : m.ID == driverID.Value));
            if (active.HasValue)
            {
                if (active.Value != 2)
                {
                    bool driver_active = false;
                    if (active.Value == 1)
                    {
                        driver_active = true;
                    }
                    lst = lst.Where(m => m.Active == driver_active);
                }
            }
            else
            {
                lst = lst.Where(m => m.Active == true);
            }
            var list = lst.OrderByDescending(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize);
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
        [ValidationFunction(ActionName.ViewListDriver)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.Driver_Info.FirstOrDefault(m => m.ID == id);
            if (obj == null) obj = new Driver_Info() { Expiration_Date = DateTime.Now };

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
            // List payroll roll back
            var listPayroll = DB.Entities.TempReport.Where(m => m.DriverID == id).ToList();
            ViewBag.ListPayroll = listPayroll;
            #endregion
            return View(obj);
        }

        [HttpPost]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(Driver_Info model, FormCollection frm)
        {
            try
            {
                if (model.Owner || (!model.Owner) && model.Owner_Name.HasValue && model.Owner_Name.Value > 0)
                {
                    if (model.Owner_Pay_Rate > 0)
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
                    else
                    {
                        ModelState.AddModelError("Owner_Pay_Rate", "The field must be greate than 0");
                    }
                }
                else
                {
                    ModelState.AddModelError("Owner_Name", "The field is required");
                    if (!(model.Owner_Pay_Rate > 0))
                    {
                        ModelState.AddModelError("Owner_Pay_Rate", "The field must be greate than 0");
                    }
                }

            }
            catch
            {

            }
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
            // List payroll roll back
            var listPayroll = DB.Entities.TempReport.Where(m => m.DriverID == model.ID).ToList();
            ViewBag.ListPayroll = listPayroll;
            return View(model);
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
        [ValidationFunction(ActionName.ViewListDriver)]
        public ActionResult _Fuel_ExpensesPartial(int id, int? page)
        {
            if (page == null) page = 0;
            var db = DB.Entities;
            return PartialView(db.Fuel___Expenses.Where(m => m.Driver_Info.ID == id).OrderByDescending(m => m.ID).ToPagedList(page.Value, pageSize));
        }
        [ValidationFunction(ActionName.ViewListDriver)]
        public ActionResult _Operating_ExpensesPartial(int id, int? page)
        {
            if (page == null) page = 0;
            var db = DB.Entities;
            return PartialView(db.Operating_Expenses.Where(m => m.Driver_Info.ID == id).OrderByDescending(m => m.ID).ToPagedList(page.Value, pageSize));
        }

        [ValidationFunction(ActionName.ProcessPayroll)]
        public ActionResult DriverPayroll(int id)
        {
            var obj = DB.Entities.Driver_Info.FirstOrDefault(m => m.ID == id);
            if (obj == null) obj = new Driver_Info();
            // Show/hide button driver payroll & expanses
            var listEquipment = DB.Entities.Equipment.Where(m => m.Driver == id).ToList();
            foreach (var equip in listEquipment)
            {
                if ((equip.OOS) || (equip.Inspection_Expiration.HasValue && (DateTime.Now - equip.Inspection_Expiration.Value).Days > 21) || listEquipment.Count > 1)
                {
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
                    // List payroll roll back
                    var listPayroll = DB.Entities.TempReport.Where(m => m.DriverID == id).ToList();
                    ViewBag.ListPayroll = listPayroll;
                    #endregion
                    if (equip.OOS || listEquipment.Count > 1)
                    {
                        ModelState.AddModelError("", "Out of services");
                    }
                    if ((equip.Inspection_Expiration.HasValue && (DateTime.Now - equip.Inspection_Expiration.Value).Days > 21))
                    {
                        ModelState.AddModelError("", "Out of date");
                    }
                    return View("NewOrEdit", obj);
                }
            }
            return View(obj);
        }

        [ValidationFunction(ActionName.ProcessRollBack)]
        public ActionResult PayrollsRollBack(int driverID, int id = 0)
        {
            var obj = DB.Entities.TempReport.FirstOrDefault(m => m.ID == id);
            ViewBag.driverID = driverID;
            #region SELECT OPTION
            string dataDate = "<option >--Select Date--</option>";
            var list = DB.Entities.TempReport.Where(m => m.DriverID == driverID).ToList();
            foreach (var item in list)
            {
                if (obj != null && obj.ID == item.ID)
                {
                    dataDate += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.ID, item.Date);
                }
                else
                {
                    dataDate += string.Format("<option value='{0}'>{1}</option>", item.ID, item.Date);
                }
            }
            ViewBag.dataDate = dataDate;
            #endregion
            if (Request.IsAjaxRequest())
            {
                return PartialView("_IndexUndoPayrollPartial", obj);
            }
            return View(obj);
        }

        [ValidationFunction(ActionName.ProcessRollBack)]
        private void RollBackAll(int tempReportID)
        {
            var db = DB.Entities;
            var tempreport = db.TempReport.FirstOrDefault(m => m.ID == tempReportID);
            #region Trips
            foreach (var item in tempreport.Temp_Trip_Info)
            {
                bool add = false;
                Trip_Info ttrip = db.Trip_Info.FirstOrDefault(m => m.Trip_ID == item.TripID);
                if (ttrip == null)
                {
                    ttrip = new Trip_Info();
                    add = true;
                }
                ttrip.Address = item.Address;
                ttrip.Comfirmed_Rate = item.Comfirmed_Rate;
                ttrip.Comment = item.Comment;
                ttrip.Current_Payroll = item.Current_Payroll;
                ttrip.Customer = item.Customer;
                ttrip.Customer_Invoiced = item.Customer_Invoiced;
                ttrip.Customer_Invoiced_date = item.Customer_Invoiced_date;
                ttrip.Dead_head_miles = item.Dead_head_miles;
                ttrip.Deliverd = item.Deliverd;
                ttrip.Delivery_date = item.Delivery_date;
                ttrip.Delivery_location = item.Delivery_location;
                ttrip.Detention = item.Detention;
                ttrip.Detention_pay = item.Detention_pay;
                ttrip.Dispatcher = item.Dispatcher;
                ttrip.Driver = item.Driver;
                ttrip.Driver_paid = item.Driver_paid;
                ttrip.Equipment_ID = item.Equipment_ID;
                ttrip.Extra_charges = item.Extra_charges;
                ttrip.Extra_stops = item.Extra_stops;
                ttrip.Invoice = item.Invoice;
                ttrip.Loaded_miles = item.Loaded_miles;
                ttrip.Order_date = item.Order_date;
                ttrip.Paid = item.Paid;
                ttrip.Pick_up_location = item.Pick_up_location;
                ttrip.Picked = item.Picked;
                ttrip.Pickup_date = item.Pickup_date;
                ttrip.PO_ = item.PO_;
                ttrip.Total_charges = item.Total_charges;
                if (add)
                {
                    db.Trip_Info.AddObject(ttrip);
                }
                else
                    db.ObjectStateManager.ChangeObjectState(ttrip, System.Data.EntityState.Modified);
            }
            #endregion
            #region Fuel Expenses
            foreach (var item in tempreport.Temp_Fuel_Expenses)
            {
                bool add = false;
                Fuel___Expenses tfuel = db.Fuel___Expenses.FirstOrDefault(m => m.ID == item.FuelExpensesID);
                if (tfuel == null)
                {
                    tfuel = new Fuel___Expenses();
                    add = true;
                }
                tfuel.Amount = item.Amount;
                tfuel.AmountN = item.AmountN;
                tfuel.Current_Payroll = item.Current_Payroll;
                tfuel.Date = item.Date;
                tfuel.Description = item.Description;
                tfuel.Driver = item.Driver;
                tfuel.fee_charged = item.fee_charged;
                tfuel.Fuel_Card = item.Fuel_Card;
                tfuel.Location = item.Location;
                tfuel.Paid_off = item.Paid_off;
                tfuel.Type = item.Type;
                if (add)
                {
                    db.Fuel___Expenses.AddObject(tfuel);
                }
                else
                    db.ObjectStateManager.ChangeObjectState(tfuel, System.Data.EntityState.Modified);
            }
            #endregion
            #region Operating Expenses
            foreach (var item in tempreport.Temp_Operating_Expenses)
            {
                bool add = false;
                Operating_Expenses toperating = db.Operating_Expenses.FirstOrDefault(m => m.ID == item.OperatingExpensesID);
                if (toperating == null)
                {
                    toperating = new Operating_Expenses();
                    add = true;
                }
                toperating.Amount = item.Amount;
                toperating.Current_Payroll = item.Current_Payroll;
                toperating.Date = item.Date;
                toperating.Description = item.Description;
                toperating.Driver = item.Driver;
                toperating.Location = item.Location;
                toperating.Type = item.Type;
                toperating.Paid_off = item.Paid_off;
                if (add)
                {
                    db.Operating_Expenses.AddObject(toperating);
                }
                else
                    db.ObjectStateManager.ChangeObjectState(toperating, System.Data.EntityState.Modified);
            }
            #endregion
            #region Owner driver Expenses
            foreach (var item in tempreport.Temp_Split_Expenses)
            {
                bool add = false;
                split_expenses tsowner = db.split_expenses.FirstOrDefault(m => m.Id == item.SplitExpensesID);
                if (tsowner == null)
                {
                    tsowner = new split_expenses();
                    add = true;
                }
                tsowner.Amount = item.Amount;
                tsowner.Current_Payroll = item.Current_Payroll;
                tsowner.Date = item.Date;
                tsowner.Details = item.Details;
                tsowner.Expenses = item.Expenses;
                tsowner.Fee_Charged = item.Fee_Charged;
                tsowner.Idndex = item.Index;
                tsowner.OwnerDriver = item.OwnerDriver;
                tsowner.Paid_Off = item.Paid_Off;
                if (add)
                {
                    db.split_expenses.AddObject(tsowner);
                }
                else
                    db.ObjectStateManager.ChangeObjectState(tsowner, System.Data.EntityState.Modified);
            }
            #endregion
            db.SaveChanges();
            DeleteItem(db.TempReport, tempreport.ID);
            //db.ObjectStateManager.ChangeObjectState(tempreport, System.Data.EntityState.Deleted);

        }

        [ValidationFunction(ActionName.ProcessRollBack)]
        public ActionResult RollBack(string driverID, int id, string Trips, string Fuel, string operating, string splitdriver, string splitowner, bool isRollBackAll = false)
        {
            var db = DB.Entities;
            if (isRollBackAll)
            {
                RollBackAll(id);
                return RedirectToAction("PayrollsRollBack", new { driverID = driverID });
            }
            #region Trips
            if (!string.IsNullOrEmpty(Trips))
            {
                string[] lstID = Trips.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> lst = new List<int>();
                foreach (var item in lstID)
                    lst.Add(int.Parse(item));
                var lstTrip = db.Temp_Trip_Info.Where(m => lst.Contains(m.TripID));
                foreach (var item in lstTrip)
                {
                    bool add = false;
                    Trip_Info ttrip = db.Trip_Info.FirstOrDefault(m => m.Trip_ID == item.TripID);
                    if (ttrip == null)
                    {
                        ttrip = new Trip_Info();
                        add = true;
                    }
                    ttrip.Address = item.Address;
                    ttrip.Comfirmed_Rate = item.Comfirmed_Rate;
                    ttrip.Comment = item.Comment;
                    ttrip.Current_Payroll = item.Current_Payroll;
                    ttrip.Customer = item.Customer;
                    ttrip.Customer_Invoiced = item.Customer_Invoiced;
                    ttrip.Customer_Invoiced_date = item.Customer_Invoiced_date;
                    ttrip.Dead_head_miles = item.Dead_head_miles;
                    ttrip.Deliverd = item.Deliverd;
                    ttrip.Delivery_date = item.Delivery_date;
                    ttrip.Delivery_location = item.Delivery_location;
                    ttrip.Detention = item.Detention;
                    ttrip.Detention_pay = item.Detention_pay;
                    ttrip.Dispatcher = item.Dispatcher;
                    ttrip.Driver = item.Driver;
                    ttrip.Driver_paid = item.Driver_paid;
                    ttrip.Equipment_ID = item.Equipment_ID;
                    ttrip.Extra_charges = item.Extra_charges;
                    ttrip.Extra_stops = item.Extra_stops;
                    ttrip.Invoice = item.Invoice;
                    ttrip.Loaded_miles = item.Loaded_miles;
                    ttrip.Order_date = item.Order_date;
                    ttrip.Paid = item.Paid;
                    ttrip.Pick_up_location = item.Pick_up_location;
                    ttrip.Picked = item.Picked;
                    ttrip.Pickup_date = item.Pickup_date;
                    ttrip.PO_ = item.PO_;
                    ttrip.Total_charges = item.Total_charges;
                    if (add)
                    {
                        db.Trip_Info.AddObject(ttrip);
                    }
                    else
                        db.ObjectStateManager.ChangeObjectState(ttrip, System.Data.EntityState.Modified);
                }
                foreach (var item in lst)
                {
                    var xxx = db.Temp_Trip_Info.FirstOrDefault(m => m.ID == item);
                    if (xxx != null)
                        db.DeleteObject(xxx);
                }
            }
            #endregion
            #region Fuel Expenses
            if (!string.IsNullOrEmpty(Fuel))
            {
                string[] lstID = Fuel.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> lst = new List<int>();
                foreach (var item in lstID)
                    lst.Add(int.Parse(item));
                var lstTrip = db.Temp_Fuel_Expenses.Where(m => lst.Contains(m.ID));
                foreach (var item in lstTrip)
                {
                    bool add = false;
                    Fuel___Expenses tfuel = db.Fuel___Expenses.FirstOrDefault(m => m.ID == item.FuelExpensesID);
                    if (tfuel == null)
                    {
                        tfuel = new Fuel___Expenses();
                        add = true;
                    }
                    tfuel.Amount = item.Amount;
                    tfuel.AmountN = item.AmountN;
                    tfuel.Current_Payroll = item.Current_Payroll;
                    tfuel.Date = item.Date;
                    tfuel.Description = item.Description;
                    tfuel.Driver = item.Driver;
                    tfuel.fee_charged = item.fee_charged;
                    tfuel.Fuel_Card = item.Fuel_Card;
                    tfuel.Location = item.Location;
                    tfuel.Paid_off = item.Paid_off;
                    tfuel.Type = item.Type;
                    if (add)
                    {
                        db.Fuel___Expenses.AddObject(tfuel);
                    }
                    else
                        db.ObjectStateManager.ChangeObjectState(tfuel, System.Data.EntityState.Modified);
                }
                foreach (var item in lst)
                {
                    var xxx = db.Temp_Fuel_Expenses.FirstOrDefault(m => m.ID == item);
                    if (xxx != null)
                        db.DeleteObject(xxx);
                }
            }

            #endregion
            #region Operating Expenses
            if (!string.IsNullOrEmpty(operating))
            {
                string[] lstID = operating.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> lst = new List<int>();
                foreach (var item in lstID)
                    lst.Add(int.Parse(item));
                var lstTrip = db.Temp_Operating_Expenses.Where(m => lst.Contains(m.ID));
                foreach (var item in lstTrip)
                {
                    bool add = false;
                    Operating_Expenses toperating = db.Operating_Expenses.FirstOrDefault(m => m.ID == item.OperatingExpensesID);
                    if (toperating == null)
                    {
                        toperating = new Operating_Expenses();
                        add = true;
                    }
                    toperating.Amount = item.Amount;
                    toperating.Current_Payroll = item.Current_Payroll;
                    toperating.Date = item.Date;
                    toperating.Description = item.Description;
                    toperating.Driver = item.Driver;
                    toperating.Location = item.Location;
                    toperating.Type = item.Type;
                    if (add)
                    {
                        db.Operating_Expenses.AddObject(toperating);
                    }
                    else
                        db.ObjectStateManager.ChangeObjectState(toperating, System.Data.EntityState.Modified);
                }
                foreach (var item in lst)
                {
                    var xxx = db.Temp_Operating_Expenses.FirstOrDefault(m => m.ID == item);
                    if (xxx != null)
                        db.DeleteObject(xxx);
                }
            }
            #endregion
            #region Owner Expenses
            if (!string.IsNullOrEmpty(splitowner))
            {
                string[] lstID = splitowner.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> lst = new List<int>();
                foreach (var item in lstID)
                    lst.Add(int.Parse(item));
                var lstTrip = db.Temp_Split_Expenses.Where(m => lst.Contains(m.ID));
                foreach (var item in lstTrip)
                {
                    bool add = false;
                    split_expenses tsowner = db.split_expenses.FirstOrDefault(m => m.Id == item.SplitExpensesID);
                    if (tsowner == null)
                    {
                        tsowner = new split_expenses();
                        add = true;
                    }
                    tsowner.Amount = item.Amount;
                    tsowner.Current_Payroll = item.Current_Payroll;
                    tsowner.Date = item.Date;
                    tsowner.Details = item.Details;
                    tsowner.Expenses = item.Expenses;
                    tsowner.Fee_Charged = item.Fee_Charged;
                    tsowner.Idndex = item.Index;
                    tsowner.OwnerDriver = item.OwnerDriver;
                    tsowner.Paid_Off = item.Paid_Off;
                    if (add)
                    {
                        db.split_expenses.AddObject(tsowner);
                    }
                    else
                        db.ObjectStateManager.ChangeObjectState(tsowner, System.Data.EntityState.Modified);
                }
                foreach (var item in lst)
                {
                    var xxx = db.Temp_Split_Expenses.FirstOrDefault(m => m.ID == item);
                    if (xxx != null)
                        db.DeleteObject(xxx);
                }
            }
            #endregion
            #region Driver Expenses
            if (!string.IsNullOrEmpty(splitdriver))
            {
                string[] lstID = splitdriver.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> lst = new List<int>();
                foreach (var item in lstID)
                    lst.Add(int.Parse(item));
                var lstTrip = db.Temp_Split_Expenses.Where(m => lst.Contains(m.ID));
                foreach (var item in lstTrip)
                {
                    bool add = false;
                    split_expenses tsowner = db.split_expenses.FirstOrDefault(m => m.Id == item.SplitExpensesID);
                    if (tsowner == null)
                    {
                        tsowner = new split_expenses();
                        add = true;
                    }
                    tsowner.Amount = item.Amount;
                    tsowner.Current_Payroll = item.Current_Payroll;
                    tsowner.Date = item.Date;
                    tsowner.Details = item.Details;
                    tsowner.Expenses = item.Expenses;
                    tsowner.Fee_Charged = item.Fee_Charged;
                    tsowner.Idndex = item.Index;
                    tsowner.OwnerDriver = item.OwnerDriver;
                    tsowner.Paid_Off = item.Paid_Off;
                    if (add)
                    {
                        db.split_expenses.AddObject(tsowner);
                    }
                    else
                        db.ObjectStateManager.ChangeObjectState(tsowner, System.Data.EntityState.Modified);
                }
                foreach (var item in lst)
                {
                    var xxx = db.Temp_Split_Expenses.FirstOrDefault(m => m.ID == item);
                    if (xxx != null)
                        db.DeleteObject(xxx);
                }
            }
            #endregion
            db.SaveChanges();

            return RedirectToAction("PayrollsRollBack", new { driverID = driverID });
        }

        [ValidationFunction(ActionName.ProcessPayroll)]
        public ActionResult BeforeDriverPayroll(string url)
        {
            Session["Payroll"] = true;
            return Redirect(url);
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