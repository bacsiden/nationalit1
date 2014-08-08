using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    [Authorize]
    public class DispatchersController : BaseController
    {
        int pageSize = 20;
        //
        // GET: /Owner/
        [ValidationFunction(ActionName.ViewListDispatchers)]
        public ActionResult Index(int? page)
        {
            return View(DB.Entities.Dispatchers.OrderByDescending(m => m.ID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }
        //
        // GET: /Owner/Edit/5
        [ValidationFunction(ActionName.ViewListDispatchers)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.Dispatchers.FirstOrDefault(m => m.ID == id);
            if (obj == null) obj = new Dispatchers();
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        [ValidationFunction(ActionName.NewOrEditItem)]
        public ActionResult NewOrEdit(Dispatchers model, FormCollection frm)
        {
            try
            {
                var db = DB.Entities;
                model.Hire_date = CommonFunction.ChangeFormatDate(frm["Hire_date"]);
                if (model.ID == 0)
                {
                    // Edit                    
                    db.Dispatchers.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("Dispatchers", model);
                    db.ObjectStateManager.ChangeObjectState(model, System.Data.EntityState.Modified);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
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
                        var obj = db.Dispatchers.FirstOrDefault(m => m.ID == tmpID);
                        db.Dispatchers.DeleteObject(obj);
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
