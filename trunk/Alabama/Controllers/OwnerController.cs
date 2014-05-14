using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace Alabama.Controllers
{
    public class OwnerController : Controller
    {
        int pageSize = 20;
        //
        // GET: /Owner/

        public ActionResult Index(int ?page)
        {
            return View(DB.Entities.Owners.OrderByDescending(m => m.OwnerID).ToPagedList(!page.HasValue ? 0 : page.Value, pageSize));
        }
        //
        // GET: /Owner/Edit/5
 
        public ActionResult NewOrEdit(int ? id=0)
        {
            var obj = DB.Entities.Owners.FirstOrDefault(m=>m.OwnerID==id);
            return View(obj);
        }

        //
        // POST: /Owner/Edit/5

        [HttpPost]
        public ActionResult NewOrEdit(Owners model)
        {
            try
            {
                var db = DB.Entities;

                if (model.OwnerID==0)
                {
                    // Edit                    
                    db.Owners.AddObject(model);                    
                }
                else
                {
                    // Add new
                    var obj = db.Owners.FirstOrDefault(m=>m.OwnerID==model.OwnerID);
                    obj.Name = model.Name;
                    obj.pay_rate = model.pay_rate;
                    obj.fee_rate = model.fee_rate;
                    obj.ssn_ = model.ssn_;
                    db.ObjectStateManager.ChangeObjectState(obj, System.Data.EntityState.Modified);
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

        public ActionResult Delete(string arrayID = "")
        {
            try
            {
                // TODO: Add delete logic here
                var lstID = arrayID.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                var db = DB.Entities;
                if (lstID.Length>0)
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
