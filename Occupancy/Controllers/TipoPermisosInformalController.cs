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
    public class TipoPermisosInformalController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: TipoPermisosInformal
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            return View(db.TipoPermisoInformal.ToList());
        }

        // GET: TipoPermisosInformal/Details/5
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoPermisoInformal tipoPermisoInformal = db.TipoPermisoInformal.Find(id);
            if (tipoPermisoInformal == null)
            {
                return HttpNotFound();
            }
            return View(tipoPermisoInformal);
        }

        // GET: TipoPermisosInformal/Create
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: TipoPermisosInformal/Create
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDTipoPermisoInformal,PermisoInformal")] TipoPermisoInformal tipoPermisoInformal)
        {
            if (ModelState.IsValid)
            {
                db.TipoPermisoInformal.Add(tipoPermisoInformal);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tipoPermisoInformal);
        }

        // GET: TipoPermisosInformal/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoPermisoInformal tipoPermisoInformal = db.TipoPermisoInformal.Find(id);
            if (tipoPermisoInformal == null)
            {
                return HttpNotFound();
            }
            return View(tipoPermisoInformal);
        }

        // POST: TipoPermisosInformal/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDTipoPermisoInformal,PermisoInformal")] TipoPermisoInformal tipoPermisoInformal)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoPermisoInformal).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoPermisoInformal);
        }

        // GET: TipoPermisosInformal/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoPermisoInformal tipoPermisoInformal = db.TipoPermisoInformal.Find(id);
            if (tipoPermisoInformal == null)
            {
                return HttpNotFound();
            }
            return View(tipoPermisoInformal);
        }

        // POST: TipoPermisosInformal/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TipoPermisoInformal tipoPermisoInformal = db.TipoPermisoInformal.Find(id);
            db.TipoPermisoInformal.Remove(tipoPermisoInformal);
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
