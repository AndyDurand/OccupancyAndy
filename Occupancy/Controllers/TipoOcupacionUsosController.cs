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
    public class TipoOcupacionUsosController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: TipoOcupacionUsos
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            return View(db.TipoOcupacionUso.ToList());
        }

        // GET: TipoOcupacionUsos/Details/5
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoOcupacionUso tipoOcupacionUso = db.TipoOcupacionUso.Find(id);
            if (tipoOcupacionUso == null)
            {
                return HttpNotFound();
            }
            return View(tipoOcupacionUso);
        }

        // GET: TipoOcupacionUsos/Create
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: TipoOcupacionUsos/Create
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDTipoOcupacionUso,OcupacionUso,GeneraContrato")] TipoOcupacionUso tipoOcupacionUso)
        {
            if (ModelState.IsValid)
            {
                db.TipoOcupacionUso.Add(tipoOcupacionUso);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tipoOcupacionUso);
        }

        // GET: TipoOcupacionUsos/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoOcupacionUso tipoOcupacionUso = db.TipoOcupacionUso.Find(id);
            if (tipoOcupacionUso == null)
            {
                return HttpNotFound();
            }
            return View(tipoOcupacionUso);
        }

        // POST: TipoOcupacionUsos/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDTipoOcupacionUso,OcupacionUso,GeneraContrato")] TipoOcupacionUso tipoOcupacionUso)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoOcupacionUso).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoOcupacionUso);
        }

        // GET: TipoOcupacionUsos/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoOcupacionUso tipoOcupacionUso = db.TipoOcupacionUso.Find(id);
            if (tipoOcupacionUso == null)
            {
                return HttpNotFound();
            }
            return View(tipoOcupacionUso);
        }

        // POST: TipoOcupacionUsos/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TipoOcupacionUso tipoOcupacionUso = db.TipoOcupacionUso.Find(id);
            db.TipoOcupacionUso.Remove(tipoOcupacionUso);
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
