using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using System.Data.Objects.SqlClient;
using Microsoft.Reporting.WebForms;
using NationalIT.Reports;
using System.Data;

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
                lst = lst.Where(m => m.Driver_paid == false);
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
                lstCompany.Select(m => m.Name + " - " + m.Address + " , " + m.FaxNumber).ToArray(), obj.Company, "--Select Company--");

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
                    if (frm["Customer_Invoiced_date"]==null)
                    {
                        model.Customer_Invoiced_date = DateTime.Now;
                    }
                    else
                    {
                        model.Customer_Invoiced_date = CommonFunction.ChangeFormatDate(frm["Customer_Invoiced_date"]).Value;
                    }
                    
                    model.Order_date = CommonFunction.ChangeFormatDate(frm["Order_date"]);
                    model.Delivery_date = CommonFunction.ChangeFormatDate(frm["Delivery_date"]);
                    model.Pickup_date = CommonFunction.ChangeFormatDate(frm["Pickup_date"]);
                    if (model.Trip_ID == 0 || model.Invoice == 0)
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
                            Driver = model.Driver_Info != null ? model.Driver_Info.Last_name + " " + model.Driver_Info.First_name : "",
                            FundedAmount = model.Total_charges,
                        };
                        db.Income.AddObject(income);
                    }
                    else
                    {
                        var income = db.Income.FirstOrDefault(m => m.InvoiceNumber == model.Invoice);
                        if (income != null)
                        {
                            income.IncomeDate = DateTime.Now;
                            income.AmountInvoiced = model.Total_charges;
                            income.Comments = model.Comment;
                            income.InvoiceNumber = model.Invoice;
                            income.Driver = model.Driver_Info != null ? model.Driver_Info.Last_name + " " + model.Driver_Info.First_name : "";
                            income.FundedAmount = model.Total_charges;
                        }
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

        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult InvoicedReport(FormCollection frm)
        {
            try
            {
                //Xuất dự liệu ra pdf
                Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extension = string.Empty;
                string reportPath = "Reports/Invoice.rdlc";

                // Setup the report viewer object and get the array of bytes
                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = reportPath;

                DataTable dt = new Invoice().DataTable1;
                var db = DB.Entities;
                int id = int.Parse(Request.QueryString["tripID"]);
                var obj = db.Trip_Info.FirstOrDefault(m => m.Trip_ID == id);
                string s = Request.QueryString["companyID"];
                int coID = int.Parse(s);
                var co = db.Company.First(m => m.ID == coID);

                obj.Company = coID;
                db.SaveChanges();
                DataRow dr = dt.NewRow();
                dr["Date"] = String.Format("{0:MM/dd/yyyy}", obj.Order_date);
                dr["CustomerName"] = obj.Customer_Info != null ? obj.Customer_Info.Customer_Name : "";
                dr["CustomerAddress"] = obj.Customer_Info != null ? obj.Customer_Info.City + ", " +
                    obj.Customer_Info.State + " " + obj.Customer_Info.ZIP_Code : "";
                dr["Street"] = obj.Customer_Info != null ? obj.Customer_Info.Street : "";
                //Lấy 2 kí tự đầu của first name, last name dirver info
                string invoice = obj.Invoice + "";
                if (obj.Driver_Info != null)
                {
                    string char1 = string.IsNullOrEmpty(obj.Driver_Info.First_name) ? null : obj.Driver_Info.First_name[0] + "";
                    string char2 = string.IsNullOrEmpty(obj.Driver_Info.Last_name) ? null : obj.Driver_Info.Last_name[0] + "";
                    invoice = char1 + char2 + invoice;
                }
                dr["Invoice"] = invoice;
                dr["Load_"] = obj.Loaded_miles;
                dr["PO_"] = obj.PO_;
                dr["PickupLocation"] = obj.Pick_up_location;
                dr["DeliveryLocation"] = obj.Delivery_location;
                dr["ComfirmedRate"] = obj.Comfirmed_Rate == null ? ".00" : (int)obj.Comfirmed_Rate + ".00";
                dr["Lumber_ExtraCharges"] = obj.Extra_charges == null ? ".00" : (int)obj.Extra_charges + ".00";
                dr["DetentionPay"] = obj.Detention_pay == null ? ".00" : (int)obj.Detention_pay + ".00";
                dr["TotalCharges"] = obj.Total_charges == null ? ".00" : (int)obj.Total_charges + ".00";
                dr["ExtraStops"] = obj.Extra_stops;
                dr["DispatcherName"] = obj.Dispatchers != null ? obj.Dispatchers.Last_name + " " + obj.Dispatchers.Last_name : "";

                dr["CompanyName"] = co.Name;
                dr["CompanyAddress"] = co.Address;
                dr["CompanyPhone"] = co.PhoneNumber;
                dr["CompanyFax"] = co.FaxNumber;
                dt.Rows.Add(dr);

                ReportDataSource newDS = new ReportDataSource(dt.TableName, dt);
                viewer.LocalReport.DataSources.Clear();
                viewer.LocalReport.DataSources.Add(newDS);
                viewer.LocalReport.Refresh();


                byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                string fileName = "Invoice.pdf";
                return File(bytes, mimeType, fileName);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
