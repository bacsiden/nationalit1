using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.IO;
using System.Web.Security;
using System.EnterpriseServices;
using System.ComponentModel;
using NationalIT.Controllers;

namespace NationalIT
{
    public class BaseController : Controller
    {
        //protected override ViewResult View(IView view, object model)
        //{
        //    ViewBag.LanguageCode = CurrentLanguageCode;
        //    ViewBag.LayoutUrl = _currentLayoutURL;
        //    return base.View(view, model);
        //}

        //protected override ViewResult View(string viewName, string masterName, object model)
        //{
        //    ViewBag.LanguageCode = CurrentLanguageCode;
        //    ViewBag.LayoutUrl = _currentLayoutURL;
        //    return base.View(viewName, masterName, model);
        //}
        public enum ActionName
        {
            // Khai báo các quyền tương ứng với bảng Function
            [System.ComponentModel.Description("Login")]
            LOGIN,
            [System.ComponentModel.Description("Log out")]
            LOGOUT,

            #region Quản lý menu
            [System.ComponentModel.Description("Add new menu")]
            ADDNEWMENU,
            [System.ComponentModel.Description("View list menu")]
            VIEWLISTMENU,
            [System.ComponentModel.Description("Edit menu")]
            EDITMENU,
            [System.ComponentModel.Description("Delete menu")]
            DELETEMENU,
            #endregion

            #region Quản lý user and group
            [System.ComponentModel.Description("Add new GROUP")]
            ADDNEWGROUP,
            [System.ComponentModel.Description("View list GROUP")]
            VIEWLISTGROUP,
            [System.ComponentModel.Description("Edit GROUP")]
            EDITGROUP,
            [System.ComponentModel.Description("Add user to GROUP")]
            ADDUSERFORGROUP,
            [System.ComponentModel.Description("Add role to GROUP")]
            ADDROLESFORGROUP,           
            [System.ComponentModel.Description("Delete GROUP")]
            DELETEGROUP,
            #endregion

            #region Quản lý vai trò
            [System.ComponentModel.Description("Add new ROLE")]
            ADDNEWROLE,
            [System.ComponentModel.Description("View list ROLE")]
            VIEWLISTROLE,
            [System.ComponentModel.Description("Edit ROLE")]
            EDITROLE,
            [System.ComponentModel.Description("Add funtion to ROLE")]
            ADDFUNCTIONFORROLE,
            [System.ComponentModel.Description("Delete ROLE")]
            DELETEROLE,
            #endregion

            #region Quản lý function
            [System.ComponentModel.Description("Add menu to FUNCTION")]
            ADDMENUFORFUNCTION,
            [System.ComponentModel.Description("View List FUNCTION")]
            VIEWLISTFUNCTION,
            #endregion

            #region Nghiệp vụ
            [System.ComponentModel.Description("Driver info managerment")]
            DRIVERINFOMANAGERMENT,
            [System.ComponentModel.Description("Trip info managerment")]
            TRIPINFOMANAGERMENT,
            [System.ComponentModel.Description("Owner managerment")]
            OWNERMANAGERMENT,
            [System.ComponentModel.Description("Operating expanses managerment")]
            OPERATINGEXPANSESMANAGERMENT,
            [System.ComponentModel.Description("Dispatchers managerment")]
            DISPATCHERSMANAGERMENT,
            [System.ComponentModel.Description("Equipment managerment")]
            EQUIPMENTMANAGERMENT,
            [System.ComponentModel.Description("Fuel Expenses managerment")]
            FUELEXPENSESMANAGERMENT,
            [System.ComponentModel.Description("Income managerment")]
            INCOMEMANAGERMENT,
            [System.ComponentModel.Description("Maintenance managerment")]
            MAINTENACEMANAGERMENT,
            [System.ComponentModel.Description("Violations managerment")]
            VIOLATIONSMANAGERMENT,
            #endregion

        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            //Tránh trường hợp sau khi logoff người sử dụng nhấn vào nút Back trên trình duyệt sẽ trở về trang cũ
            Response.AddHeader("pragma", "no-cache");
            Response.AddHeader("cache-control", "private");
            Response.CacheControl = "no-cache";
            Response.Cache.SetExpires(DateTime.Now.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
        }
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
        public sealed class ValidationFunctionAttribute : ActionFilterAttribute
        {
            /// <summary>
            /// Quyền được phép truy cập
            /// </summary>
            public string AllowFunctionCodes { get; set; }

            public string UrlRedirect { get; set; }


            public ValidationFunctionAttribute(ActionName allowFunctionCode)
            {
                AllowFunctionCodes = allowFunctionCode.ToString();
            }


