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
    public class ParametrosController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        [Authorize(Roles = "SuperAdmin")]
        // GET: Parametros
        public ActionResult Index()
        {
            return View(db.Parametros.ToList());
        }


        // GET: Parametros/Details/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Parametros parametros = db.Parametros.Find(id);
            if (parametros == null)
            {
                return HttpNotFound();
            }
            return View(parametros);
        }

        // GET: Parametros/Create
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Parametros/Create
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDParametro,Yar,UMAProrrateada,FechaLimiteProrrateada,UMANormal,FechaLimiteNormal")] Parametros parametros)
        {
            if (ModelState.IsValid)
            {
                db.Parametros.Add(parametros);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(parametros);
        }

        // GET: Parametros/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Parametros parametros = db.Parametros.Find(id);
            if (parametros == null)
            {
                return HttpNotFound();
            }
            return View(parametros);
        }

        // POST: Parametros/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDParametro,Yar,UMAProrrateada,FechaLimiteProrrateada,UMANormal,FechaLimiteNormal")] Parametros parametros)
        {
            if (ModelState.IsValid)
            {
                db.Entry(parametros).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(parametros);
        }

        // GET: Parametros/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Parametros parametros = db.Parametros.Find(id);
            if (parametros == null)
            {
                return HttpNotFound();
            }
            return View(parametros);
        }

        // POST: Parametros/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Parametros parametros = db.Parametros.Find(id);
            db.Parametros.Remove(parametros);
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
