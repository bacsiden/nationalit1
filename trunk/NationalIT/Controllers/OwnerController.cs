using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace NationalIT.Controllers
{
    [Authorize]    
    public class OwnerController : BaseController
    {
        int pageSize = 20;
        //
        // GET: /Owner/

        [ValidationFunction(ActionName.ViewListOwners)]
        public ActionResult Index(int? page)
        {
            return View(DB.Entities.Owners.OrderBy(m=>m.OwnerID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }
        //
        // GET: /Owner/Edit/5
        [ValidationFunction(ActionName.ViewListOwners)]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.Owners.FirstOrDefault(m => m.OwnerID == id);
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5
        [ValidationFunction(ActionName.NewOrEditItem)]
        [HttpPost]
        public ActionResult NewOrEdit(Owners model)
        {
            try
            {
                var db = DB.Entities;

                if (model.OwnerID == 0)
                {
                    // Edit                    
                    db.Owners.AddObject(model);
                }
                else
                {
                    // Add new      
                    db.AttachTo("Owners", model);
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
                        var obj = db.Owners.FirstOrDefault(m => m.OwnerID == tmpID);
                        db.Owners.DeleteObject(obj);
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
