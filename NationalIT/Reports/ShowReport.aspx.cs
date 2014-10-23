using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Microsoft.Reporting.WebForms;

namespace NationalIT.Reports
{

    public partial class ShowReport : System.Web.UI.Page
    {
        public bool AllDate
        {
            get
            {
                return !string.IsNullOrEmpty(Request.QueryString["allDate"]);
            }
        }
        public DateTime StartDate
        {
            get
            {
                if (Request.QueryString["StartDate"] == null) return DateTime.Now;
                return DateTime.Parse(Request.QueryString["StartDate"]);
            }
        }
        public DateTime EndDate
        {
            get
            {
                if (Request.QueryString["EndDate"] == null) return DateTime.Now;
                return DateTime.Parse(Request.QueryString["EndDate"]);
            }
        }
        public int DispatcherID
        {
            get
            {
                return string.IsNullOrEmpty(Request.QueryString["selectDispatcher"]) ? 0 : int.Parse(Request.QueryString["selectDispatcher"]);
            }
        }
        public int CompanyID
        {
            get
            {
                return string.IsNullOrEmpty(Request.QueryString["selectCompany"]) ? 0 : int.Parse(Request.QueryString["selectCompany"]);
            }
        }
        public int DriverID
        {
            get
            {
                return string.IsNullOrEmpty(Request.QueryString["selectDriver"]) ? 0 : int.Parse(Request.QueryString["selectDriver"]);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {

                string reportName = Request.QueryString["ReportName"];
                ReportViewer1.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("/Reports/" + reportName + ".rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportParameter[] parameters = null;
                DataTable dt = LoadReportData(reportName, out parameters);
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("DataSet1", dt));
                if (parameters != null)
                    this.ReportViewer1.LocalReport.SetParameters(parameters);

                ReportViewer1.LocalReport.Refresh();
            }
        }

        private DataTable LoadReportData(string reportName, out ReportParameter[] rparam)
        {
            rparam = null;
            switch (reportName)
            {
                case "TripInfoOutstanding":
                    return TripInfoOutstanding();
                case "ScheduleOfInvoices":
                    return ScheduleOfInvoices(out rparam);
                case "DispatcherTrip1":
                    return DispatcherTrip1();
                case "DispatcherTrip2":
                    return DispatcherTrip2();
                case "Invoice":
                    return Invoice();
                default:
                    return null;
            }
        }

        private DataTable TripInfoOutstanding()
        {
            var db = DB.Entities;
            var lst = db.Trip_Info.Where(m => (DispatcherID == 0 || m.Dispatcher == DispatcherID) &&
                (DriverID == 0 || m.Driver == DriverID) &&
                (CompanyID == 0 || m.Company == CompanyID) && AllDate ? true :
                (m.Order_date != null && m.Order_date.Value >= StartDate && m.Order_date.Value <= EndDate)).ToList();

            List<int> lstDriver = lst.Where(m => m.Driver != null).Select(m => m.Driver.Value).Distinct().ToList<int>();

            DataTable dt = new TripInfoOutstanding().DataTable1;
            foreach (int driverID in lstDriver)
            {
                int totalCharges = 0;
                List<DataRow> lstDR = new List<DataRow>();
                foreach (var item in lst.Where(m => m.Driver == driverID))
                {
                    DataRow dr = dt.NewRow();
                    dr["DriverID"] = driverID;
                    dr["DriverName"] = item.Driver_Info != null ? item.Driver_Info.First_name + " " + item.Driver_Info.First_name : "";
                    dr["OrderDate"] = String.Format("{0:MM/dd/yyyy}", item.Order_date);
                    dr["PickupDate"] = String.Format("{0:MM/dd/yyyy}", item.Pickup_date);
                    dr["DeliveryDate"] = String.Format("{0:MM/dd/yyyy}", item.Delivery_date);
                    dr["CustomerName"] = item.Customer_Info != null ? item.Customer_Info.Customer_Name : "";
                    dr["DeliveryLocation"] = item.Delivery_location;
                    dr["ComfirmedRate1"] = item.Comfirmed_Rate == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Comfirmed_Rate);
                    dr["LumperExtra1"] = item.Extra_charges == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Extra_charges);
                    dr["DetentionPay1"] = item.Detention_pay == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Detention_pay);
                    dr["ChargesBack1"] = item.Chargebacks == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Chargebacks);
                    dr["TotalCharges1"] = item.Total_charges == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Total_charges);

