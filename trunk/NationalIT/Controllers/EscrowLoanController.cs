using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    public class EscrowLoanController : BaseController
    {
        int pageSize = 20;
        //
        // GET: /Owner/
        [ValidationFunction(ActionName.ViewListEscrowLoan)]
        public ActionResult Index(int? page, int? driverID, int? ownerID, string ownerName = "", string driverName = "")
        {
            var list = DB.Entities.EscrowLoan.Where(m => true);
            if(driverID!=null)
                list=list.Where(m => (!m.Owner && m.OwnerDriver == driverID.Value)
                || (ownerID.HasValue && ownerID.Value != 0 && m.Owner && m.OwnerDriver == ownerID.Value));
            if (driverID.HasValue && driverID.Value != 0)
            {
                ViewBag.ForDriver = "of driver: " + driverName;
            }
            if (ownerID.HasValue && ownerID.Value != 0)
            {
                ViewBag.ForOwner = ", owner: " + ownerName;
            }
            return View(list.OrderByDescending(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }
        void SelectOption(EscrowLoan obj)
        {
            #region SELECT Owner or Driver
            string dataDriver_Info = "<option value=''>--Select Driver_Info--</option>";
            foreach (var item in NationalIT.DB.Entities.Driver_Info)
            {
                if (obj != null && !obj.Owner && obj.OwnerDriver == item.ID)
                {
                    dataDriver_Info += string.Format("<option value='{0}' selected='selected'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
                else
                {
                    dataDriver_Info += string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.Last_name, item.First_name);
                }
            }
            ViewBag.dataDriver = dataDriver_Info;

            string dataOwner_Info = "<option value=''>--Select Owner_Info--</option>";
            foreach (var item in NationalIT.DB.Entities.Owners)
            {
                if (obj != null && obj.Owner && obj.OwnerDriver == item.OwnerID)
                {
                    dataOwner_Info += string.Format("<option value='{0}' selected='selected'>{1}</option>", item.OwnerID, item.Name);
                }
                else
                {
                    dataOwner_Info += string.Format("<option value='{0}'>{1}</option>", item.OwnerID, item.Name);
                }
            }
            ViewBag.dataOwner = dataOwner_Info;
            #endregion
        }
        //
        // GET: /Owner/Edit/5
        [ValidationFunction(ActionName.ViewListEscrowLoan)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.EscrowLoan.FirstOrDefault(m => m.ID == id);
            if (obj == null)
            {
                obj = new EscrowLoan();
            }
            SelectOption(obj);
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5
        [ValidationFunction(ActionName.NewOrEditItem)]
        [HttpPost]
        public ActionResult NewOrEdit(EscrowLoan model, FormCollection frm)
        {
            try
            {
                var db = DB.Entities;
                if (model.Owner)
                {
                    model.OwnerDriver = int.Parse(frm["OwnerDriver2"]);
                }
                if (model.ID == 0)
                {
                    // Edit                    
                    db.EscrowLoan.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("EscrowLoan", model);
                    db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                SelectOption(model);
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
                        var obj = db.EscrowLoan.FirstOrDefault(m => m.ID == tmpID);
                        db.EscrowLoan.DeleteObject(obj);
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
