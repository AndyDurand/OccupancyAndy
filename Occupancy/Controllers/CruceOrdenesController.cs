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
    public class CruceOrdenesController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();
        [Authorize(Roles = "SuperAdmin")]
        // GET: CruceOrdenes
        public ActionResult Index()
        {
            return View(db.CruceOrden.ToList());
        }

        // GET: CruceOrdenes/Details/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CruceOrden cruceOrden = db.CruceOrden.Find(id);
            if (cruceOrden == null)
            {
                return HttpNotFound();
            }
            return View(cruceOrden);
        }

        // GET: CruceOrdenes/Create
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: CruceOrdenes/Create
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,IDEspacio,IDTipoMov,IDArbitrioMov")] CruceOrden cruceOrden)
        {
            if (ModelState.IsValid)
            {
                db.CruceOrden.Add(cruceOrden);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cruceOrden);
        }

        // GET: CruceOrdenes/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CruceOrden cruceOrden = db.CruceOrden.Find(id);
            if (cruceOrden == null)
            {
                return HttpNotFound();
            }
            return View(cruceOrden);
        }

        // POST: CruceOrdenes/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,IDEspacio,IDTipoMov,IDArbitrioMov")] CruceOrden cruceOrden)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cruceOrden).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cruceOrden);
        }

        // GET: CruceOrdenes/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CruceOrden cruceOrden = db.CruceOrden.Find(id);
            if (cruceOrden == null)
            {
                return HttpNotFound();
            }
            return View(cruceOrden);
        }

        // POST: CruceOrdenes/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CruceOrden cruceOrden = db.CruceOrden.Find(id);
            db.CruceOrden.Remove(cruceOrden);
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
