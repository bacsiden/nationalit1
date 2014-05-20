using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace Alabama.Reports
{
    public partial class ShowReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string reportName = Request.QueryString["ReportName"];
                ReportViewer1.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("/Reports/" + reportName + ".rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();
                DataTable dt = LoadReportData(reportName);
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("DataSet1", dt));

                ReportViewer1.LocalReport.Refresh();
            }
        }

        private DataTable LoadReportData(string reportName)
        {
            switch (reportName)
            {
                case "TripInfoOutstanding":
                    return TripInfoOutstanding();
                case "ScheduleOfInvoices":
                    return ScheduleOfInvoices();
                case "DispatcherTrip":
                    return DispatcherTrip();
                default:
                    return null;
            }
        }

        private DataTable TripInfoOutstanding()
        {
            var db = DB.Entities;
            List<int?> lstDriver = db.Trip_Info.Take(100).Select(m => m.Driver).Distinct().ToList();
            var lst = db.Trip_Info.Take(100).ToList();
            DataTable dt = new TripInfoOutstanding().DataTable1;
            foreach (int? driverID in lstDriver)
            {
                if (driverID != null)
                {
                    int totalCharges = 0;
                    List<DataRow> lstDR = new List<DataRow>();
                    foreach (var item in lst.Where(m => m.Driver == driverID))
                    {
                        DataRow dr = dt.NewRow();
                        dr["DriverID"] = driverID;
                        dr["DriverName"] = item.Driver_Info != null ? item.Driver_Info.First_name + item.Driver_Info.First_name : "";
                        dr["OrderDate"] = String.Format("{0:MM/dd/yyyy}", item.Order_date);
                        dr["PickupDate"] = String.Format("{0:MM/dd/yyyy}", item.Pickup_date);
                        dr["DeliveryDate"] = String.Format("{0:MM/dd/yyyy}", item.Delivery_date);
                        dr["CustomerName"] = item.Customer_Info != null ? item.Customer_Info.Customer_Name : "";
                        dr["DeliveryLocation"] = item.Delivery_location;
                        dr["ComfirmedRate1"] = item.Comfirmed_Rate;
                        dr["LumperExtra1"] = "LumperExtra";
                        dr["DetentionPay1"] = item.Detention_pay;
                        dr["ChargesBack1"] = "ChargesBack1";
                        dr["TotalCharges1"] = item.Total_charges;

                        totalCharges += (int)item.Total_charges;
                        lstDR.Add(dr);
                    }
                    foreach (var item in lstDR)
                    {
                        item["TotalCharges"] = totalCharges;
                        dt.Rows.Add(item);
                    }
                }
            }


            return dt;
        }

        private DataTable ScheduleOfInvoices()
        {
            var db = DB.Entities;
            //List<int?> lstDriver = db.Trip_Info.Take(100).Select(m => m.Driver).Distinct().ToList();
            var lst = db.Trip_Info.Take(100).ToList();
            DataTable dt = new TripInfoOutstanding().DataTable1;
            //foreach (int? driverID in lstDriver)
            //{
                //if (driverID != null)
                //{
                    //int totalCharges = 0;
                    //List<DataRow> lstDR = new List<DataRow>();
                    foreach (var item in lst)
                    {
                        DataRow dr = dt.NewRow();
                        dr["Name"] = item.Driver_Info != null ? item.Driver_Info.First_name + item.Driver_Info.First_name : "";
                        dr["Date"] = String.Format("{0:MM/dd/yyyy}", item.Order_date);
                        dr["Invoice"] = item.Invoice;
                        dr["PO"] = item.PO_;
                        dr["Amount"] = 100;

                        //totalCharges += (int)item.Total_charges;
                        dt.Rows.Add(dr);
                    }
                    //foreach (var item in lstDR)
                    //{
                    //    item["TotalCharges"] = totalCharges;
                    //    dt.Rows.Add(item);
                    //}
                //}
            //}


            return dt;
        }

        private DataTable DispatcherTrip()
        {
            var db = DB.Entities;
            List<int?> lstDriver = db.Trip_Info.Take(100).Select(m => m.Driver).Distinct().ToList();
            var lst = db.Trip_Info.Take(100).ToList();
            DataTable dt = new DispatcherTrip().DataTable1;
            foreach (int? driverID in lstDriver)
            {
                if (driverID != null)
                {
                    int totalCharges = 0;
                    List<DataRow> lstDR = new List<DataRow>();
                    foreach (var item in lst.Where(m => m.Driver == driverID))
                    {
                        DataRow dr = dt.NewRow();
                        dr["DriverID"] = driverID;
                        dr["DriverName"] = item.Driver_Info != null ? item.Driver_Info.First_name + item.Driver_Info.First_name : "";
                        dr["OrderDate"] = String.Format("{0:MM/dd/yyyy}", item.Order_date);
                        dr["PickupDate"] = String.Format("{0:MM/dd/yyyy}", item.Pickup_date);
                        dr["DeliveryDate"] = String.Format("{0:MM/dd/yyyy}", item.Delivery_date);
                        dr["CustomerName"] = item.Customer_Info != null ? item.Customer_Info.Customer_Name : "";
                        dr["DeliveryLocation"] = item.Delivery_location;
                        dr["ComfirmedRate1"] = item.Comfirmed_Rate;
                        dr["LumperExtra1"] = "LumperExtra";
                        dr["DetentionPay1"] = item.Detention_pay;
                        dr["ChargesBack1"] = "ChargesBack1";
                        dr["TotalCharges1"] = item.Total_charges;

                        totalCharges += (int)item.Total_charges;
                        lstDR.Add(dr);
                    }
                    foreach (var item in lstDR)
                    {
                        item["TotalCharges"] = totalCharges;
                        dt.Rows.Add(item);
                    }
                }
            }


            return dt;
        }
    }
}