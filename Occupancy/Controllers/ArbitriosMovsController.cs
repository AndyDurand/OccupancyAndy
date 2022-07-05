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
    public class ArbitriosMovsController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: ArbitriosMovs
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea")]
        public ActionResult Index()
        {
            var arbitriosMov = db.ArbitriosMov.Include(a => a.Departamentos).Include(a => a.Espacios);
            return View(arbitriosMov.ToList());
        }

        // GET: ArbitriosMovs/Details/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArbitriosMov arbitriosMov = db.ArbitriosMov.Find(id);
            if (arbitriosMov == null)
            {
                return HttpNotFound();
            }
            return View(arbitriosMov);
        }

        // GET: ArbitriosMovs/Create
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento");
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio");
            return View();
        }

        // POST: ArbitriosMovs/Create
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDArbitrioMov,IDEspacio,IDDepto,Arbitrio,NaturalezaArbitrio,Nombre,NombreCorto,EsCorriente,TieneAdcional,TieneRecargos,EsRezago,TieneAdicionalRezago,TieneRecargosRezago,TieneMulta,TieneHonorarios,TieneEjecucion")] ArbitriosMov arbitriosMov)
        {
            if (ModelState.IsValid)
            {
                db.ArbitriosMov.Add(arbitriosMov);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento", arbitriosMov.IDDepto);
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", arbitriosMov.IDEspacio);
            return View(arbitriosMov);
        }

        // GET: ArbitriosMovs/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArbitriosMov arbitriosMov = db.ArbitriosMov.Find(id);
            if (arbitriosMov == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento", arbitriosMov.IDDepto);
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", arbitriosMov.IDEspacio);
            return View(arbitriosMov);
        }

        // POST: ArbitriosMovs/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDArbitrioMov,IDEspacio,IDDepto,Arbitrio,NaturalezaArbitrio,Nombre,NombreCorto,EsCorriente,TieneAdcional,TieneRecargos,EsRezago,TieneAdicionalRezago,TieneRecargosRezago,TieneMulta,TieneHonorarios,TieneEjecucion")] ArbitriosMov arbitriosMov)
        {
            if (ModelState.IsValid)
            {
                db.Entry(arbitriosMov).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento", arbitriosMov.IDDepto);
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", arbitriosMov.IDEspacio);
            return View(arbitriosMov);
        }

        // GET: ArbitriosMovs/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArbitriosMov arbitriosMov = db.ArbitriosMov.Find(id);
            if (arbitriosMov == null)
            {
                return HttpNotFound();
            }
            return View(arbitriosMov);
        }

        // POST: ArbitriosMovs/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ArbitriosMov arbitriosMov = db.ArbitriosMov.Find(id);
            db.ArbitriosMov.Remove(arbitriosMov);
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
