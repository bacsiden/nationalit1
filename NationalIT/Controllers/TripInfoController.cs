using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using System.Data.Objects.SqlClient;

namespace NationalIT.Controllers
{
    [Authorize]
    public class TripInfoController : BaseController
    {
        DB_9B22F2_nationalitEntities db = DB.Entities;
        int pageSize = 20;
        //
        // GET: /Owner/
        [ValidationFunction(ActionName.ViewListTrip)]
        public ActionResult Index(int? page, int? driverID, int? tripID, int? driverPaid)
        {
            var db = DB.Entities;
            var lst = db.Trip_Info.Where(m => (driverID == null ? true : m.Driver == driverID.Value) && (tripID == null ? true : m.Invoice == tripID.Value));
            if (driverPaid.HasValue)
            {
                if (driverPaid.Value != 2)
                {
                    bool driver_paid = false;
                    if (driverPaid.Value == 1)
                    {
                        driver_paid = true;
                    }
                    lst = lst.Where(m => m.Driver_paid == driver_paid);
                }
            }
            else
            {
                lst = lst.Where(m => m.Driver_paid == true);
            }
            var list = lst.OrderByDescending(m => m.Driver_paid).ThenBy(m => m.Trip_ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_IndexPartial", list);
            }
            SelectOption();
            return View(list);
        }
        //
        // GET: /Owner/Edit/5
        void SelectOption()
        {
            #region SELECT OPTION
            var lst = DB.Entities.Trip_Info.Select(m => m.Invoice).ToList();
            string dataTripInfo = "<option value=''>- All Invoice_ID -</option>";
            foreach (var item in lst)
            {
                dataTripInfo += string.Format("<option value='{0}'>{0}</option>", item);
            }
            ViewBag.dataTripInfo = dataTripInfo;

            string dataDriver_Info = "<option value=''>- All Driver_Info -</option>";
            foreach (var item in NationalIT.DB.Entities.Driver_Info)
            {
                dataDriver_Info += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
            }
            ViewBag.dataDriver_Info = dataDriver_Info;
            #endregion
        }
        [ValidationFunction(ActionName.ViewListTrip)]
        public ActionResult NewOrEdit(int? driverID, int? id = 0)
        {
            var obj = DB.Entities.Trip_Info.FirstOrDefault(m => m.Trip_ID == id);
            if (obj == null)
            {
                obj = new Trip_Info() { Driver = driverID, Picked = true, Current_Payroll = true, Customer_Invoiced_date = DateTime.Now.Date, Order_date = DateTime.Now, Pickup_date = DateTime.Now };
            }

            #region SELECT OPTION
            string dataCustomer = "<option value='' >--Select Customer--</option>";
            foreach (var item in NationalIT.DB.Entities.Customer_Info)
            {
                if (obj != null && obj.Customer == item.Customer_ID)
                {
                    dataCustomer += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.Customer_ID, item.Customer_Name);
                }
                else
                {
                    dataCustomer += string.Format("<option value='{0}'>{1}</option>", item.Customer_ID, item.Customer_Name);
                }
            }
            ViewBag.dataCustomer = dataCustomer;
            string dataDispatchers = "<option value=''>--Select Dispatcher--</option>";
            foreach (var item in NationalIT.DB.Entities.Dispatchers)
            {
                if (obj != null && obj.Dispatcher == item.ID)
                {
                    dataDispatchers += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
                else
                {
                    dataDispatchers += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
            }
            ViewBag.dataDispatchers = dataDispatchers;

            string dataDriver = "<option value=''>--Select Driver--</option>";
            foreach (var item in NationalIT.DB.Entities.Driver_Info)
            {
                if (obj != null && obj.Driver == item.ID)
                {
                    dataDriver += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
                else
                {
                    dataDriver += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
            }
            ViewBag.dataDriver = dataDriver;

            string dataEquipment = "<option  value='' >--Select Equiment--</option>";
            foreach (var item in NationalIT.DB.Entities.Equipment)
            {
                if (obj != null && obj.Equipment_ID == item.ID)
                {
                    dataEquipment += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.ID, item.Equipment_number);
                }
                else
                {
                    dataEquipment += string.Format("<option value='{0}'>{1}</option>", item.ID, item.Equipment_number);
                }
            }
            ViewBag.dataEquipment = dataEquipment;

            var lstCompany = db.Company.ToList();
            ViewBag.dataCompany = CommonFunction.BuildDropdown(lstCompany.Select(m => m.ID.ToString()).ToArray(),
                lstCompany.Select(m => m.Name+" - "+m.Address+" , "+m.FaxNumber).ToArray(), obj.Company, "--Select Company--");

            #endregion

            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(Trip_Info model, FormCollection frm)
        {
            try
            {
                if (model.Dispatcher.HasValue)
                {
                    var db = DB.Entities;
                    model.Customer_Invoiced_date = CommonFunction.ChangeFormatDate(frm["Customer_Invoiced_date"]).Value;
                    model.Order_date = CommonFunction.ChangeFormatDate(frm["Order_date"]);
                    model.Delivery_date = CommonFunction.ChangeFormatDate(frm["Delivery_date"]);
                    model.Pickup_date = CommonFunction.ChangeFormatDate(frm["Pickup_date"]);
                    if (model.Trip_ID == 0)
                    {
                        // New
                        db.Trip_Info.AddObject(model);
                        db.SaveChanges();
                        model.Invoice = 9999 + model.Trip_ID;

                        var income = new Income()
                        {
                            IncomeDate = DateTime.Now,
                            AmountInvoiced = model.Total_charges,
                            Comments = model.Comment,
                            InvoiceNumber = model.Invoice,
                            Driver = model.Driver_Info != null ? model.Driver_Info.Last_name + " " + model.Driver_Info.First_name : null,
                            FundedAmount = model.Total_charges,
                        };
                        db.Income.AddObject(income);
                    }
                    else
                    {
                        db.AttachTo("Trip_Info", model);
                        db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Dispatcher", "This field is required");
                }
            }
            catch
            {

            }
            #region SELECT OPTION
            string dataCustomer = "<option value=''>--Select Customer--</option>";
            foreach (var item in NationalIT.DB.Entities.Customer_Info)
            {
                if (model != null && model.Customer == item.Customer_ID)
                {
                    //dataDispatchers += "{ \"id\": " + item.ID + ", \"label\": \"" + item.Last_name + " " + item.First_name + "\" }";
                    dataCustomer += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.Customer_ID, item.Customer_Name);
                }
                else
                {
                    dataCustomer += string.Format("<option value='{0}'>{1}</option>", item.Customer_ID, item.Customer_Name);
                }
            }
            ViewBag.dataCustomer = dataCustomer;
            string dataDispatchers = "<option value=''>--Select Dispatcher--</option>";
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

            string dataDriver = "<option value=''>--Select Driver--</option>";
            foreach (var item in NationalIT.DB.Entities.Driver_Info)
            {
                if (model != null && model.Driver == item.ID)
                {
                    dataDriver += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
                else
                {
                    dataDriver += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
            }
            ViewBag.dataDriver = dataDriver;

            string dataEquipment = "<option value='' >--Select Equiment--</option>";
            foreach (var item in NationalIT.DB.Entities.Equipment)
            {
                if (model != null && model.Equipment_ID == item.ID)
                {
                    dataEquipment += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.ID, item.Equipment_number);
                }
                else
                {
                    dataEquipment += string.Format("<option value='{0}'>{1}</option>", item.ID, item.Equipment_number);
                }
            }
            ViewBag.dataEquipment = dataEquipment;
            var lstCompany = DB.Entities.Company.ToList();
            ViewBag.dataCompany = CommonFunction.BuildDropdown(lstCompany.Select(m => m.ID.ToString()).ToArray(),
                lstCompany.Select(m => m.Name + " - " + m.Address + " , " + m.FaxNumber).ToArray(), model.Company, "--Select Company--");
            #endregion
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
                foreach (var item in lstID)
                {
                    DeleteItem(db.Trip_Info, item, "Trip_ID");
                }
            }
            catch
            {

            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult GetCutomerInfoByKey(int? page, string query = "")
        {
            page = page.HasValue ? page.Value : 1;
            int id = 0;
            int.TryParse(query, out id);
            var db = NationalIT.DB.Entities;
            var list = db.Customer_Info.Where(m =>
                m.Customer_Name.Contains(query) || (id == 0) ? true : m.Customer_ID == id);
            return PartialView(list.OrderByDescending(m => m.Customer_ID).ToPagedList(page.Value, 5));
        }
        [HttpPost]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public string GetAdressCustomerInfo(int id)
        {
            string address = "";
            var obj = DB.Entities.Customer_Info.FirstOrDefault(m => m.Customer_ID == id);
            if (obj != null)
            {
                address = obj.Street + ", " + obj.City + ", " + obj.State + " " + obj.ZIP_Code + ", Phone: " + obj.Phone;
            }
            return address;
        }
    }
}
