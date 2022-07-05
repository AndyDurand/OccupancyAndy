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
    public class VendedoresController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: Vendedores
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            var vendedores = db.Vendedores.Include(v => v.Permisos);
            return View(vendedores.ToList());
        }

        // GET: Vendedores/Details/5
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendedores vendedores = db.Vendedores.Find(id);
            if (vendedores == null)
            {
                return HttpNotFound();
            }
            return View(vendedores);
        }

        // GET: Vendedores/Create
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Create()
        {
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones");
            return View();
        }

        // POST: Vendedores/Create
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDVendedor,IDPermiso,Nombre")] Vendedores vendedores)
        {
            if (ModelState.IsValid)
            {
                db.Vendedores.Add(vendedores);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", vendedores.IDPermiso);
            return View(vendedores);
        }

        // GET: Vendedores/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendedores vendedores = db.Vendedores.Find(id);
            if (vendedores == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", vendedores.IDPermiso);
            return View(vendedores);
        }

        // POST: Vendedores/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDVendedor,IDPermiso,Nombre")] Vendedores vendedores)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vendedores).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", vendedores.IDPermiso);
            return View(vendedores);
        }

        // GET: Vendedores/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendedores vendedores = db.Vendedores.Find(id);
            if (vendedores == null)
            {
                return HttpNotFound();
            }
            return View(vendedores);
        }

        // POST: Vendedores/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vendedores vendedores = db.Vendedores.Find(id);
            db.Vendedores.Remove(vendedores);
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
