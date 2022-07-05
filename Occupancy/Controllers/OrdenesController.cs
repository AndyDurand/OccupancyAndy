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
    public class OrdenesController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: Ordenes
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Index()
        {
            var ordenes = db.Ordenes.Include(o => o.ArbitriosMov).Include(o => o.Departamentos).Include(o => o.Movimientos).Include(o => o.Users);
                
            return View(ordenes.ToList());
        }

        // GET: Ordenes/Details/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ordenes ordenes = db.Ordenes.Find(id);
            if (ordenes == null)
            {
                return HttpNotFound();
            }
            return View(ordenes);
        }

        // GET: Ordenes/Create
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.IDArbitrioMov = new SelectList(db.ArbitriosMov, "IDArbitrioMov", "Arbitrio");
            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento");
            //ViewBag.IDMovimiento = new SelectList(db.Movimientos, "IDMovimiento", "FolioRecibo");
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER");
            ViewBag.IDMovimiento = new SelectList(db.Personas, "IDPersona", "Nombre");
            return View();
        }

        // POST: Ordenes/Create
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDOrden,IDUser,IDArbitrioMov,IDMovimiento,IDDepto,FolioRecibo,FechaEmision,ImporteTotal,Corriente,Adicional,Recargo,Rezago,AdicionalRezago,RecargoRezago,Multa,Honorarios,Ejecucion,Redondeo,Observaciones,Estatus")] Ordenes ordenes)
        {
            if (ModelState.IsValid)
            {
                db.Ordenes.Add(ordenes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IDArbitrioMov = new SelectList(db.ArbitriosMov, "IDArbitrioMov", "Arbitrio", ordenes.IDArbitrioMov);
            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento", ordenes.IDDepto);
            //ViewBag.IDMovimiento = new SelectList(db.Movimientos, "IDMovimiento", "FolioRecibo", ordenes.IDMovimiento);
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER", ordenes.IDUser);
            //ViewBag.IDMovimiento = new SelectList(db.Personas, "IDPersona", "Nombre", ordenes.IDMovimiento);
            return View(ordenes);
        }

        // GET: Ordenes/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ordenes ordenes = db.Ordenes.Find(id);
            if (ordenes == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDArbitrioMov = new SelectList(db.ArbitriosMov, "IDArbitrioMov", "Arbitrio", ordenes.IDArbitrioMov);
            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento", ordenes.IDDepto);
            //ViewBag.IDMovimiento = new SelectList(db.Movimientos, "IDMovimiento", "FolioRecibo", ordenes.IDMovimiento);
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER", ordenes.IDUser);
            //ViewBag.IDMovimiento = new SelectList(db.Personas, "IDPersona", "Nombre", ordenes.IDMovimiento);
            return View(ordenes);
        }

        // POST: Ordenes/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDOrden,IDUser,IDArbitrioMov,IDMovimiento,IDDepto,FolioRecibo,FechaEmision,ImporteTotal,Corriente,Adicional,Recargo,Rezago,AdicionalRezago,RecargoRezago,Multa,Honorarios,Ejecucion,Redondeo,Observaciones,Estatus")] Ordenes ordenes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ordenes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDArbitrioMov = new SelectList(db.ArbitriosMov, "IDArbitrioMov", "Arbitrio", ordenes.IDArbitrioMov);
            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento", ordenes.IDDepto);
            //ViewBag.IDMovimiento = new SelectList(db.Movimientos, "IDMovimiento", "FolioRecibo", ordenes.IDMovimiento);
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER", ordenes.IDUser);
            //ViewBag.IDMovimiento = new SelectList(db.Personas, "IDPersona", "Nombre", ordenes.IDMovimiento);
            return View(ordenes);
        }

        // GET: Ordenes/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ordenes ordenes = db.Ordenes.Find(id);
            if (ordenes == null)
            {
                return HttpNotFound();
            }
            return View(ordenes);
        }

        // POST: Ordenes/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ordenes ordenes = db.Ordenes.Find(id);
            db.Ordenes.Remove(ordenes);
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
