using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    [Authorize]
    public class ConfigController : BaseController
    {
        [ValidationFunction(ActionName.ViewSystemConfig)]
        public ActionResult Index()
        {
            return View(DB.Entities.mConfig.ToList());
        }
        //
        // GET: /Owner/Edit/5
        [ValidationFunction(ActionName.ViewSystemConfig)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.mConfig.FirstOrDefault(m => m.ID == id);
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5
        [ValidationFunction(ActionName.NewOrEditItem)]
        [HttpPost]
        public ActionResult NewOrEdit(mConfig model, FormCollection frm)
        {
            try
            {
                var db = DB.Entities;
                if (model.ID == 0)
                {
                    // Edit                    
                    db.mConfig.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("mConfig", model);
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
                        var obj = db.mConfig.FirstOrDefault(m => m.ID == tmpID);
                        db.mConfig.DeleteObject(obj);
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
