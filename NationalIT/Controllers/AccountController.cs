using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using NationalIT.Models;
using System.Data.SqlClient;
using System.Data;

namespace NationalIT.Controllers
{
    public class UserDAL : DB.BaseClass<User>
    {
        public const string ADMIN = "admin";
        public List<string> GetListFunctionCodeByUsername(string username)
        {
            List<string> listCode = new List<string>();
            try
            {
                //var db = DB.Entities;                
                //var list = (from p in db.V_Function
                //            join
                //            q in db.V_FunctionInRole
                //            on p.ID equals q.FunctionID
                //            where q.V_Role.V_UserInRole.FirstOrDefault(m => m.V_User.UserName == username) != null
                //            select p).ToList();

                //if (list != null && list.Count() > 0)
                //{
                //    foreach (var item in list)
                //    {
                //        listCode.Add(item.Code);
                //    }
                //}
                var db = DB.Entities;
                var list = db.Function.Where(m => m.Role1.FirstOrDefault(n => n.Group.FirstOrDefault(x => x.User.FirstOrDefault(y => y.UserName == username) != null) != null) != null);
                if (list != null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        listCode.Add(item.Code);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return listCode;
        }
        public List<int> GetMenuIDByUsername(string username)
        {
            List<int> listID = new List<int>();
            try
            {
                var db = DB.Entities;
                var list = db.Menu.Where(n => n.Group.FirstOrDefault(x => x.User.FirstOrDefault(y => y.UserName == username) != null) != null).ToList();
                if (list != null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        listID.Add(item.ID);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return listID;
        }
        public User GetUserByUserName(string userName)
        {
            var context = DB.Entities;
            return context.User.FirstOrDefault(m => m.UserName.ToLower() == userName.ToLower());
        }
        /// <summary>
        /// Reset password (dành cho Admin muốn reset mật khẩu của member)
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool ChangePassword(string userName, string newPassword)
        {
            try
            {
                MembershipUser aspnetUser = Membership.GetUser(userName);
                string resetPassword = aspnetUser.ResetPassword();
                return aspnetUser.ChangePassword(resetPassword, newPassword);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Đổi mật khẩu yêu cầu nhập vào mật khẩu cũ (dành cho member sau khi đăng nhập)
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            try
            {
                MembershipUser aspnetUser = Membership.GetUser(userName);
                return aspnetUser.ChangePassword(oldPassword, newPassword);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Guid CreateAspnetUser(string username, string password)
        {
            try
            {
                MembershipUser aspnetUser = Membership.CreateUser(username, password);
                return (Guid)aspnetUser.ProviderUserKey;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public User GetUserByID(int id)
        {
            return GetByID(id);
        }
        public User GetCurrentUser
        {
            get
            {
                MembershipUser mbsUser = Membership.GetUser();
                if (mbsUser != null)
                {
                    Guid id = (Guid)mbsUser.ProviderUserKey;
                    return DB.Entities.User.FirstOrDefault(m => m.AspnetUserID == id);
                }
                return null;
            }
        }
        public void LockUserByID(int id)
        {
            var user = GetUserByID(id);
            if (UserDAL.ADMIN.Equals(user.UserName))
            {
                return;
            }
            user.Locked = true;
            Update(user);
        }
    }

    public class AccountController : BaseController
    {

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Account/LogOn
        // **************************************


        [Authorize]
        public ActionResult Index(int id = 0)
        {
            try
            {
                if (id == 0)
                {
                    @ViewBag.GroupName = "List User";
                    return View(DB.Entities.User);
                }
                else
                {
                    var db = DB.Entities;
                    var g = db.Group.First(m => m.ID == id);
                    //@ViewBag.GroupName = "Tên nhóm: <a href='" + HttpContext.Request.Url + "'>" + g.Title + "</a>";
                    @ViewBag.GroupName = g.Title;
                    return View(db.User.Where(m => m.Group.FirstOrDefault(x => x.ID == id) != null));
                }
            }
            catch (Exception ex)
            {
                return View(new List<User>());
            }
        }

        [Authorize]
        public ActionResult NewOrEdit(int? id = 0)
        {
            var obj = DB.Entities.User.FirstOrDefault(m => m.ID == id);
            ViewBag.Title = "Edit user";
            if (obj == null)
            {
                ViewBag.Title = "Add user";
                obj = new User();
            }
            return View(obj);
        }

        [HttpPost]
        [Authorize]
        public ActionResult NewOrEdit(User model, string password)
        {
            try
            {
                var db = DB.Entities;
                if (model.ID == 0)
                {
                    // Add new   
                    var aspNewUserID = new UserDAL().CreateAspnetUser(model.UserName, password);
                    model.AspnetUserID = aspNewUserID;
                    db.User.AddObject(model);
                }
                else
                {
                    // Edit    
                    db.AttachTo("User", model);
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

        [Authorize]
        public ActionResult DeleteByListID(string arrayID = "")
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
                        var obj = db.User.FirstOrDefault(m => m.ID == tmpID);
                        if (UserDAL.ADMIN.Equals(obj.UserName))
                        {
                            continue;
                        }
                        new UserDAL().Delete(tmpID);
                    }
                    db.SaveChanges();
                }
            }
            catch
            {

            }
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult LockByListID(string arrayID = "")
        {
            try
            {
                // TODO: Add delete logic here
                var lstID = arrayID.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var db = DB.Entities;
                var userDAL = new UserDAL();
                if (lstID.Length > 0)
                {
                    foreach (var item in lstID)
                    {
                        // Thực hiện khóa tài khoản người dùng
                        userDAL.LockUserByID(int.Parse(item));
                    }
                }
            }
            catch
            {

            }
            return RedirectToAction("Index");
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = new UserDAL().GetUserByUserName(model.UserName);
                if (user != null && !user.Locked)
                {
                    if (MembershipService.ValidateUser(model.UserName, model.Password))
                    {
                        FormsService.SignIn(model.UserName, model.RememberMe);
                        ListFunctionCode = new UserDAL().GetListFunctionCodeByUsername(model.UserName);
                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "DriverInfo");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Username or password incorrect.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Username or password incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        [Authorize]
        public ActionResult LogOff()
        {
            FormsService.SignOut();

            return RedirectToAction("Index", "DriverInfo");
        }

        // **************************************
        // URL: /Account/Register
        // **************************************

        [Authorize]
        public ActionResult Register()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            // DropDown Bưu cục
            var db = DB.Entities;
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var db = DB.Entities;
                // Attempt to register the user
                if (db.User.FirstOrDefault(m => m.UserName.Equals(model.UserName)) == null)
                {


                    MembershipUser aspnetUser = Membership.CreateUser(model.UserName, model.Password, model.Email);
                    Guid userCreated = (Guid)aspnetUser.ProviderUserKey;
                    if (userCreated != null)
                    {

                        db.User.AddObject(new User() { UserName = model.UserName, Email = model.Email, PhoneNumber = model.Phone, Address = model.Address, AspnetUserID = userCreated, Name = model.Name });
                        db.SaveChanges();
                        FormsService.SignIn(model.UserName, false /* createPersistentCookie */);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Register unsuccesfully!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tài khoản đã tồn tại!");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        //[Authorize]
        //public ActionResult ChangePassword()
        //{
        //    ViewBag.PasswordLength = MembershipService.MinPasswordLength;
        //    return View();
        //}

        //[Authorize]
        //[HttpPost]
        //public ActionResult ChangePassword(ChangePasswordModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
        //        {
        //            return RedirectToAction("ChangePasswordSuccess");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    ViewBag.PasswordLength = MembershipService.MinPasswordLength;
        //    return View(model);
        //}

        [Authorize]
        public ActionResult ChangePassword()
        {
            return PartialView("_ChangePass");
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(Models.ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    var userDAL = new UserDAL();
                    bool succeeded = userDAL.ChangePassword(CurrentUser.UserName, model.OldPassword, model.NewPassword);
                    if (succeeded)
                    {
                        return JavaScript(@"noticeChangePassWord(true, 'Change password succesfully.');");
                    }
                    else
                    {
                        ModelState.AddModelError("OldPassword", "OldPassword is invailid");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("NewPassword", "Change password error");
                }

            }
            // If we got this far, something failed, redisplay form
            return PartialView("_ChangePass", model);
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        [Authorize]
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }






    }
}
