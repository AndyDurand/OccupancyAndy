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
    public class GirosController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: Giros
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            return View(db.Giros.ToList());
        }

        // GET: Giros/Details/5
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Giros giros = db.Giros.Find(id);
            if (giros == null)
            {
                return HttpNotFound();
            }
            return View(giros);
        }

        // GET: Giros/Create
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult CreateGiro()
        {
            // defino los Giros generales no por Espacio
            return View();
        }

        // POST: Giros/Create
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateGiro(string nameGiro)
        {
            if (ModelState.IsValid && (nameGiro.Length > 0 && nameGiro.Length <= 50))
            {
                //return RedirectToAction("Index");
                ///
                Giros giro = new Giros();
                giro.Giro = nameGiro;
                db.Giros.Add(giro);
                db.SaveChanges();
            }

            return View();
            //return View(giros);
        }

        // GET: Giros/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Giros giros = db.Giros.Find(id);
            if (giros == null)
            {
                return HttpNotFound();
            }
            return View(giros);
        }

        // POST: Giros/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDGiro,Giro")] Giros giros)
        {
            if (ModelState.IsValid)
            {
                db.Entry(giros).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(giros);
        }

        // GET: Giros/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Giros giros = db.Giros.Find(id);
            if (giros == null)
            {
                return HttpNotFound();
            }
            return View(giros);
        }

        // POST: Giros/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Giros giros = db.Giros.Find(id);
            db.Giros.Remove(giros);
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
