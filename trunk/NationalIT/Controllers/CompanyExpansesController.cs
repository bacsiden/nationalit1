using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    [Authorize]
    public class CompanyExpansesController : BaseController
    {
        //
        // GET: /Company_Expanses/
        int pageSize = 20;
        //
        // GET: /Owner/
        [ValidationFunction(ActionName.ViewListCompanyExpanses)]
        public ActionResult Index(int? page)
        {
            return View(DB.Entities.Company_Expanses.OrderByDescending(m => m.CompanyExpansesID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }
        //
        // GET: /Owner/Edit/5
        [ValidationFunction(ActionName.ViewListCompanyExpanses)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var db = DB.Entities;
            var obj = db.Company_Expanses.FirstOrDefault(m => m.CompanyExpansesID == id);
            if (obj == null) obj = new Company_Expanses();
            var lstDis = db.Dispatchers.ToList();
            ViewBag.dataDispatcher = CommonFunction.BuildDropdown(lstDis.Select(m => m.ID.ToString()).ToArray(),
                lstDis.Select(m => m.Last_name + " " + m.First_name).ToArray(), obj.ApprovedBy, "--Select dispatcher--");
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(Company_Expanses model, FormCollection frm)
        {
            var db = DB.Entities;
            try
            {
               
                model.Date = CommonFunction.ChangeFormatDate(frm["Date"]);
                if (model.CompanyExpansesID == 0)
                {
                    // Edit                    
                    db.Company_Expanses.AddObject(model);
                }
                else
                {
                    // Add new
                    db.AttachTo("Company_Expanses", model);
                    db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                var lstDis = db.Dispatchers.ToList();
                ViewBag.dataDispatcher = CommonFunction.BuildDropdown(lstDis.Select(m => m.ID.ToString()).ToArray(),
                    lstDis.Select(m => m.Last_name + " " + m.First_name).ToArray(), model.ApprovedBy, "--Select dispatcher--");
                return View(model);
            }
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
                if (lstID.Length > 0)
                {
                    foreach (var item in lstID)
                    {
                        int tmpID = int.Parse(item);
                        var obj = db.Company_Expanses.FirstOrDefault(m => m.CompanyExpansesID == tmpID);
                        db.Company_Expanses.DeleteObject(obj);
                    }
                    db.SaveChanges();
                }
            }
            catch
            {

            }
            return RedirectToAction("Index");
        }
    }
}
