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
    public class SeccionesController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: Secciones        
        [Authorize(Roles = "SuperAdmin,  AdminAuditor, AdminConsulta,  AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            var secciones = db.Secciones.Include(s => s.Espacios);
            return View(secciones.ToList());
        }
        // GET 
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult ViewSec(int? idEsp)
        {
            // --Las Naves de ese Espacio
            var seccQuery = from s in db.Secciones 
                             orderby s.Seccion 
                             where s.IDEspacio == idEsp
                             select s;
             
            ViewBag.idEspacio = idEsp;
            if (seccQuery.ToList().Count() != 0)
            {
                ViewBag.nombreEspacio = seccQuery.First().Espacios.Espacio;
                Session["ID_Espacio"] = idEsp;
                return View(seccQuery.ToList());
            }
            // no tiene Secciones aún
            Espacios espacios = db.Espacios.Find(idEsp);
            if (espacios == null)
            {
                return HttpNotFound();
            }
            ViewBag.nombreEspacio = espacios.Espacio;
            Session["ID_Espacio"] = espacios.IDEspacio;
            return View();

        }
        // GET
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult CreateSec(int? id)
        {
            //-- Si viene de ViewSec Index es una Sección de ese Espacio
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
        public ActionResult CreateSec(string nameSec)
        {
            int id = (int)Session["ID_Espacio"];
            if (ModelState.IsValid && (nameSec.Length > 0 && nameSec.Length <= 50))
            {
                ///
                Secciones seccion = new Secciones();
                seccion.Seccion = nameSec;
                seccion.IDEspacio = id;
                db.Secciones.Add(seccion);
                db.SaveChanges();

            }
            return RedirectToAction("ViewSec", new { idEsp = id });
        }

        // GET
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Secciones secciones = db.Secciones.Find(id);
            if (secciones == null)
            {
                return HttpNotFound();
            }            
            return View(secciones);
        }

        // POST: Secciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Edit([Bind(Include = "IDSeccion,Seccion,IDEspacio")] Secciones secciones)
        {
            int id = (int)Session["ID_Espacio"];
            if (ModelState.IsValid)
            {
                db.Entry(secciones).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ViewSec", new { idEsp = id });
            }            
            return RedirectToAction("ViewSec", new { idEsp = id });
        }

        // GET: Secciones/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Secciones secciones = db.Secciones.Find(id);
            if (secciones == null)
            {
                return HttpNotFound();
            }
            return View(secciones);
        }

        // POST: Secciones/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Secciones secciones = db.Secciones.Find(id);
            db.Secciones.Remove(secciones);
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
