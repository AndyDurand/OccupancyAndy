using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Occupancy.Models;

namespace Occupancy.Controllers
{
    [Authorize]
    public class NavesController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: Naves
        [Authorize(Roles = "SuperAdmin,  AdminAuditor, AdminConsulta,  AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            var naves = db.Naves.Include(n => n.Espacios);
            return View(naves.ToList());
        }

        // GET: Naves; desde Index Espacios
        [Authorize(Roles = "SuperAdmin,  AdminAuditor, AdminConsulta,  AdminArea, FuncionarioA")]
        public ActionResult ViewNav(int? idEsp)
        {            
            // --Las Naves de ese Espacio
            var navesQuery = from n in db.Naves
                             orderby n.Nave
                             where n.IDEspacio == idEsp
                             select n;
            
            ViewBag.idEspacio = idEsp;
            if (navesQuery.ToList().Count() != 0  )
            {
                ViewBag.nombreEspacio = navesQuery.First().Espacios.Espacio;
                Session["ID_Espacio"] = idEsp;
                return View(navesQuery.ToList());
            }
            // no tiene Naves aún
            Espacios espacios = db.Espacios.Find(idEsp);
            if (espacios == null)
            {
                return HttpNotFound();
            }
            ViewBag.nombreEspacio = espacios.Espacio;
            Session["ID_Espacio"] = espacios.IDEspacio;
            return View();

        }

        // GET: Naves/CreateNav
        //-- Desde Vista NavIndex 
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult CreateNav(int? id)
        {
         
            //-- Si viene de ViewNav Index, es una Nave de ese Espacio
            Espacios e = db.Espacios.Find(id);
            if (e != null)
            {
                ViewBag.idEspacio = e.IDEspacio;
                ViewBag.nombreEspacio = e.Espacio;
            }                   
            return View();
        }

        // POST
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "IDNave,Nave,IDEspacio")] Naves naves)
        public ActionResult CreateNav(string nameNav)
        {
            int id = (int)Session["ID_Espacio"];
            if (ModelState.IsValid && (nameNav.Length > 0 && nameNav.Length <= 50))
            {
                ///
                Naves naves = new Naves();
                naves.Nave = nameNav;
                naves.IDEspacio = id;
                db.Naves.Add(naves);
                db.SaveChanges();               
            
            }
            return RedirectToAction("ViewNav", new { idEsp = id });  
        }

        // GET
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Naves naves = db.Naves.Find(id);
            if (naves == null)
            {
                return HttpNotFound();
            }
            return View(naves);
        }

        // POST
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDNave,Nave,IDEspacio")] Naves naves)
        {
            int id = (int)Session["ID_Espacio"];
            if (ModelState.IsValid)
            {
                db.Entry(naves).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ViewNav", new { idEsp = id });
            }            
            return RedirectToAction("ViewNav", new { idEsp = id });
        }

        // GET
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Naves naves = db.Naves.Find(id);
            if (naves == null)
            {
                return HttpNotFound();
            }
            return View(naves);
        }

        // POST
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Naves naves = db.Naves.Find(id);
            db.Naves.Remove(naves);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