                    totalCharges += (int)item.Total_charges;
                    lstDR.Add(dr);
                }
                foreach (var item in lstDR)
                {
                    item["TotalCharges"] = string.Format("{0:C}", totalCharges);
                    dt.Rows.Add(item);
                }
            }

            return dt;
        }

        private DataTable ScheduleOfInvoices(out ReportParameter[] rparam)
        {
            var db = DB.Entities;
            var lst = db.Trip_Info.Where(m => (DispatcherID == 0 || m.Dispatcher == DispatcherID) &&
                (DriverID == 0 || m.Driver == DriverID) &&
                (CompanyID == 0 || m.Company == CompanyID) && AllDate ? true :
                (m.Order_date != null && m.Order_date.Value >= StartDate && m.Order_date.Value <= EndDate)).ToList();

            DataTable dt = new ScheduleOfInvoices().DataTable1;
            long totals = 0;
            foreach (var item in lst)
            {
                DataRow dr = dt.NewRow();
                dr["Name"] = item.Customer_Info != null ? item.Customer_Info.Customer_Name : "";
                dr["Date"] = String.Format("{0:MM/dd/yyyy}", item.Order_date);
                //Lấy 2 kí tự đầu của first name, last name dirver info
                string invoice = item.Invoice + "";
                if (item.Driver_Info != null)
                {
                    string char1 = string.IsNullOrEmpty(item.Driver_Info.First_name) ? null : item.Driver_Info.First_name[0] + "";
                    string char2 = string.IsNullOrEmpty(item.Driver_Info.Last_name) ? null : item.Driver_Info.Last_name[0] + "";
                    invoice = char1 + char2 + invoice;
                }
                dr["Invoice"] = invoice;
                dr["PO"] = item.PO_;
                dr["Amount"] = (int)item.Total_charges + ".00";
                int total = 0;
                if (item.Total_charges.HasValue)
                    total = (int)item.Total_charges;
                totals += total;
                //totalCharges += (int)item.Total_charges;
                dt.Rows.Add(dr);
            }
            rparam = new ReportParameter[3];
            rparam[0] = new ReportParameter("DateOfAssignment", string.Format("{0:MM/dd/yyyy}", DateTime.Now));
            rparam[1] = new ReportParameter("Count", lst.Count.ToString());
            rparam[2] = new ReportParameter("Total", totals.ToString());
            return dt;
        }

        private DataTable DispatcherTrip1()
        {
            var db = DB.Entities;

            var lst = db.Trip_Info.Where(m => (DispatcherID == 0 || m.Dispatcher == DispatcherID) &&
                (DriverID == 0 || m.Driver == DriverID) &&
                (CompanyID == 0 || m.Company == CompanyID) && AllDate ? true :
                (m.Order_date != null && m.Order_date.Value >= StartDate && m.Order_date.Value <= EndDate)
                 && m.Deliverd && m.Customer_Invoiced).ToList();

            List<int> lstDis = lst.Where(m => m.Dispatcher != null).Select(m => m.Dispatcher.Value).Distinct().ToList<int>();
            DataTable dt = new DispatcherTrip().DataTable1;
            foreach (int dispatherID in lstDis)
            {
                int sumofCharges = 0;
                List<DataRow> lstDR = new List<DataRow>();
                foreach (var item in lst.Where(m => m.Dispatcher == dispatherID))
                {
                    DataRow dr = dt.NewRow();
                    //dr["DispatherID"] = dispatherID;
                    var objdis = db.Dispatchers.FirstOrDefault(m => m.ID == dispatherID);
                    dr["DispatcherName"] = objdis != null ? objdis.First_name + objdis.Last_name : "1";
                    dr["OrderDate"] = String.Format("{0:MM/dd/yyyy}", item.Order_date);
                    dr["PickupDate"] = String.Format("{0:MM/dd/yyyy}", item.Pickup_date);
                    dr["DeliveryDate"] = String.Format("{0:MM/dd/yyyy}", item.Delivery_date);
                    dr["CustomerName"] = item.Customer_Info != null ? item.Customer_Info.Customer_Name : "";
                    dr["CustomerAddress"] = item.Customer_Info != null ? item.Customer_Info.State + "" + item.Customer_Info.Street : "";
                    dr["DriverName"] = item.Driver_Info != null ? item.Driver_Info.First_name + "" + item.Driver_Info.Last_name : "";
                    //dr["DeliveryLocation"] = item.Delivery_location;
                    //dr["ComfirmedRate"] = item.Comfirmed_Rate == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Comfirmed_Rate);
                    //dr["LumperExtra"] = item.Extra_charges == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Extra_charges);
                    //dr["DetentionPay"] = item.Detention_pay == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Detention_pay);
                    //dr["ChargesBack"] = item.Chargebacks == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Chargebacks);
                    dr["TotalCharges"] = item.Total_charges;// == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Total_charges);
                    dr["PayRate"] = objdis.PayRate;
                    sumofCharges += (int)item.Total_charges;
                    lstDR.Add(dr);
                }
                foreach (var item in lstDR)
                {
                    double payrate = double.Parse(item["PayRate"].ToString());
                    item["SumOfCharges"] = string.Format("{0:C}", sumofCharges * payrate);
                    dt.Rows.Add(item);
                }
            }

            return dt;
        }
        private DataTable DispatcherTrip2()
        {
            var db = DB.Entities;

            var lst = db.Trip_Info.Where(m => (DispatcherID == 0 || m.Dispatcher == DispatcherID) &&
                (DriverID == 0 || m.Driver == DriverID) &&
                (CompanyID == 0 || m.Company == CompanyID) && AllDate ? true :
                (m.Order_date != null && m.Order_date.Value >= StartDate && m.Order_date.Value <= EndDate)
                && m.Deliverd && m.Customer_Invoiced && m.Driver_paid).ToList();

            List<int> lstDis = lst.Where(m => m.Dispatcher != null).Select(m => m.Dispatcher.Value).Distinct().ToList<int>();
            DataTable dt = new DispatcherTrip().DataTable1;
            foreach (int dispatherID in lstDis)
            {
                int sumofCharges = 0;
                List<DataRow> lstDR = new List<DataRow>();
                foreach (var item in lst.Where(m => m.Dispatcher == dispatherID))
                {
                    DataRow dr = dt.NewRow();
                    //dr["DispatherID"] = dispatherID;
                    var objdis = db.Dispatchers.FirstOrDefault(m => m.ID == dispatherID);
                    dr["DispatcherName"] = objdis != null ? objdis.First_name + objdis.Last_name : "1";
                    dr["OrderDate"] = String.Format("{0:MM/dd/yyyy}", item.Order_date);
                    dr["PickupDate"] = String.Format("{0:MM/dd/yyyy}", item.Pickup_date);
                    dr["DeliveryDate"] = String.Format("{0:MM/dd/yyyy}", item.Delivery_date);
                    dr["CustomerName"] = item.Customer_Info != null ? item.Customer_Info.Customer_Name : "";
                    dr["CustomerAddress"] = item.Customer_Info != null ? item.Customer_Info.State + "" + item.Customer_Info.Street : "";
                    dr["DriverName"] = item.Driver_Info != null ? item.Driver_Info.First_name + "" + item.Driver_Info.Last_name : "";
                    //dr["DeliveryLocation"] = item.Delivery_location;
                    //dr["ComfirmedRate"] = item.Comfirmed_Rate == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Comfirmed_Rate);
                    //dr["LumperExtra"] = item.Extra_charges == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Extra_charges);
                    //dr["DetentionPay"] = item.Detention_pay == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Detention_pay);
                    //dr["ChargesBack"] = item.Chargebacks == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Chargebacks);
                    dr["TotalCharges"] = item.Total_charges;// == null ? string.Format("{0:C}", 0) : string.Format("{0:C}", item.Total_charges);
                    dr["PayRate"] = objdis.PayRate;
                    sumofCharges += (int)item.Total_charges;
                    lstDR.Add(dr);
                }
                foreach (var item in lstDR)
                {
                    double payrate = double.Parse(item["PayRate"].ToString());
                    item["SumOfCharges"] = string.Format("{0:C}", sumofCharges * payrate);
                    dt.Rows.Add(item);
                }
            }

            return dt;
        }

        private DataTable Invoice()
        {
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

            return dt;
        }
    }
}