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
                case "DispatcherTrip":
                    return LoadDispatcherTrip();
                case "TripInfoOutstanding":
                    return TripInfoOutstanding();
                default:
                    return null;
            }
        }

        private DataTable LoadDispatcherTrip()
        {
            DataTable dt = new DispatcherTrip().DispatcherTrip1;
            DataRow dr = dt.NewRow();
            dr["DispatcherName"] = "xxxxxx";
            dr["CustomerName"] = "y";
            dr["TripID"] = "zzz";
            dt.Rows.Add(dr);
            return dt;
        }

        private DataTable TripInfoOutstanding()
        {
            DataTable dt = new TripInfoOutstanding().DataTable1;
            foreach (var item in DB.Entities.Trip_Info.Take(100))
            {
                DataRow dr = dt.NewRow();
                dr["DriverName"] = item.Driver_Info != null ? item.Driver_Info.First_name + item.Driver_Info.First_name : "";
                dr["CustomerName"] = item.Customer_Info != null ? item.Customer_Info.Customer_Name : "";
                dr["TotalCharges"] = item.Total_charges;
                dt.Rows.Add(dr);
            }

            return dt;
        }
    }
}