using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NationalIT.Controllers
{
    public class GroupController : BaseController
    {
        [Authorize]
        [ValidationFunction("/Home/index", ActionName.VIEWLISTGROUP)]
        public ActionResult Index()
        {
            return View(DB.Entities.mGroup);
        }

        [Authorize]
        [ValidationFunction("/Group/index", ActionName.ADDUSERFORGROUP)]
        public ActionResult UsersInGroup(int id = 0)
        {
            var db = DB.Entities;
            var g = db.mGroup.First(m => m.ID == id);
            if (g == null)
                return RedirectToAction("Index");

            ViewBag.GoupName = g.Title;
            ViewBag.GroupID = id;
            return View(DB.Entities.mUser.Where(m => m.mGroup.FirstOrDefault(x => x.ID == g.ID) != null));
        }

        [Authorize]
        [ValidationFunction("/Group/index", ActionName.ADDUSERFORGROUP)]
        public ActionResult RemoveUser(int id, int groupID)
        {
            var db = DB.Entities;
            var user = db.mUser.First(m => m.ID == id);
            var group = db.mGroup.First(m => m.ID == groupID);
            user.mGroup.Remove(group);
            db.SaveChanges();
            return RedirectToAction("UsersInGroup", new { id = groupID });
        }

        [Authorize]
        [ValidationFunction("/Group/index", ActionName.ADDUSERFORGROUP)]
        public ActionResult AddUser(int id)
        {
            ViewBag.GroupID = id;
            ViewBag.GroupName = DB.Entities.mGroup.First(m => m.ID == id).Title;
            return View(new mUser());
        }

        [Authorize]
        [ValidationFunction("/Group/index", ActionName.ADDUSERFORGROUP)]
        public ActionResult DoAddUser(mUser objUser, int groupID)
        {
            try
            {
                var db = DB.Entities;
                var user = db.mUser.FirstOrDefault(m => m.UserName == objUser.UserName);
                var group = db.mGroup.First(m => m.ID == groupID);
                user.mGroup.Add(group);
                db.SaveChanges();
                return RedirectToAction("UsersInGroup", new { id = groupID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Tài khoản bạn nhập vào không đúng.");
                return View("AddUser", objUser);
            }
        }

        [Authorize]
        public ActionResult Details(int id = 0)
        {
            var obj = DB.Entities.mGroup.First(m => m.ID == id);
            if (obj == null)
                return RedirectToAction("Index");
            return View(obj);
        }

        [Authorize]
        [ValidationFunction("/Group/index", ActionName.ADDNEWGROUP)]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidationFunction("/Group/index", ActionName.ADDNEWGROUP)]
        public ActionResult Create(mGroup model)
        {
            try
            {
                // TODO: Add insert logic here
                var db = DB.Entities;
                var group = new mGroup() { Title = model.Title };
                db.mGroup.AddObject(group);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [Authorize]
        [ValidationFunction("/Group/index", ActionName.EDITGROUP)]
        public ActionResult Edit(int id = 0)
        {
            var obj = DB.Entities.mGroup.First(m => m.ID == id);
            if (obj == null)
                return RedirectToAction("Index");
            return View(obj);
        }

        [HttpPost]
        [Authorize]
        [ValidationFunction("/Group/index", ActionName.EDITGROUP)]
        public ActionResult Edit(mGroup model)
        {
            try
            {
                // TODO: Add update logic here
                var db = DB.Entities;
                var obj = db.mGroup.First(m => m.ID == model.ID);
                obj.Title = model.Title;
                db.ObjectStateManager.ChangeObjectState(obj, System.Data.EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [Authorize]
        [ValidationFunction("/Group/index", ActionName.DELETEGROUP)]
        public ActionResult Delete(int id)
        {
            try
            {
                var db = DB.Entities;
                var obj = db.mGroup.First(m => m.ID == id);
                db.mGroup.DeleteObject(obj);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        [ValidationFunction("/Group/index", ActionName.ADDROLESFORGROUP)]
        public ActionResult Role(int id)
        {
            try
            {
                var db = DB.Entities;
                var lst = db.mRole.Where(m => m.mGroup.FirstOrDefault(n => n.ID == id) != null);
                string s = "";
                foreach (var item in db.mRole)
                {
                    string check = "";
                    if (lst.FirstOrDefault(m => m.ID == item.ID) != null)
                    {
                        check = "checked='checked'";
                    }
                    s += "<label class='checkbox'><input type='checkbox' " + check + " value='" + item.ID + "' />" + item.Name + "</label>";
                }
                ViewBag.listRole = s;
                return View(db.mGroup.FirstOrDefault(m => m.ID == id));

            }
            catch (Exception)
            {
                return View();
            }
        }

        [Authorize]
        [ValidationFunction("/Group/index", ActionName.ADDROLESFORGROUP)]
        public ActionResult DoRole(int groupID, string listCheck)
        {
            try
            {
                var db = DB.Entities;
                var group = db.mGroup.FirstOrDefault(m => m.ID == groupID);
                string[] listChecked = listCheck.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in db.mRole)
                {
                    if (listChecked.Contains(item.ID.ToString()))
                    {
                        if (group.mRole.FirstOrDefault(m => m.ID == item.ID) == null)
                        {
                            group.mRole.Add(item);
                        }
                    }
                    else
                        if (group.mRole.FirstOrDefault(m => m.ID == item.ID) != null)
                        {
                            group.mRole.Remove(item);
                        }
                }

                db.SaveChanges();
                return RedirectToAction("index");
            }
            catch (Exception)
            {
                return View();
            }
        }
        //[Authorize]
        //[ValidationFunction("/Group/index", ActionName.ADDMENUFORGROUP)]
        //public ActionResult Menu(int id)
        //{
        //    try
        //    {
        //        var db = DB.Entities;
        //        var lst = db.mMenu.Where(m => m.mGroup.FirstOrDefault(n => n.ID == id) != null);
        //        string s = "";
        //        foreach (var item in db.mMenu)
        //        {
        //            string check = "";
        //            if (lst.FirstOrDefault(m => m.ID == item.ID) != null)
        //            {
        //                check = "checked='checked'";
        //            }
        //            s += "<label class='checkbox'><input type='checkbox' " + check + " value='" + item.ID + "' />" + item.Title + "</label>";
        //        }
        //        ViewBag.listMenu = s;
        //        return View(db.mGroup.FirstOrDefault(m => m.ID == id));

        //    }
        //    catch (Exception)
        //    {
        //        return View();
        //    }
        //}
        //[Authorize]
        //[HttpPost]
        //[ValidationFunction("/Group/index", ActionName.ADDMENUFORGROUP)]
        //public ActionResult Menu(int groupID, string listCheck)
        //{
        //    try
        //    {
        //        var db = DB.Entities;
        //        var group = db.mGroup.FirstOrDefault(m => m.ID == groupID);
        //        string[] listChecked = listCheck.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        foreach (var item in db.mMenu)
        //        {
        //            if (listChecked.Contains(item.ID.ToString()))
        //            {
        //                if (group.mMenu.FirstOrDefault(m => m.ID == item.ID) == null)
        //                {
        //                    group.mMenu.Add(item);
        //                }
        //            }
        //            else
        //                if (group.mMenu.FirstOrDefault(m => m.ID == item.ID) != null)
        //                {
        //                    group.mMenu.Remove(item);
        //                }
        //        }

        //        db.SaveChanges();
        //        return RedirectToAction("index");
        //    }
        //    catch (Exception)
        //    {
        //        return View();
        //    }
        //}
    }
}
