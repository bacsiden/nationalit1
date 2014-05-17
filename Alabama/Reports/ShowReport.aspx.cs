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
            DataTable dt = new DispatcherTrip().DispatcherTrip1;
            DataRow dr = dt.NewRow();
            dr["DispatcherName"] = "xxxxxx";
            dr["CustomerName"] = "y";
            dr["TripID"] = "zzz";
            dt.Rows.Add(dr);
            return dt;
        }
    }
}