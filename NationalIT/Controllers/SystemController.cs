using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Web.Security;

namespace NationalIT.Controllers
{
    public class SystemController : BaseController
    {
        //
        // GET: /System/
        [Authorize]
        [ValidationFunction(ActionName.SystemAdmin)]
        [HttpGet]
        public ActionResult Index()
        {
            // Đường dẫn tới file xml.
            string fileName = HttpContext.Server.MapPath("~/App_Data/config.xml");
            if (!System.IO.File.Exists(fileName))
            {
                CrearDocumentoXML(fileName);
            }
            // Tạo một đối tượng TextReader mới
            var xtr = System.Xml.Linq.XDocument.Load(fileName);
            string path_backup = "";
            // string passwordsso = "";
            //bool useOtherMailServer = false;
            var config = xtr.Elements("system.config").FirstOrDefault();
            try
            {
                if (config.Descendants("path-backup").Any())
                    path_backup = config.Element("path-backup").Value.Trim();
            }
            catch (Exception ex)
            {
                return View();
            }
            var model = new Models.SystemModel()
            {
                PathBackupFolder = path_backup
            };
            return View(model);
        }


        [Authorize]
        [ValidationFunction(ActionName.SystemAdmin)]
        [HttpPost]
        public ActionResult Index(Models.SystemModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Đường dẫn tới file xml.
                    string fileName = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "/App_Data/config.xml";
                    if (!System.IO.File.Exists(fileName))
                    {
                        CrearDocumentoXML(fileName);
                    }

                    // Tạo một đối tượng TextReader mới
                    var xtr = System.Xml.Linq.XDocument.Load(fileName);
                    if (!xtr.Descendants("system.config").Any())
                    {
                        XElement EmailElement = new XElement("system.config");
                        xtr.Add(EmailElement);
                    }
                    var config = xtr.Elements("system.config").FirstOrDefault();

                    string path_backup = model.PathBackupFolder;

                    if (path_backup.LastIndexOf("\\") != path_backup.Length - 1)
                    {
                        path_backup += "\\";
                    }
                    if (!string.IsNullOrEmpty(path_backup))
                    {
                        if (!config.Descendants("path-backup").Any())
                        {
                            XElement passElement = new XElement("path-backup");
                            passElement.Value = path_backup;
                            config.Add(passElement);
                        }
                        else
                        {
                            config.SetElementValue("path-backup", path_backup);
                        }
                    }

                    xtr.Save(fileName);
                    ViewBag.saveSuccess = "1";
                    return JavaScript(@"alert('Save successfully.');$('#loadingBefore').html('');");
                }
                catch (Exception ex)
                {
                    return JavaScript(@"alert('Error system. Please try again a few minutes.');$('#loadingBefore').html('');");
                }
            }
            return PartialView(model);
        }
        protected void CrearDocumentoXML(string filePath)
        {

            XDocument miXML = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            XElement systemConfig = new XElement("system.config");

            // create note....path-backup,path_backup_image, ....
            XElement path_backup = new XElement("path-backup");

            systemConfig.Add(path_backup);

            miXML.Add(systemConfig);

            miXML.Save(filePath);
        }
        [Authorize]
        [ValidationFunction(ActionName.SystemAdmin)]
        [HttpGet]
        public ActionResult ResetToDefault()
        {
            var model = new Models.SystemModel()
            {
                PathBackupFolder = "",
            };
            return RedirectToAction("Index", "System");
        }
    }
}
