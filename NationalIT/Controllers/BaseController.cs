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
using System.Data.SqlClient;

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
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            ViewBag.IsLogin = CurrentUser != null;

            if (CurrentUser != null)
            {
                #region Quản trị hệ thống
                bool isAdmin = CurrentUser.UserName.Equals(UserDAL.ADMIN, StringComparison.OrdinalIgnoreCase);
                ViewBag.ShowButtonNewOrEdit = IsFunctionInRole(ActionName.NewOrEditItem.ToString()) ||
                    isAdmin;
                ViewBag.ShowButtonDelete = IsFunctionInRole(ActionName.DeleteItem.ToString()) ||
                    isAdmin;
                ViewBag.ShowButtonProcessPayroll = IsFunctionInRole(ActionName.ProcessPayroll.ToString()) ||
                    isAdmin;
                ViewBag.ShowButtonProcessRollBack = IsFunctionInRole(ActionName.ProcessRollBack.ToString()) ||
                    isAdmin;
                ViewBag.ShowBackupMenu = IsFunctionInRole(ActionName.BACKUPDATABASE.ToString()) ||
                                         IsFunctionInRole(ActionName.RESTOREDATABASE.ToString()) ||
                                         isAdmin;
                #endregion
            }
        }
        public enum ActionName
        {
            // Khai báo các quyền tương ứng với bảng Function            
            ViewListTrip,
            ViewListDriver,
            ViewListCutomer,
            ViewListFuelExpenses,
            ViewListSplitExpenses,
            ViewListOperatingExpenses,
            ViewListOwners,
            ViewListCompanyExpanses,
            ViewListIncome,
            ViewListEquipment,
            ViewListMaintenance,
            ViewListVolations,
            ViewAllReport,
            SystemAdmin,
            NewOrEditItem,
            DeleteItem,
            ProcessPayroll,
            ProcessRollBack,
            ViewListDispatchers,
            ViewListFixedCharges,
            BACKUPDATABASE,
            RESTOREDATABASE,
        }
        public enum ConditionValidate
        {
            AND = -1,
            OR = -2,
            NULL = -3,
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
            public List<string> AllowFunctionCodes { get; set; }

            public string UrlRedirect { get; set; }

            /// <summary>
            /// Điều kiện đối với danh sách các quyền truy cập
            /// </summary>
            public ConditionValidate Condition { get; set; }

            public ValidationFunctionAttribute(ActionName allowFunctionCode)
            {
                AllowFunctionCodes = new List<string>();
                AllowFunctionCodes.Add(allowFunctionCode.ToString());
            }


            public ValidationFunctionAttribute(string urlRedirect, ActionName allowFunctionCode)
            {
                UrlRedirect = urlRedirect;
                AllowFunctionCodes = new List<string>();
                AllowFunctionCodes.Add(allowFunctionCode.ToString());
            }

            public ValidationFunctionAttribute(ConditionValidate condition, params ActionName[] allowFunctionCodes)
            {
                Condition = condition;
                AllowFunctionCodes = new List<string>();
                foreach (var item in allowFunctionCodes)
                {
                    AllowFunctionCodes.Add(item.ToString());
                }
            }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                bool isValid = true;
                var user = new UserDAL().GetCurrentUser;
                if (user.Locked)
                {
                    new AccountController().LogOff();
                    filterContext.Result = new RedirectResult("/Account/AccessDenied");
                }
                else
                {
                    if (!user.UserName.Equals(UserDAL.ADMIN, StringComparison.OrdinalIgnoreCase) && AllowFunctionCodes.Count > 0)
                    {
                        if (Condition == ConditionValidate.NULL || Condition == ConditionValidate.OR)//Trường hợp yêu cầu user có một trong các chức năng đã khai báo
                        {
                            isValid = false;
                            foreach (var code in AllowFunctionCodes)
                            {
                                if (IsFunctionInRole(code))
                                {
                                    isValid = true;
                                    break;
                                }
                            }
                        }
                        else if (Condition == ConditionValidate.AND)//Trường hợp yêu cầu user có tất cả các chức năng đã khai báo
                        {
                            isValid = true;
                            foreach (var code in AllowFunctionCodes)
                            {
                                if (!IsFunctionInRole(code))
                                {
                                    isValid = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (!isValid)
                    {
                        if (String.IsNullOrEmpty(UrlRedirect))
                        {
                            filterContext.Result = new RedirectResult("/Account/AccessDenied");
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
        public void DeleteItem(object entity, object id, string idFieldName = "ID")
        {
            string s = entity.ToString();
            int k = s.LastIndexOf('.');
            string tableName = s.Substring(k + 1, s.Length - k - 2);
            s = "delete from " + tableName + " where " + idFieldName + "=" + id;
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand co = new SqlCommand(s, conn);
            co.ExecuteNonQuery();
            conn.Close();
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

        #region ListCode of User

        public bool IsFunctionInRole(string code)
        {
            if (ListFunctionCode != null)
            {
                if (ListFunctionCode.Contains(code))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsFunctionInRole(string[] codeArr)
        {
            if (ListFunctionCode != null)
            {
                foreach (var code in codeArr)
                {
                    if (ListFunctionCode.Contains(code))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsOneOfFunctionsInRole(string lstCode)
        {
            if (ListFunctionCode != null)
            {
                string[] lst = lstCode.Split(',');
                foreach (string item in lst)
                {
                    if (ListFunctionCode.Contains(item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion
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