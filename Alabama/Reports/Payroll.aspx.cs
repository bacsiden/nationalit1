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
            //Trips=1-2-3&Fuel=3-4-6&operating=3-4-5&splitdriver=2-3$splitowner=3-4
            if (!Page.IsPostBack)
            {
                ReportViewer1.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("/Reports/CurrentPayroll.rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();
                var parameters = new List<ReportParameter>();
                var ds = new CurrentPayroll();
                var db = DB.Entities;
                int driverid = int.Parse(Request.QueryString["driverid"]);
                var driverinfo = db.Driver_Info.FirstOrDefault(m => m.ID == driverid);
                double total1 = 0;
                double fee1 = 0;
                #region Trips
                DataTable trip = ds.Trips;
                string para = Request.QueryString["trips"];
                if (!string.IsNullOrEmpty(para))
                {
                    string[] lstID = para.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> lst = new List<int>();
                    foreach (var item in lstID)
                        lst.Add(int.Parse(item));
                    var lstTrip = db.Trip_Info.Where(m => lst.Contains(m.Trip_ID));
                    foreach (var item in lstTrip)
                    {
                        var dr = trip.NewRow();
                        dr["PickupDate"] = string.Format("{0:MM/dd/yyyy}", item.Order_date);
                        dr["DeliveryDate"] = string.Format("{0:MM/dd/yyyy}", item.Delivery_date);
                        if (item.Total_charges != null)
                            dr["Charges"] = ((double)item.Total_charges).ToString("N2");
                        if (item.Total_charges != null)
                            total1 += (double)item.Total_charges;
                        trip.Rows.Add(dr);
                    }
                }
                if (driverinfo.Pay_rate != null)
                {
                    fee1 = total1 * driverinfo.Pay_rate.Value;
                    total1 += fee1;
                }
                ReportParameter driverName = new ReportParameter("DriverName", driverinfo.First_name + " " + driverinfo.Last_name);
                ReportParameter tripfee = new ReportParameter("TripTotalFee", fee1.ToString("N2"));
                ReportParameter tripTotal = new ReportParameter("TripTotal", total1.ToString("N2"));
                parameters.Add(driverName);
                parameters.Add(tripfee);
                parameters.Add(tripTotal);
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("Trips", trip));
                #endregion
                double total2 = 0;
                double fee2 = 0;
                #region Fuel Expenses
                DataTable fuel = ds.FuelExpenses;
                para = Request.QueryString["Fuel"];
                if (!string.IsNullOrEmpty(para))
                {
                    string[] lstID = para.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> lst = new List<int>();
                    foreach (var item in lstID)
                        lst.Add(int.Parse(item));
                    var lstTrip = db.Fuel___Expenses.Where(m => lst.Contains(m.ID));
                    foreach (var item in lstTrip)
                    {
                        var dr1 = fuel.NewRow();
                        dr1["Date"] = String.Format("{0:MM/dd/yyyy}", item.Date);
                        dr1["Type"] = item.Type;
                        if (item.Amount != null)
                            dr1["Amount"] = ((double)item.Amount).ToString("N2");
                        dr1["Gallons"] = "0";
                        if (item.Amount != null)
                            total2 += (double)item.Amount;
                        fuel.Rows.Add(dr1);
                    }
                }
                if (driverinfo.fuel___advance_Fee_rate != null)
                {
                    fee2 = total2 * driverinfo.fuel___advance_Fee_rate.Value;
                    total2 += fee2;
                }
                ReportParameter fuelFee = new ReportParameter("FuelFee", total2.ToString("N2"));
                ReportParameter fuelAmount = new ReportParameter("FuelAmount", fee2.ToString("N2"));
                parameters.Add(fuelFee);
                parameters.Add(fuelAmount);
                ReportViewer1.LocalReport.DataSources.Add(
                    new Microsoft.Reporting.WebForms.ReportDataSource("FuelExpenses", fuel));
                #endregion
                double total3 = 0;
                #region Operating Expenses
                DataTable opera = ds.OperatingExpenses;
                para = Request.QueryString["operating"];
                if (!string.IsNullOrEmpty(para))
                {
                    string[] lstID = para.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> lst = new List<int>();
                    foreach (var item in lstID)
                        lst.Add(int.Parse(item));
                    var lstTrip = db.Operating_Expenses.Where(m => lst.Contains(m.ID));
                    foreach (var item in lstTrip)
                    {
                        var dr2 = opera.NewRow();
                        dr2["Date"] = string.Format("{0:MM/dd/yyyy}", item.Date);
                        dr2["Type"] = item.Type;
                        if (item.Amount != null)
                            dr2["Amount"] = ((double)item.Amount).ToString("N2");
                        if (item.Amount != null)
                            total3 += (double)item.Amount;
                        opera.Rows.Add(dr2);
                    }
                }
                ReportParameter oTotalAmount = new ReportParameter("OTotalAmount", total3.ToString("N2"));
                parameters.Add(oTotalAmount);
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("OperatingExpenses", opera));
                #endregion
                double total4 = 0;
                double fee4 = 0;
                #region Owner Expenses
                DataTable osplit = ds.OwnerExpenses;
                para = Request.QueryString["splitowner"];
                if (!string.IsNullOrEmpty(para))
                {
                    string[] lstID = para.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> lst = new List<int>();
                    foreach (var item in lstID)
                        lst.Add(int.Parse(item));
                    var lstTrip = db.split_expenses.Where(m => lst.Contains(m.Id));
                    foreach (var item in lstTrip)
                    {
                        var dr3 = osplit.NewRow();
                        dr3["Date"] = string.Format("{0:MM/dd/yyyy}", item.Date);
                        dr3["Expenses"] = item.Expenses;
                        if (item.Amount != null)
                            dr3["Amount"] = ((double)item.Amount).ToString("N2");
                        if (item.Amount != null)
                            total4 += (double)item.Amount;
                        osplit.Rows.Add(dr3);
                    }
                }
                if (driverinfo.fuel___advance_Fee_rate != null)
                {
                    fee4 = total4 * driverinfo.fuel___advance_Fee_rate.Value;
                    total4 += fee4;
                }
                ReportParameter oExFee = new ReportParameter("OExFee", total4.ToString("N2"));
                ReportParameter oExTotal = new ReportParameter("OExTotal", fee4.ToString("N2"));
                parameters.Add(oExFee);
                parameters.Add(oExTotal);
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("OwnerExpenses", osplit));
                #endregion
                double total5 = 0;
                double fee5 = 0;
                #region Driver Expenses
                DataTable dsplit = ds.DriverExpenses;
                para = Request.QueryString["splitdriver"];
                if (!string.IsNullOrEmpty(para))
                {
                    string[] lstID = para.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> lst = new List<int>();
                    foreach (var item in lstID)
                        lst.Add(int.Parse(item));
                    var lstTrip = db.split_expenses.Where(m => lst.Contains(m.Id));
                    foreach (var item in lstTrip)
                    {
                        var dr4 = dsplit.NewRow();
                        dr4["Date"] = string.Format("{0:MM/dd/yyyy}", item.Date);
                        dr4["Expenses"] = item.Expenses;
                        if (item.Amount != null)
                            dr4["Amount"] = ((double)item.Amount).ToString("N2");
                        if (item.Amount != null)
                            total5 += (double)item.Amount;
                        dsplit.Rows.Add(dr4);
                    }
                }
                if (driverinfo.fuel___advance_Fee_rate != null)
                {
                    fee5 = total5 * driverinfo.fuel___advance_Fee_rate.Value;
                    total5 += fee5;
                }
                ReportParameter dExFee = new ReportParameter("DExFee", total5.ToString("N2"));
                ReportParameter dExTotal = new ReportParameter("DExTotal", fee5.ToString("N2"));
                parameters.Add(dExFee);
                parameters.Add(dExTotal);
                ReportViewer1.LocalReport.DataSources.Add(
                new Microsoft.Reporting.WebForms.ReportDataSource("DriverExpenses", dsplit));
                #endregion

                double sum = total2 + total3 + total4 + total5;
                double payrollAmount = total1 - sum;
                double ownerPayment = 0;
                if (driverinfo.Owner_Pay_Rate != null)
                    ownerPayment = (double)driverinfo.Owner_Pay_Rate * payrollAmount;
                double driverPayment = payrollAmount - ownerPayment;

                ReportParameter PayrollAmount = new ReportParameter("PayrollAmount", payrollAmount.ToString("N2"));
                ReportParameter OwnerPayment = new ReportParameter("OwnerPayment", ownerPayment.ToString("N2"));
                ReportParameter DriverPayment = new ReportParameter("DriverPayment", driverPayment.ToString("N2"));
                parameters.Add(PayrollAmount);
                parameters.Add(OwnerPayment);
                parameters.Add(DriverPayment);

                this.ReportViewer1.LocalReport.SetParameters(parameters);
                ReportViewer1.LocalReport.Refresh();
            }
        }
    }
}