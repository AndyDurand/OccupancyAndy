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
    public class PermisosController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: Permisos
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            var permisos = db.Permisos.Include(p => p.Espacios).Include(p => p.GiroInformal).Include(p => p.Permisos1).Include(p => p.Permisos2).Include(p => p.Personas).Include(p => p.TipoMedioInformal).Include(p => p.TipoOcupacionUso).Include(p => p.TipoPermisoInformal).Include(p => p.Users);
            return View(permisos.ToList());
        }

        // GET: Permisos/Details/5
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Permisos permisos = db.Permisos.Find(id);
            if (permisos == null)
            {
                return HttpNotFound();
            }
            return View(permisos);
        }

        // GET: Permisos/Create
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Create()
        {
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio");
            ViewBag.IDGiroInformal = new SelectList(db.GiroInformal, "IDGiroInformal", "GiroInformal1");
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones");
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones");
            ViewBag.IDPersona = new SelectList(db.Personas, "IDPersona", "Nombre");
            ViewBag.IDTipoMedioInformal = new SelectList(db.TipoMedioInformal, "IDTipoMedioInformal", "Medio");
            ViewBag.IDTipoOcupacionUso = new SelectList(db.TipoOcupacionUso, "IDTipoOcupacionUso", "OcupacionUso");
            ViewBag.IDTipoPermisoInformal = new SelectList(db.TipoPermisoInformal, "IDTipoPermisoInformal", "PermisoInformal");
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER");
            return View();
        }

        // POST: Permisos/Create

        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDPermiso,IDPersona,IDEspacio,IDTipoOcupacionUso,IDGiroInformal,IDTipoMedioInformal,IDTipoPermisoInformal,NombreComercial,Activo,DiaFijoPago,FechaPermiso,FechaHrInicio,FechaHrFin,IDUser,Observaciones")] Permisos permisos)
        {
            if (ModelState.IsValid)
            {
                db.Permisos.Add(permisos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", permisos.IDEspacio);
            ViewBag.IDGiroInformal = new SelectList(db.GiroInformal, "IDGiroInformal", "GiroInformal1", permisos.IDGiroInformal);
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", permisos.IDPermiso);
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", permisos.IDPermiso);
            ViewBag.IDPersona = new SelectList(db.Personas, "IDPersona", "Nombre", permisos.IDPersona);
            ViewBag.IDTipoMedioInformal = new SelectList(db.TipoMedioInformal, "IDTipoMedioInformal", "Medio", permisos.IDTipoMedioInformal);
            ViewBag.IDTipoOcupacionUso = new SelectList(db.TipoOcupacionUso, "IDTipoOcupacionUso", "OcupacionUso", permisos.IDTipoOcupacionUso);
            ViewBag.IDTipoPermisoInformal = new SelectList(db.TipoPermisoInformal, "IDTipoPermisoInformal", "PermisoInformal", permisos.IDTipoPermisoInformal);
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER", permisos.IDUser);
            return View(permisos);
        }

        // GET: Permisos/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Permisos permisos = db.Permisos.Find(id);
            if (permisos == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", permisos.IDEspacio);
            ViewBag.IDGiroInformal = new SelectList(db.GiroInformal, "IDGiroInformal", "GiroInformal1", permisos.IDGiroInformal);
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", permisos.IDPermiso);
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", permisos.IDPermiso);
            ViewBag.IDPersona = new SelectList(db.Personas, "IDPersona", "Nombre", permisos.IDPersona);
            ViewBag.IDTipoMedioInformal = new SelectList(db.TipoMedioInformal, "IDTipoMedioInformal", "Medio", permisos.IDTipoMedioInformal);
            ViewBag.IDTipoOcupacionUso = new SelectList(db.TipoOcupacionUso, "IDTipoOcupacionUso", "OcupacionUso", permisos.IDTipoOcupacionUso);
            ViewBag.IDTipoPermisoInformal = new SelectList(db.TipoPermisoInformal, "IDTipoPermisoInformal", "PermisoInformal", permisos.IDTipoPermisoInformal);
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER", permisos.IDUser);
            return View(permisos);
        }

        // POST: Permisos/Edit/5

        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDPermiso,IDPersona,IDEspacio,IDTipoOcupacionUso,IDGiroInformal,IDTipoMedioInformal,IDTipoPermisoInformal,NombreComercial,Activo,DiaFijoPago,FechaPermiso,FechaHrInicio,FechaHrFin,IDUser,Observaciones")] Permisos permisos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(permisos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", permisos.IDEspacio);
            ViewBag.IDGiroInformal = new SelectList(db.GiroInformal, "IDGiroInformal", "GiroInformal1", permisos.IDGiroInformal);
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", permisos.IDPermiso);
            ViewBag.IDPermiso = new SelectList(db.Permisos, "IDPermiso", "Observaciones", permisos.IDPermiso);
            ViewBag.IDPersona = new SelectList(db.Personas, "IDPersona", "Nombre", permisos.IDPersona);
            ViewBag.IDTipoMedioInformal = new SelectList(db.TipoMedioInformal, "IDTipoMedioInformal", "Medio", permisos.IDTipoMedioInformal);
            ViewBag.IDTipoOcupacionUso = new SelectList(db.TipoOcupacionUso, "IDTipoOcupacionUso", "OcupacionUso", permisos.IDTipoOcupacionUso);
            ViewBag.IDTipoPermisoInformal = new SelectList(db.TipoPermisoInformal, "IDTipoPermisoInformal", "PermisoInformal", permisos.IDTipoPermisoInformal);
            ViewBag.IDUser = new SelectList(db.Users, "IDUser", "IDASPNETUSER", permisos.IDUser);
            return View(permisos);
        }

        // GET: Permisos/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Permisos permisos = db.Permisos.Find(id);
            if (permisos == null)
            {
                return HttpNotFound();
            }
            return View(permisos);
        }

        // POST: Permisos/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Permisos permisos = db.Permisos.Find(id);
            db.Permisos.Remove(permisos);
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
