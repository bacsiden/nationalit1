using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Alabama.Reports
{
    public partial class ShowReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
        {
                ReportViewer1.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("/Reports/Report1.rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();
                //ReportViewer1.LocalReport.DataSources.Add(
                    //new Microsoft.Reporting.WebForms.ReportDataSource("DataSet1", dataset.Tables[0]));

                ReportViewer1.LocalReport.Refresh();
        }
        }
    }
}