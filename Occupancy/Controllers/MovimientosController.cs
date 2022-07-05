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
    public class MovimientosController : Controller
    {
        private readonly OccupancyEntities db = new OccupancyEntities();

        // GET: Movimientos
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            var movimientos = db.Movimientos.Include(m => m.Contratos).Include(m => m.Permisos).Include(m => m.TipoMovimiento).Include(m => m.Users);
            return View(movimientos.ToList());
        }

        // GET: Movimientos/Details/5
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movimientos movimientos = db.Movimientos.Find(id);
            if (movimientos == null)
            {
                return HttpNotFound();
            }
            return View(movimientos);
        }

        // GET: Movimientos/Create
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.IDContrato = new SelectList(db.Contratos, "IDContrato", "NombreComercial");
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones");
            ViewBag.IDTipoMovimiento = new SelectList(db.TipoMovimiento, "IDTipoMovimiento", "TipoMovimiento1");
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER");
            return View();
        }

        // POST: Movimientos/Create
        [Authorize(Roles = "SuperAdmin")]
        // 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDMovimiento,IDContrato,IDPermiso,IDTipoMovimiento,IDUser,ImporteTotal,FolioRecibo,FechaEmision,FechaVencimiento,Pagado,FechaPago,Corriente,Adicional,Recargos,Rezago,AdicionalRezago,RecargoRezago,Multa,Honorarios,Ejecucion,Redondeo,Observaciones")] Movimientos movimientos)
        {
            if (ModelState.IsValid)
            {
                db.Movimientos.Add(movimientos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IDContrato = new SelectList(db.Contratos, "IDContrato", "NombreComercial", movimientos.IDContrato);
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", movimientos.IDPermiso);
            ViewBag.IDTipoMovimiento = new SelectList(db.TipoMovimiento, "IDTipoMovimiento", "TipoMovimiento1", movimientos.IDTipoMovimiento);
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER", movimientos.IDUser);
            return View(movimientos);
        }

        // GET: Movimientos/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movimientos movimientos = db.Movimientos.Find(id);
            if (movimientos == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDContrato = new SelectList(db.Contratos, "IDContrato", "NombreComercial", movimientos.IDContrato);
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", movimientos.IDPermiso);
            ViewBag.IDTipoMovimiento = new SelectList(db.TipoMovimiento, "IDTipoMovimiento", "TipoMovimiento1", movimientos.IDTipoMovimiento);
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER", movimientos.IDUser);
            return View(movimientos);
        }

        // POST: Movimientos/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDMovimiento,IDContrato,IDPermiso,IDTipoMovimiento,IDUser,ImporteTotal,FolioRecibo,FechaEmision,FechaVencimiento,Pagado,FechaPago,Corriente,Adicional,Recargos,Rezago,AdicionalRezago,RecargoRezago,Multa,Honorarios,Ejecucion,Redondeo,Observaciones")] Movimientos movimientos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(movimientos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDContrato = new SelectList(db.Contratos, "IDContrato", "NombreComercial", movimientos.IDContrato);
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", movimientos.IDPermiso);
            ViewBag.IDTipoMovimiento = new SelectList(db.TipoMovimiento, "IDTipoMovimiento", "TipoMovimiento1", movimientos.IDTipoMovimiento);
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER", movimientos.IDUser);
            return View(movimientos);
        }

        // GET: Movimientos/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movimientos movimientos = db.Movimientos.Find(id);
            if (movimientos == null)
            {
                return HttpNotFound();
            }
            return View(movimientos);
        }

        // POST: Movimientos/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Movimientos movimientos = db.Movimientos.Find(id);
            db.Movimientos.Remove(movimientos);
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
