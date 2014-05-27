using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Microsoft.Reporting.WebForms;

namespace Alabama.Reports
{
    public partial class Payroll : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                ReportViewer1.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("/Reports/CurrentPayroll.rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();
                var parameters = new List<ReportParameter>();
                var ds = new CurrentPayroll();
                DataTable trip = ds.Trips;
                for (int i = 0; i < 3; i++)
                {
                    var dr = trip.NewRow();
                    dr["PickupDate"] = "xxx";
                    dr["DeliveryDate"] = "xxx";
                    dr["Charges"] = "xxx";
                    trip.Rows.Add(dr);
                }
                ReportParameter driverName = new ReportParameter("DriverName", "xxx");
                ReportParameter tripfee = new ReportParameter("TripTotalFee", "xxx");
                ReportParameter tripTotal = new ReportParameter("TripTotal", "xxx");
                parameters.Add(driverName);
                parameters.Add(tripfee);
                parameters.Add(tripTotal);
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("Trips", trip));
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("FuelExpenses", (DataTable)ds.FuelExpenses));
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("OperatingExpenses", (DataTable)ds.OperatingExpenses));
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("OwnerExpenses", (DataTable)ds.OwnerExpenses));
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("DriverExpenses", (DataTable)ds.DriverExpenses));

                this.ReportViewer1.LocalReport.SetParameters(parameters);
                ReportViewer1.LocalReport.Refresh();
            }
        }

        private DataTable TripInfoOutstanding()
        {
            var db = DB.Entities;
            DateTime fromdate = DateTime.Parse(Request.QueryString["startdate"]);
            DateTime todate = DateTime.Parse(Request.QueryString["enddate"]);
            List<int?> lstDriver = db.Trip_Info.Where(m => m.Order_date >= fromdate && m.Order_date <= todate).Select(m => m.Driver).Distinct().ToList();
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
    }
}