using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Microsoft.Reporting.WebForms;
using System.Security;

namespace NationalIT.Reports
{
    public partial class Payroll : System.Web.UI.Page
    {
        [System.Web.Mvc.Authorize]
        protected void Page_Load(object sender, EventArgs e)
        {

            //Trips=1-2-3&Fuel=3-4-6&operating=3-4-5&splitdriver=2-3$splitowner=3-4&escrowLoan=-1-2
            if (!Page.IsPostBack)
            {
                #region Reprint
                string tempreport = Request.QueryString["tempreport"];
                if (!string.IsNullOrEmpty(tempreport))
                {
                    int trID = int.Parse(tempreport);
                    var db = DB.Entities;
                    var objtr = db.TempReport.FirstOrDefault(m => m.ID == trID);
                    #region common prepare
                    ReportViewer1.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                    if (objtr.Temp_EscrowLoan != null && objtr.Temp_EscrowLoan.Count > 0)
                    {
                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("/Reports/CurrentPayroll.rdlc");
                    }
                    else
                    {
                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("/Reports/CurrentPayrollNoEscrowLoan.rdlc");
                    }

                    ReportViewer1.LocalReport.DataSources.Clear();
                    var parameters = new List<ReportParameter>();
                    var ds = new CurrentPayroll();
                    var driverinfo = db.Driver_Info.FirstOrDefault(m => m.ID == objtr.DriverID);
                    #endregion
                    #region Trips
                    double total1 = 0;
                    double fee1 = 0;
                    DataTable trip = ds.Trips;
                    foreach (var item in objtr.Temp_Trip_Info)
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
                    //Calculate total fee
                    fee1 = total1 * objtr.Pay_rate;
                    ReportParameter driverName = new ReportParameter("DriverName", driverinfo.First_name + " " + driverinfo.Last_name);
                    ReportParameter tripfee = new ReportParameter("TripTotalFee", fee1.ToString("N2"));
                    ReportParameter tripTotal = new ReportParameter("TripTotal", total1.ToString("N2"));
                    parameters.Add(driverName);
                    parameters.Add(tripfee);
                    parameters.Add(tripTotal);
                    ReportViewer1.LocalReport.DataSources.Add(
                    new Microsoft.Reporting.WebForms.ReportDataSource("Trips", trip));
                    #endregion
                    #region Fuel Expenses
                    double total2 = 0;
                    double fee2 = 0;
                    DataTable fuel = ds.FuelExpenses;
                    foreach (var item in objtr.Temp_Fuel_Expenses)
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

                    fee2 = total2 * objtr.fuel___advance_Fee_rate;
                    ReportParameter fuelFee = new ReportParameter("FuelFee", fee2.ToString("N2"));
                    ReportParameter fuelAmount = new ReportParameter("FuelAmount", total2.ToString("N2"));
                    parameters.Add(fuelFee);
                    parameters.Add(fuelAmount);
                    ReportViewer1.LocalReport.DataSources.Add(
                        new Microsoft.Reporting.WebForms.ReportDataSource("FuelExpenses", fuel));
                    #endregion
                    #region Operating Expenses
                    double total3 = 0;
                    DataTable opera = ds.OperatingExpenses;
                    foreach (var item in objtr.Temp_Operating_Expenses)
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
                    ReportParameter oTotalAmount = new ReportParameter("OTotalAmount", total3.ToString("N2"));
                    parameters.Add(oTotalAmount);
                    ReportViewer1.LocalReport.DataSources.Add(
                    new Microsoft.Reporting.WebForms.ReportDataSource("OperatingExpenses", opera));
                    #endregion
                    #region Owner Expenses
                    double total4 = 0;
                    double fee4 = 0;
                    double ownerFeeRate = 0;
                    if (driverinfo.Owner_Name != null)
                    {
                        Owners objOwner = db.Owners.FirstOrDefault(m => m.OwnerID == driverinfo.Owner_Name.Value);
                        if (objOwner != null)
                            if (objOwner.fee_rate != null)
                                ownerFeeRate = (double)objOwner.fee_rate;
                    }

                    DataTable osplit = ds.OwnerExpenses;
                    foreach (var item in objtr.Temp_Split_Expenses.Where(m => m.Index == 2))
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
                    fee4 = total4 * ownerFeeRate;

                    ReportParameter oExFee = new ReportParameter("OExFee", fee4.ToString("N2"));
                    ReportParameter oExTotal = new ReportParameter("OExTotal", total4.ToString("N2"));
                    parameters.Add(oExFee);
                    parameters.Add(oExTotal);
                    ReportViewer1.LocalReport.DataSources.Add(
                    new Microsoft.Reporting.WebForms.ReportDataSource("OwnerExpenses", osplit));
                    #endregion
                    #region Driver Expenses
                    double total5 = 0;
                    double fee5 = 0;
                    DataTable dsplit = ds.DriverExpenses;
                    foreach (var item in objtr.Temp_Split_Expenses.Where(m => m.Index == 1))
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
                    fee5 = total5 * ownerFeeRate;
                    ReportParameter dExFee = new ReportParameter("DExFee", fee5.ToString("N2"));
                    ReportParameter dExTotal = new ReportParameter("DExTotal", total5.ToString("N2"));
                    parameters.Add(dExFee);
                    parameters.Add(dExTotal);
                    ReportViewer1.LocalReport.DataSources.Add(
                    new Microsoft.Reporting.WebForms.ReportDataSource("DriverExpenses", dsplit));
                    #endregion
                    #region EscrowLoan
                    double totalOwner = 0;
                    double totalDriver = 0;
                    if (objtr.Temp_EscrowLoan != null && objtr.Temp_EscrowLoan.Count > 0)
                    {                        
                        DataTable dsEscrowLoanOwner = ds.EscrowLoanOwner;
                        DataTable dsEscrowLoanDriver = ds.EscrowLoanDriver;
                        foreach (var item in objtr.Temp_EscrowLoan)
                        {
                            if (item.Owner)
                            {
                                var dr4 = dsEscrowLoanOwner.NewRow();
                                dr4["Expenses"] = item.Expenses;
                                if (item.CurrentCharge.HasValue)
                                    dr4["CurrentCharge"] = ((double)item.CurrentCharge).ToString("N2");
                                dsEscrowLoanOwner.Rows.Add(dr4);
                                totalOwner += item.CurrentCharge.HasValue ? item.CurrentCharge.Value : 0;
                            }
                            else
                            {
                                var dr4 = dsEscrowLoanDriver.NewRow();
                                dr4["Expenses"] = item.Expenses;
                                if (item.CurrentCharge.HasValue)
                                    dr4["CurrentCharge"] = ((double)item.CurrentCharge).ToString("N2");
                                dsEscrowLoanDriver.Rows.Add(dr4);
                                totalDriver += item.CurrentCharge.HasValue ? item.CurrentCharge.Value : 0;
                            }
                        }
                        ReportParameter EscrowPayment = new ReportParameter("EscrowAmount", totalOwner.ToString("N2"));
                        ReportParameter LoanPayment = new ReportParameter("LoanAmount", totalDriver.ToString("N2"));
                        parameters.Add(EscrowPayment);
                        parameters.Add(LoanPayment);

                        ReportViewer1.LocalReport.DataSources.Add(
                        new Microsoft.Reporting.WebForms.ReportDataSource("EscrowLoanDriver", dsEscrowLoanDriver));

                        ReportViewer1.LocalReport.DataSources.Add(
                        new Microsoft.Reporting.WebForms.ReportDataSource("EscrowLoanOwner", dsEscrowLoanOwner));
                    }
                    #endregion
                    #region Calculate the total
                    double r1 = Math.Round(total1 - fee1 - total2 - fee2 - total3, 2);

                    double ownerPayment = driverinfo.Owner_Pay_Rate * r1;
                    double driverPayment = r1 - ownerPayment;

                    ownerPayment = ownerPayment - total4 - fee4 - totalOwner;
                    driverPayment = driverPayment - total5 - fee5 - totalDriver;

                    ReportParameter PayrollAmount = new ReportParameter("PayrollAmount", Math.Round(r1,2).ToString("N2"));
                    ReportParameter OwnerPayment = new ReportParameter("OwnerPayment", Math.Round(ownerPayment, 2).ToString("N2"));
                    ReportParameter DriverPayment = new ReportParameter("DriverPayment", Math.Round(driverPayment, 2).ToString("N2"));
                    parameters.Add(PayrollAmount);
                    parameters.Add(OwnerPayment);
                    parameters.Add(DriverPayment);
                    if (objtr.DriverPayment != driverPayment)
                    {
                        objtr.DriverPayment = driverPayment;
                        objtr.TotalAmount = r1;
                        db.SaveChanges();
                    }
                    this.ReportViewer1.LocalReport.SetParameters(parameters);
                    ReportViewer1.LocalReport.Refresh();
                    #endregion
                }
                else
                {
                #endregion

                    bool flag = true;
                    if (Session["Payroll"] == null)
                    {
                        flag = false;
                    }
                    #region common prepare
                    int isescrowloan = int.Parse(Request.QueryString["isescrowloan"]);
                    ReportViewer1.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                    if (isescrowloan == 0)
                    {
                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("/Reports/CurrentPayrollNoEscrowLoan.rdlc");
                    }
                    else
                    {
                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("/Reports/CurrentPayroll.rdlc");
                    }
                    ReportViewer1.LocalReport.DataSources.Clear();
                    var parameters = new List<ReportParameter>();
                    var ds = new CurrentPayroll();
                    var db = DB.Entities;
                    int driverid = int.Parse(Request.QueryString["driverid"]);
                    var driverinfo = db.Driver_Info.FirstOrDefault(m => m.ID == driverid);
                    //create a temp report
                    TempReport tr = new TempReport();
                    if (flag)
                    {
                        tr.DriverID = driverid;
                        tr.Date = DateTime.Now;
                        tr.Pay_rate = driverinfo.Pay_rate;
                        tr.fuel___advance_Fee_rate = driverinfo.fuel___advance_Fee_rate;
                        db.TempReport.AddObject(tr);
                        db.SaveChanges();
                    }
                    #endregion
                    #region Trips
                    double total1 = 0;
                    double fee1 = 0;
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
                            if (flag)
                            {
                                Temp_Trip_Info ttrip = new Temp_Trip_Info();
                                ttrip.TripID = item.Trip_ID;
                                ttrip.TempReportID = tr.ID;
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
                                item.Driver_paid = true;
                                db.Temp_Trip_Info.AddObject(ttrip);
                            }
                            //db.ObjectStateManager.ChangeObjectState(item, System.Data.EntityState.Modified);
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
                    //Calculate total fee
                    fee1 = total1 * driverinfo.Pay_rate;
                    ReportParameter driverName = new ReportParameter("DriverName", driverinfo.First_name + " " + driverinfo.Last_name);
                    ReportParameter tripfee = new ReportParameter("TripTotalFee", fee1.ToString("N2"));
                    ReportParameter tripTotal = new ReportParameter("TripTotal", total1.ToString("N2"));
                    parameters.Add(driverName);
                    parameters.Add(tripfee);
                    parameters.Add(tripTotal);
                    ReportViewer1.LocalReport.DataSources.Add(
                    new Microsoft.Reporting.WebForms.ReportDataSource("Trips", trip));
                    #endregion
                    #region Fuel Expenses
                    double total2 = 0;
                    double fee2 = 0;
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
                            if (flag)
                            {
                                Temp_Fuel_Expenses tfuel = new Temp_Fuel_Expenses();
                                tfuel.FuelExpensesID = item.ID;
                                tfuel.TempReportID = tr.ID;
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
                                db.Temp_Fuel_Expenses.AddObject(tfuel);
                                item.Paid_off = true;
                            }

                            // db.ObjectStateManager.ChangeObjectState(item, System.Data.EntityState.Modified);
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

                    fee2 = total2 * driverinfo.fuel___advance_Fee_rate;
                    ReportParameter fuelFee = new ReportParameter("FuelFee", fee2.ToString("N2"));
                    ReportParameter fuelAmount = new ReportParameter("FuelAmount", total2.ToString("N2"));
                    parameters.Add(fuelFee);
                    parameters.Add(fuelAmount);
                    ReportViewer1.LocalReport.DataSources.Add(
                        new Microsoft.Reporting.WebForms.ReportDataSource("FuelExpenses", fuel));
                    #endregion
                    #region Operating Expenses
                    double total3 = 0;
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
                            if (flag)
                            {
                                Temp_Operating_Expenses toperating = new Temp_Operating_Expenses();
                                toperating.OperatingExpensesID = item.ID;
                                toperating.TempReportID = tr.ID;
                                toperating.Amount = item.Amount;
                                toperating.Current_Payroll = item.Current_Payroll;
                                toperating.Date = item.Date;
                                toperating.Description = item.Description;
                                toperating.Driver = item.Driver;
                                toperating.Location = item.Location;
                                toperating.Type = item.Type;
                                toperating.Paid_off = item.Paid_off;
                                db.Temp_Operating_Expenses.AddObject(toperating);
                                item.Paid_off = true;
                            }
                            //db.ObjectStateManager.ChangeObjectState(item, System.Data.EntityState.Modified);
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
                    #region Owner Expenses
                    double total4 = 0;
                    double fee4 = 0;
                    double ownerFeeRate = 0;
                    if (driverinfo.Owner_Name != null)
                    {
                        Owners objOwner = db.Owners.FirstOrDefault(m => m.OwnerID == driverinfo.Owner_Name.Value);
                        if (objOwner != null)
                            if (objOwner.fee_rate != null)
                                ownerFeeRate = (double)objOwner.fee_rate;
                    }

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
                            if (flag)
                            {
                                Temp_Split_Expenses tsowner = new Temp_Split_Expenses();
                                tsowner.SplitExpensesID = item.Id;
                                tsowner.TempReportID = tr.ID;
                                tsowner.Amount = item.Amount;
                                tsowner.Current_Payroll = item.Current_Payroll;
                                tsowner.Date = item.Date;
                                tsowner.Details = item.Details;
                                tsowner.Expenses = item.Expenses;
                                tsowner.Fee_Charged = item.Fee_Charged;
                                tsowner.Index = item.Idndex;
                                tsowner.OwnerDriver = item.OwnerDriver;
                                tsowner.Paid_Off = item.Paid_Off;
                                db.Temp_Split_Expenses.AddObject(tsowner);
                                item.Paid_Off = true;
                            }
                            // db.ObjectStateManager.ChangeObjectState(item, System.Data.EntityState.Modified);
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
                    fee4 = total4 * ownerFeeRate;

                    ReportParameter oExFee = new ReportParameter("OExFee", fee4.ToString("N2"));
                    ReportParameter oExTotal = new ReportParameter("OExTotal", total4.ToString("N2"));
                    parameters.Add(oExFee);
                    parameters.Add(oExTotal);
                    ReportViewer1.LocalReport.DataSources.Add(
                    new Microsoft.Reporting.WebForms.ReportDataSource("OwnerExpenses", osplit));
                    #endregion
                    #region Driver Expenses
                    double total5 = 0;
                    double fee5 = 0;
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
                            if (flag)
                            {
                                Temp_Split_Expenses tsowner = new Temp_Split_Expenses();
                                tsowner.SplitExpensesID = item.Id;
                                tsowner.TempReportID = tr.ID;
                                tsowner.Amount = item.Amount;
                                tsowner.Current_Payroll = item.Current_Payroll;
                                tsowner.Date = item.Date;
                                tsowner.Details = item.Details;
                                tsowner.Expenses = item.Expenses;
                                tsowner.Fee_Charged = item.Fee_Charged;
                                tsowner.Index = item.Idndex;
                                tsowner.OwnerDriver = item.OwnerDriver;
                                tsowner.Paid_Off = item.Paid_Off;
                                db.Temp_Split_Expenses.AddObject(tsowner);
                                item.Paid_Off = true;
                            }
                            //  db.ObjectStateManager.ChangeObjectState(item, System.Data.EntityState.Modified);
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

                    fee5 = total5 * ownerFeeRate;
                    ReportParameter dExFee = new ReportParameter("DExFee", fee5.ToString("N2"));
                    ReportParameter dExTotal = new ReportParameter("DExTotal", total5.ToString("N2"));
                    parameters.Add(dExFee);
                    parameters.Add(dExTotal);
                    ReportViewer1.LocalReport.DataSources.Add(
                    new Microsoft.Reporting.WebForms.ReportDataSource("DriverExpenses", dsplit));
                    #endregion

                    #region EscrowLoan
                    double totalOwner = 0;
                    double totalDriver = 0;
                    if (isescrowloan == 1)
                    {                        
                        DataTable dsEscrowLoanOwner = ds.EscrowLoanOwner;
                        DataTable dsEscrowLoanDriver = ds.EscrowLoanDriver;
                        para = Request.QueryString["escrowLoan"];
                        if (!string.IsNullOrEmpty(para))
                        {
                            string[] lstID = para.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                            List<int> lst = new List<int>();
                            foreach (var item in lstID)
                                lst.Add(int.Parse(item));
                            var lstEscrowloan = db.EscrowLoan.Where(m => lst.Contains(m.ID));
                            foreach (var item in lstEscrowloan)
                            {
                                if (flag)
                                {
                                    Temp_EscrowLoan tEscrowLoan = new Temp_EscrowLoan();
                                    tEscrowLoan.AmountPaid = item.AmountPaid;
                                    tEscrowLoan.TempReportID = tr.ID;
                                    tEscrowLoan.TotalAmount = item.TotalAmount;
                                    tEscrowLoan.OwnerDriver = item.OwnerDriver;
                                    tEscrowLoan.Owner = item.Owner;
                                    tEscrowLoan.EscrowLoanID = item.ID;
                                    tEscrowLoan.Expenses = item.Expenses;
                                    tEscrowLoan.Balance = item.Balance;
                                    tEscrowLoan.CurrentCharge = item.CurrentCharge;
                                    tEscrowLoan.OwnerDriver = item.OwnerDriver;
                                    tEscrowLoan.Escrow_Loan = item.Escrow_Loan;
                                    db.Temp_EscrowLoan.AddObject(tEscrowLoan);
                                }
                                //  //db.ObjectStateManager.ChangeObjectState(item, System.Data.EntityState.Modified);
                                if (item.Owner)
                                {
                                    var dr4 = dsEscrowLoanOwner.NewRow();
                                    dr4["Expenses"] = item.Expenses;
                                    if (item.CurrentCharge.HasValue)
                                        dr4["CurrentCharge"] = ((double)item.CurrentCharge).ToString("N2");
                                    dsEscrowLoanOwner.Rows.Add(dr4);

                                    totalOwner += item.CurrentCharge.HasValue ? item.CurrentCharge.Value : 0;
                                    item.AmountPaid += item.CurrentCharge.HasValue ? item.CurrentCharge.Value : 0;
                                    item.Balance = item.TotalAmount - item.AmountPaid;
                                    item.CurrentCharge = 0;
                                }
                                else
                                {
                                    var dr4 = dsEscrowLoanDriver.NewRow();
                                    dr4["Expenses"] = item.Expenses;
                                    if (item.CurrentCharge.HasValue)
                                        dr4["CurrentCharge"] = ((double)item.CurrentCharge).ToString("N2");
                                    dsEscrowLoanDriver.Rows.Add(dr4);

                                    totalDriver += item.CurrentCharge.HasValue ? item.CurrentCharge.Value : 0;
                                    item.AmountPaid += item.CurrentCharge.HasValue ? item.CurrentCharge.Value : 0;
                                    item.Balance = item.TotalAmount - item.AmountPaid;
                                    item.CurrentCharge = 0;
                                }

                            }
                        }
                        ReportParameter EscrowPayment = new ReportParameter("EscrowAmount", totalOwner.ToString("N2"));
                        ReportParameter LoanPayment = new ReportParameter("LoanAmount", totalDriver.ToString("N2"));
                        parameters.Add(EscrowPayment);
                        parameters.Add(LoanPayment);

                        ReportViewer1.LocalReport.DataSources.Add(
                        new Microsoft.Reporting.WebForms.ReportDataSource("EscrowLoanDriver", dsEscrowLoanDriver));

                        ReportViewer1.LocalReport.DataSources.Add(
                        new Microsoft.Reporting.WebForms.ReportDataSource("EscrowLoanOwner", dsEscrowLoanOwner));
                    }
                    #endregion
                    if (flag)
                    {
                        db.SaveChanges();
                    }

                    #region Calculate the total
                    double r1 = total1 - fee1 - total2 - fee2 - total3;

                    double ownerPayment = driverinfo.Owner_Pay_Rate * r1;
                    double driverPayment = r1 - ownerPayment;

                    ownerPayment = ownerPayment - total4 - fee4 - totalOwner;
                    driverPayment = driverPayment - total5 - fee5 - totalDriver;
                    tr.DriverPayment = driverPayment;
                    tr.TotalAmount = r1;
                    db.SaveChanges();
                    ReportParameter PayrollAmount = new ReportParameter("PayrollAmount", Math.Round(r1,2).ToString("N2"));
                    ReportParameter OwnerPayment = new ReportParameter("OwnerPayment", Math.Round(ownerPayment, 2).ToString("N2"));
                    ReportParameter DriverPayment = new ReportParameter("DriverPayment", Math.Round(driverPayment, 2).ToString("N2"));
                    parameters.Add(PayrollAmount);
                    parameters.Add(OwnerPayment);
                    parameters.Add(DriverPayment);

                    this.ReportViewer1.LocalReport.SetParameters(parameters);
                    ReportViewer1.LocalReport.Refresh();
                    #endregion
                    if (Session["Payroll"] != null)
                    {
                        Session.Remove("Payroll");
                    }
                }
            }
        }
    }
}