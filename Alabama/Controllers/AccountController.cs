﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Alabama.Models;
using System.Data.SqlClient;
using System.Data;

namespace Alabama.Controllers
{
    public class UserDAL : DB.BaseClass<User>
    {
        public const string ADMIN = "huyenvv";
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


        public ActionResult Index(int id = 0)
        {
            try
            {
                if (id == 0)
                {
                    @ViewBag.GroupName = "Danh sách người dùng";
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


        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
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
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            FormsService.SignOut();

            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/Register
        // **************************************

        public ActionResult Register()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            // DropDown Bưu cục
            var db = DB.Entities;
            List<BuuCuc> lstBuuCuc = new List<BuuCuc>();
            var obj = new BuuCuc() { ID = 0, TenBuuCuc = "- Chọn bưu cục -" };
            lstBuuCuc.Add(obj);
            lstBuuCuc.AddRange(db.BuuCuc);
            var selectListBuuCuc = new SelectList(lstBuuCuc, "ID", "TenBuuCuc");
            ViewBag.SelectListBuuCuc = selectListBuuCuc;
            return View();
        }

        [HttpPost]
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

                        db.User.AddObject(new User() { UserName = model.UserName, Email = model.Email, PhoneNumber = model.Phone, Address = model.Address, AspnetUserID = userCreated, UserKindID = 1, BuuCucID = model.BuuCucID, Name = model.Name });
                        db.SaveChanges();
                        FormsService.SignIn(model.UserName, false /* createPersistentCookie */);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Đăng ký không thành công!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tài khoản đã tồn tại!");
                }
            }
            // DropDown Bưu cục
            List<BuuCuc> lstBuuCuc = new List<BuuCuc>();
            var obj = new BuuCuc() { ID = 0, TenBuuCuc = "- Chọn bưu cục -" };
            lstBuuCuc.Add(obj);
            lstBuuCuc.AddRange(DB.Entities.BuuCuc);
            var selectListBuuCuc = new SelectList(lstBuuCuc, "ID", "TenBuuCuc");
            ViewBag.SelectListBuuCuc = selectListBuuCuc;

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        public Models.Account Get(int id)
        {
            try
            {
                DataTable dt = DataUtilities.GetTable("Account_GetByID", CommandType.StoredProcedure, "@ID", id);
                return new Models.Account(dt);
            }
            catch
            {
                return null;
            }
        }

        public Models.Account Get(string user, string pass)
        {
            try
            {
                DataTable dt = DataUtilities.GetTable("Account_Select_User_Pass",
                CommandType.StoredProcedure, "@username", user, "@pass", pass);
                return new Models.Account(dt);
            }
            catch
            {
                return null;
            }
        }

        public Models.Account Get(string user)
        {
            try
            {
                DataTable dt = DataUtilities.GetTable("Account_Select_UserName",
                CommandType.StoredProcedure, "@username", user);
                return new Models.Account(dt);
            }
            catch
            {
                return null;
            }
        }

        public DataTable Gets(string name) // Gat table of Account has the similar name
        {
            try
            {
                return DataUtilities.GetTable("Account_Select_Name",
                    CommandType.StoredProcedure, "@username", name);
            }
            catch
            {
                return null;
            }
        }

        public int GetGroup(string user, string pass)
        {
            try
            {
                Models.Account a = Get(user, pass);
                return a.GroupRole;
            }
            catch
            {
                return 10;
            }
        }

        public object ChangeBalance(string userName, int changeType, int money, int transactionID)
        {
            SqlParameter[] pars = new SqlParameter[4];
            pars[0] = new SqlParameter("@username", userName);
            pars[1] = new SqlParameter("@changetypeid", changeType);
            pars[2] = new SqlParameter("@money", money);
            pars[3] = new SqlParameter("@transactionid", transactionID);
            return DataUtilities.ExcuteScalar("Account_ChangeBalance",
                CommandType.StoredProcedure, pars);
        }