            public ValidationFunctionAttribute(string urlRedirect, ActionName allowFunctionCode)
            {
                UrlRedirect = urlRedirect;
                AllowFunctionCodes = allowFunctionCode.ToString();
            }
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                bool isValid = true;
                var user = new UserDAL().GetCurrentUser;
                if (user.Locked)
                {
                    new AccountController().LogOff();
                    filterContext.Result = new RedirectResult("/BuuKien/Index");
                }
                else
                {
                    if (!user.UserName.Equals(UserDAL.ADMIN, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(AllowFunctionCodes))
                    {
                        isValid = true;
                        if (!IsFunctionInRole(AllowFunctionCodes))
                        {
                            isValid = false;
                        }
                    }
                    if (!isValid)
                    {
                        if (String.IsNullOrEmpty(UrlRedirect))
                        {
                            filterContext.Result = new RedirectResult("/BuuKien/Index");
                        }
                        else
                        {
                            filterContext.Result = new RedirectResult(UrlRedirect);
                        }
                    }
                }

                base.OnActionExecuting(filterContext);
                return;
            }
            public bool IsFunctionInRole(string code)
            {
                var functionCodeList = new List<string>();
                if (System.Web.HttpContext.Current.Session["ListFunctionCode"] != null)
                {
                    functionCodeList = (List<string>)System.Web.HttpContext.Current.Session["ListFunctionCode"];
                }
                else
                {
                    functionCodeList = new UserDAL().GetListFunctionCodeByUsername(System.Web.HttpContext.Current.User.Identity.Name);
                }

                if (functionCodeList != null)
                {
                    if (functionCodeList.Contains(code))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public mUser CurrentUser
        {
            get
            {
                return new UserDAL().GetCurrentUser;
            }
        }
        public ActionResult ViewHome
        {
            get
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public string GetView(string actionName)
        {
            //string site_language = (string)Request.RequestContext.RouteData.Values["site_language"];
            //if (site_language != (string)Session[Constant.SESSION_CURRENT_SITE_LANGUAGE_CODE])
            //{
            //    if (site_language == null)
            //    {
            //        Session[Constant.SESSION_CURRENT_SITE_CODE] = null;
            //        Session[Constant.SESSION_CURRENT_LANGUAGE_CODE] = Constant.DefaultSiteLanguage;
            //        return actionName;
            //    }
            //    else
            //    {
            //        string s = site_language.ToString().Trim().ToLower();
            //        string s1 = s.Replace("admin", null);
            //        if (s != s1) Session[Constant.SESSION_CURRENT_SITE_CODE] = null;
            //        if (!string.IsNullOrEmpty(s1)) Session[Constant.SESSION_CURRENT_LANGUAGE_CODE] = s1.Replace("-", null);
            //    }
            //}
            //if (Session[Constant.SESSION_CURRENT_SITE_CODE] == null)
            //    return actionName;
            return "Admin/" + actionName;
        }

        public int PageSize
        {
            get
            {
                return int.Parse(System.Configuration.ConfigurationManager.AppSettings["PageSize"]);
            }
        }
        public List<string> ListFunctionCode
        {
            get
            {
                if (Session["ListFunctionCode"] == null)
                {
                    Session["ListFunctionCode"] = new UserDAL().GetListFunctionCodeByUsername(CurrentUser.UserName);
                }
                return (List<string>)Session["ListFunctionCode"];
            }
            set
            {
                Session["ListFunctionCode"] = value;
            }
        }
        public List<int> ListMenuID
        {
            get
            {
                if (Session["ListMenuID"] == null)
                {
                    Session["ListMenuID"] = new UserDAL().GetListFunctionCodeByUsername(CurrentUser.UserName);
                }
                return (List<int>)Session["ListMenuID"];
            }
            set
            {
                Session["ListMenuID"] = value;
            }
        }

        #region Render View|PartialView to String ()
        //Chú ý:
        //1. Các hàm này không thể truyền vào cùng lúc cả Model và ViewData do ViewData đã được sử dụng để chứa Model
        //2. Các hàm này không thể truyền vào ViewBag được

        private void InvalidateControllerContext()
        {
            if (ControllerContext == null)
            {
                ControllerContext context = new ControllerContext(System.Web.HttpContext.Current.Request.RequestContext, this);
                ControllerContext = context;
            }
        }

        //1
        public string RenderPartialToString(string partialViewName)
        {
            InvalidateControllerContext();
            IView view = ViewEngines.Engines.FindPartialView(ControllerContext, partialViewName).View;
            string result = RenderViewToString(view);
            return result;
        }

        //2
        public string RenderPartialToString(string partialViewName, object model)
        {
            InvalidateControllerContext();
            IView view = ViewEngines.Engines.FindPartialView(ControllerContext, partialViewName).View;
            string result = RenderViewToString(view, model);
            return result;
        }

        //3
        public string RenderViewToString(string viewName)
        {
            InvalidateControllerContext();
            IView view = ViewEngines.Engines.FindView(ControllerContext, viewName, null).View;
            string result = RenderViewToString(view);
            return result;
        }

        public string RenderViewToString(string viewName, object model)
        {
            InvalidateControllerContext();
            IView view = ViewEngines.Engines.FindView(ControllerContext, viewName, null).View;
            string result = RenderViewToString(view, model);
            return result;
        }

        public string RenderViewToString(IView view)
        {
            InvalidateControllerContext();
            string result = null;
            if (view != null)
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter writer = new StringWriter(sb))
                {
                    ViewContext viewContext = new ViewContext(ControllerContext, view, this.ViewData, this.TempData, writer);
                    view.Render(viewContext, writer);
                    writer.Flush();
                }
                result = sb.ToString();
            }
            return result;
        }

        public string RenderViewToString(IView view, object model)
        {
            InvalidateControllerContext();
            string result = null;
            if (view != null)
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter writer = new StringWriter(sb))
                {
                    ViewContext viewContext = new ViewContext(ControllerContext, view, new ViewDataDictionary(model), this.TempData, writer);
                    view.Render(viewContext, writer);
                    writer.Flush();
                }
                result = sb.ToString();
            }
            return result;
        }
        #endregion Render View|PartialView to String
    }
}