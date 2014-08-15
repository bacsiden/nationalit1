using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System.Web.Configuration;
using System.IO;
using NationalIT.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace NationalIT.Controllers
{
    public class BackupAndRestoreController : BaseController
    {
        //
        // GET: /BackupAndRestore/
        [Authorize]
        [ValidationFunction(ConditionValidate.OR, ActionName.BACKUPDATABASE, ActionName.RESTOREDATABASE)]
        public ActionResult Index()
        {
            string FilePathXML = HttpContext.Server.MapPath("~/App_Data/Data.xml");
            var doc = XDocument.Load(FilePathXML);
            bool isAdmin = CurrentUser.UserName.Equals(UserDAL.ADMIN);
            ViewBag.ShowBackupMenu = IsFunctionInRole(ActionName.BACKUPDATABASE.ToString()) || isAdmin;
            ViewBag.ShowDeleteMenu = ViewBag.ShowRestoreMenu = IsFunctionInRole(ActionName.RESTOREDATABASE.ToString()) || isAdmin;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_RestorePartial", doc.Descendants("Note").OrderByDescending(m => (int)m.Element("ID")).ToList());
            }
            return View(doc.Descendants("Note").OrderByDescending(m => (int)m.Element("ID")).ToList());
        }

        [Authorize]
        [ValidationFunction(ActionName.BACKUPDATABASE)]
        public ActionResult Backup()
        {
            DateTime now = DateTime.Now;
            string NameBak = "Backup_" + now.Year + now.Month + now.Day + "_" + now.Hour + "-" + now.Minute + "-" + now.Second + ".bak";
            try
            {
                string FilePathXML = HttpContext.Server.MapPath("~/App_Data/Data.xml");
                var db = XDocument.Load(FilePathXML).Descendants("Note").Where(m => m.Element("Name").Value == NameBak).FirstOrDefault();
                int i = 1;
                while (db != null)
                {
                    NameBak = NameBak + "_" + i++;
                    db = XDocument.Load(FilePathXML).Descendants("Note").Where(m => m.Element("Name").Value == NameBak).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
            }
            return PartialView(new BackupModel { Name = NameBak });
        }

        [HttpPost]
        [Authorize]
        [ValidationFunction(ActionName.BACKUPDATABASE)]
        public ActionResult Backup(BackupModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Name.Contains("\\"))
                    {
                        ModelState.AddModelError("Name", "File name must not contain one of the following characters /\\:*?\"<>|");
                        return PartialView(model);
                    }
                    string FilePathXML = HttpContext.Server.MapPath("~/App_Data/Data.xml");
                    var db = XDocument.Load(FilePathXML).Descendants("Note").Where(m => m.Element("Name").Value == model.Name).FirstOrDefault();
                    if (db != null)
                    {
                        ModelState.AddModelError("", "This file name is exists! Please choose a different file name.");
                        return PartialView(model);
                    }
                    // Đường dẫn tới file xml.
                    string path = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "/App_Data/config.xml";
                    // Tạo một đối tượng TextReader mới
                    var xtr = System.Xml.Linq.XDocument.Load(path);
                    string backupFolder = string.Format(xtr.Element("system.config").Element("path-backup").Value.Trim());   //Vuongj
                    string fileName = model.Name;
                    if (fileName.IndexOf(".bak") <= 0)
                    {
                        fileName += ".bak";
                    }

                    var filePath = backupFolder;
                    if (filePath.LastIndexOf("\\") == filePath.Length - 1)
                    {
                        filePath += fileName;
                    }
                    else
                    {
                        filePath += "\\" + fileName;
                    }

                    String conStr = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(conStr);
                    string databaseName = builder.InitialCatalog;

                    string query = string.Format(@"BACKUP DATABASE {0} TO DISK = '{1}' WITH INIT", databaseName, filePath);
                    int result = DB.Entities.ExecuteStoreCommand(query);
                    if (result != 0)
                    {
                        //add to Data.xml
                        var backupObj = new Models.BackupModel();
                        backupObj.Name = model.Name;
                        backupObj.FilePath = filePath;
                        backupObj.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        InsertBackupDatabase(backupObj);


                        ViewBag.Success = "1";
                        return JavaScript(@"$('#BackupBox').modal('hide');  $.sticky('Backup database successfully.', { autoclose: 5000, position: 'top-center', type: 'st-success' }); setTimeout(function() {location.reload(true);},1500);");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Cannot open backup device"))
                    {
                        // Đường dẫn tới file xml.
                        string path = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "/App_Data/config.xml";
                        // Tạo một đối tượng TextReader mới
                        var xtr = System.Xml.Linq.XDocument.Load(path);
                        string backupFolder = string.Format(xtr.Element("system.config").Element("path-backup").Value.Trim());
                        return Content("<div class='modal-header'><a class='close' data-dismiss='modal'>x</a><h3>Error message</h3></div><div class='modal-body'><p class='field-validation-error'><b>Backup database unsuccessfully.</b></p><p class='field-validation-error'>Folder backup \"" + backupFolder + "\" not found. <br />Please check again system config!</p></div>");
                    }
                    return Content("<div class='modal-header'><a class='close' data-dismiss='modal'>x</a><h3>Error message</h3></div><div class='modal-body'><p class='field-validation-error'><b>Backup database unsuccessfully.</b></p><p class='field-validation-error'>Please try again in a few minutes</p></div>");
                }
            }
            return PartialView(model);
        }

        [Authorize]
        [ValidationFunction(ActionName.RESTOREDATABASE)]
        public ActionResult Restore()
        {
            string FilePathXML = HttpContext.Server.MapPath("~/App_Data/Data.xml");
            var doc = XDocument.Load(FilePathXML);

            return View(doc.Descendants("Note").OrderByDescending(m => (int)m.Element("ID")).ToList());
        }

        [HttpGet]
        [Authorize]
        [ValidationFunction(ActionName.RESTOREDATABASE)]
        public ActionResult RestoreByID(string id)
        {
            string pathDataXML = HttpContext.Server.MapPath("~/App_Data/Data.xml");
            // Đường dẫn tới file xml.
            string pathConfigXML = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "/App_Data/config.xml";
            // Tạo một đối tượng TextReader mới
            var xtr = System.Xml.Linq.XDocument.Load(pathConfigXML);
            string backupFolder = string.Format(xtr.Element("system.config").Element("path-backup").Value.Trim());
            var db = XDocument.Load(pathDataXML).Descendants("Note").Where(m => m.Element("ID").Value == id).FirstOrDefault();
            var dbBackUp = DB.Entities;
            string pathFull = "";
            if ((backupFolder.LastIndexOf("\\") != backupFolder.Length - 1))
            {
                pathFull = backupFolder + "\\" + db.Element("Name").Value;
            }
            else
            {
                pathFull = backupFolder + db.Element("Name").Value;
            }
            try
            {
                string queryCheck = "DECLARE @result INT EXEC master.dbo.xp_fileexist '"+pathFull+"', @result OUTPUT  Select @result";
                int isExists = dbBackUp.ExecuteStoreQuery<int>(queryCheck,null).FirstOrDefault();
                if (isExists != 0)
                {
                    string queryStore = "use master ALTER DATABASE NationalIT SET SINGLE_USER With ROLLBACK IMMEDIATE;";
                    queryStore += "RESTORE DATABASE NationalIT FROM disk = '" + pathFull + "' WITH REPLACE;";
                    queryStore += "use master ALTER DATABASE NationalIT SET MULTI_USER;";
                    queryStore += "use master ALTER DATABASE NationalIT SET ONLINE";

                    int result = dbBackUp.ExecuteStoreCommand(queryStore);
                    if (Convert.ToInt32(result) != 0)
                    {
                        ViewBag.Success = "1";
                        return JavaScript(@"$.sticky('Restore database successfully.', { autoclose: 5000, position: 'top-center', type: 'st-success' });");
                    }
                }
            }
            catch (Exception ex)
            {

            }
            string message = "$('#ModalGeneral').empty().html(\"<div class='modal-header'><a class='close' data-dismiss='modal'>x</a><h3>Error message</h3></div><div class='modal-body'><p class='field-validation-error'><b>Restore database unsuccessfully!</b> <br /> - Please check folder restore in system config: &#34;" + pathFull.Replace("\\", "&#92;") + "&#34;.<br />- Or your user sql server have not permisson restore.</p></div>\").modal('show');";
            return JavaScript(message);
        }

        [Authorize]
        [ValidationFunction(ActionName.BACKUPDATABASE)]
        public void InsertBackupDatabase(Models.BackupModel note)
        {
            string FilePathXML = HttpContext.Server.MapPath("~/App_Data/Data.xml");
            var doc = XDocument.Load(FilePathXML);
            var getNote = doc.Descendants("Note");
            try
            {
                if (getNote.Where(c => c.Element("Name").Value == note.Name && c.Element("FilePath").Value == note.FilePath).FirstOrDefault().Elements().Count() != 0)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var lastId = (from c in doc.Descendants("Note")
                                  select (int)c.Element("ID")
                                ).OrderBy(x => x).Max();

                    note.ID = lastId + 1;

                }
                catch (Exception ex1)
                {
                    note.ID = 1;
                }

                var noteNode =
                          new XElement("Note",
                          new XElement("ID", note.ID),
                          new XElement("Name", note.Name),
                          new XElement("FilePath", note.FilePath),
                          new XElement("Date", note.Date)
                      );
                doc.Element("Notes").Element("BackupDatabase").Add(noteNode);
                doc.Save(FilePathXML);
            }


        }

        public bool UpdateBackupDatabase(Models.BackupModel note)
        {
            try
            {
                string FilePathXML = HttpContext.Server.MapPath("~/App_Data/Data.xml");
                var doc = XDocument.Load(FilePathXML);
                var noteNode = doc.Elements("Notes").Elements("Note").Where(x => x.Element("ID").Value == note.ID.ToString()).Take(1);
                noteNode.Elements("Name").SingleOrDefault().Value = note.Name;
                noteNode.Elements("FilePath").SingleOrDefault().Value = note.FilePath;
                doc.Save(FilePathXML);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [Authorize]
        [ValidationFunction(ActionName.RESTOREDATABASE)]
        public ActionResult DeleteBackupDatabaseByID(int id)
        {
            bool success = true;
            string message = "";
            try
            {
                string FilePathXML = HttpContext.Server.MapPath("~/App_Data/Data.xml");
                var doc = XDocument.Load(FilePathXML);
                var ObjectDelete = doc.Descendants("Note").Where(x => x.Element("ID").Value == id.ToString()).FirstOrDefault();
                //string ServerDBName = WebConfigurationManager.AppSettings["DBServer"].ToString();
                string path = ObjectDelete.Element("FilePath").Value;
                if (System.IO.File.Exists(path))
                {
                    // Use a try block to catch IOExceptions, to 
                    // handle the case of the file already being 
                    // opened by another process.
                    System.IO.File.Delete(path);
                }
                ObjectDelete.RemoveAll();
                ObjectDelete.Remove();
                doc.Save(FilePathXML);
                ViewBag.getLibrary = "1";
                success = true;
                message = "Delete successfully";
            }
            catch (System.IO.IOException e)
            {
                success = false;
                message = "Error system. File not found or file restore is failed. Please try again";
            }
            catch (Exception ex)
            {
                success = false;
                message = "Error system. Please try again a few minutes.";
            }
            return Json(new { IsSuccess = success, Message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}
