using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Alabama.Controllers
{
    public class Customer_InfoController : Controller
    {
        //
        // GET: /Customer_Info/

        public ActionResult Index()
        {
            return View(DB.Entities.Customer_Info.ToList());
        }

        //
        // GET: /Customer_Info/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Customer_Info/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Customer_Info/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        //
        // GET: /Customer_Info/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Customer_Info/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Customer_Info/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Customer_Info/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