        public void Insert(Models.Account ac)
        {
            SqlParameter[] pars = new SqlParameter[13];
            pars[0] = new SqlParameter("@UserName", ac.UserName);
            pars[1] = new SqlParameter("@Pass", ac.Pass);
            pars[2] = new SqlParameter("@Name", ac.Name);
            pars[3] = new SqlParameter("@Email", ac.Email);
            pars[4] = new SqlParameter("@SexID", ac.SexID);
            pars[5] = new SqlParameter("@Birthday", ac.Birthday);
            pars[6] = new SqlParameter("@Address", ac.Address);
            pars[7] = new SqlParameter("@Balance", ac.Balance);
            pars[8] = new SqlParameter("@VBalance", ac.VBalance);
            pars[9] = new SqlParameter("@Point", ac.Point);
            pars[10] = new SqlParameter("@FailCard", ac.FailCard);
            pars[11] = new SqlParameter("@GroupRole", ac.GroupRole);
            pars[12] = new SqlParameter("@Status", ac.Status);

            DataUtilities.ExcuteNonQuery("Account_Insert",
                CommandType.StoredProcedure, pars);
        }

        public void Update(Models.Account ac)
        {
            SqlParameter[] pars = new SqlParameter[14];
            pars[0] = new SqlParameter("@UserName", ac.UserName);
            pars[1] = new SqlParameter("@Pass", ac.Pass);
            pars[2] = new SqlParameter("@Name", ac.Name);
            pars[3] = new SqlParameter("@Email", ac.Email);
            pars[4] = new SqlParameter("@SexID", ac.SexID);
            pars[5] = new SqlParameter("@Birthday", ac.Birthday);
            pars[6] = new SqlParameter("@Address", ac.Address);
            pars[7] = new SqlParameter("@Balance", ac.Balance);
            pars[8] = new SqlParameter("@VBalance", ac.VBalance);
            pars[9] = new SqlParameter("@Point", ac.Point);
            pars[10] = new SqlParameter("@FailCard", ac.FailCard);
            pars[11] = new SqlParameter("@GroupRole", ac.GroupRole);
            pars[12] = new SqlParameter("@Status", ac.Status);
            pars[13] = new SqlParameter("@ID", ac.ID);

            DataUtilities.ExcuteNonQuery("Account_Update",
                CommandType.StoredProcedure, pars);
        }

        public int Del(int id)
        {
            return DataUtilities.ExcuteNonQuery("Account_Delete", CommandType.StoredProcedure, "@ID", id);
        }

        public ActionResult DoAddCard(FormCollection collection)
        {
            try
            {
                string user = collection["txtUserName"];
                string cardChar = collection["cardChar"];
                DateTime date = DateTime.Now;
                return RedirectToAction("AddCard");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult AddCard()
        {
            return View();
        }

        public ActionResult AddMoney()
        {
            return View();
        }

        public ActionResult NganLuongRedirect(FormCollection collection)
        {
            Session["Transaction"] = 1;

            ////Bat loi java script
            ///////////////////////////////////////////////////////////

            //Hiển thị tổng giá tiền ở cuối trang và nơi đặt nút thanh toán
            String return_url = ConfigInfo.NganLuong_ReturnUrl;// Địa chỉ trả về 
            String transaction_info = "1"; //transaction ID
            String order_code = collection["txtUserName"];//Mã giỏ hàng
            String receiver = ConfigInfo.NganLuong_ReciveEmail;//Tài khoản nhận tiền
            String price = collection["money"]; //ShoppingCart.Instance.GetSubTotal().ToString();//Lấy giá của giỏ hàng


            ////Tao transaction ID
            //string pass = "Ngọc Châu";
            //pass += "^" + DateTime.Now.ToString();

            //VPSS.CheckOut.Secure secure = new VPSS.CheckOut.Secure();
            //string sp = secure.EncryptToBase64String(pass, VPSS.PublicKeySystem.VPSS);

            //int trans = new VPSSServices.AllServicesSoapClient().GetTran(sp);
            //if (trans < 0)
            //    Response.Write("TIME OUT!");

            //transaction_info = trans.ToString();

            //tao request string
            NL_Checkout nl = new NL_Checkout();
            String url;
            url = nl.buildCheckoutUrl(return_url, receiver, transaction_info, order_code, price);
            if (!url.StartsWith("http://"))
                url = "http://" + url;
            //Chuyển sang cổng thanh toán ngân lượng
            Response.Redirect(url);
            //ImageButton imgBtn = new ImageButton();
            //imgBtn.ImageUrl = "https://www.nganluong.vn/data/images/buttons/11.gif";//source file ảnh
            //imgNganLuong.PostBackUrl = url;//Gán địa chỉ url cho nút thanh toán
            return View();
        }
    }
}
