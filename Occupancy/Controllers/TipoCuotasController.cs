using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Occupancy.Models;

namespace Occupancy.Controllers
{
    [Authorize]
    public class TipoCuotasController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: TipoCuotas
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            // 
            using (Repositorio<Users> obj = new Repositorio<Users>())
            {
                var u = User.Identity.GetUserId();
                var value = obj.Retrive(x => x.IDASPNETUSER == u).IDUser;
                var valueIdDepto = obj.Retrive(x => x.IDASPNETUSER == u).IDDepto;
                Session["ID_User"] = value;
                Session["ID_Depto"] = valueIdDepto;

            }
            int idDepto = (int)Session["ID_Depto"];
            // SuperAdmin y AdminAuditor pueden crear Cuotas para cualquier Depto
            if (User.IsInRole("SuperAdmin") || User.IsInRole("AdminAuditor"))
            {
                var tipoCuotas = db.TipoCuotas.Include(t => t.Espacios)
                .Include(t => t.Naves);
                return View(tipoCuotas.ToList());
            }
            else if (User.IsInRole("AdminArea") || User.IsInRole("FuncionarioA"))
            {
                var tipoCuotas = db.TipoCuotas.Include(t => t.Espacios)
                .Include(t => t.Naves)
                .Where(t => t.Espacios.IDDepto == idDepto);
                return View(tipoCuotas.ToList());
            }
            else return RedirectToAction("InvalidProfile", "Home");
        }

        // GET: TipoCuotas/Details/5
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoCuotas tipoCuotas = db.TipoCuotas.Find(id);
            if (tipoCuotas == null)
            {
                return HttpNotFound();
            }
            return View(tipoCuotas);
        }

        // Llenado de Select List Naves, desde vista Create, dependiendo de idEspacio -- ---
        public JsonResult GetNavesList(int espacioId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Naves> navesList = db.Naves
                                   .Where(x => x.IDEspacio == espacioId)
                                   .ToList();
            return Json(navesList, JsonRequestBehavior.AllowGet);
        }

        // Llenado de Select List Espacios, desde vista Create, dependiendo de idEspacio -- ---
        public JsonResult GetEspaciosList(int deptoId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Espacios> seccionesList = db.Espacios 
                                   .Where(x => x.IDDepto  == deptoId)
                                   .ToList();
            return Json(seccionesList, JsonRequestBehavior.AllowGet);
        }

        // GET: TipoCuotas/Create
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminArea, FuncionarioA")]
        public ActionResult Create()
        {
            // 
            int IDDeptoUser = (int)Session["ID_Depto"];
            if (User.IsInRole("SuperAdmin") || User.IsInRole("AdminAuditor"))
            {
                var espacioQry = from esp in db.Espacios
                                 orderby esp.Espacio                                
                                 select esp;
                ViewBag.IDEspacio = new SelectList(espacioQry.AsNoTracking(), "IDEspacio", "Espacio");
                // IDNAve List cambia en la vista por el Espacio seleccionado 
                var navesQry = from nav in db.Naves
                               orderby nav.Nave                               
                               select nav;
                ViewBag.IDNave = new SelectList(navesQry.AsNoTracking(), "IDNave", "Nave");
                return View();

            }
            else if (User.IsInRole("AdminArea") || User.IsInRole("FuncionarioA"))
            {
                var espacioQry = from esp in db.Espacios
                                 orderby esp.Espacio
                                 where esp.IDDepto == IDDeptoUser
                                 select esp;
                ViewBag.IDEspacio = new SelectList(espacioQry.AsNoTracking(), "IDEspacio", "Espacio");
                // IDNAve List cambia en la vista por el Espacio seleccionado 
                var navesQry = from nav in db.Naves
                               orderby nav.Nave
                               where nav.Espacios.IDDepto == IDDeptoUser
                               select nav;
                ViewBag.IDNave = new SelectList(navesQry.AsNoTracking(), "IDNave", "Nave");
                return View();
            }
            else return RedirectToAction("InvalidProfile", "Home");
        }


        // POST: TipoCuotas/Create
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "")] TipoCuotas tipoCuotas)
        {
            if (ModelState.IsValid)
            {
                tipoCuotas.Year = tipoCuotas.FechaAplicaInicial.Year;
                db.TipoCuotas.Add(tipoCuotas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", tipoCuotas.IDEspacio);
            ViewBag.IDNave = new SelectList(db.Naves, "IDNave", "Nave", tipoCuotas.IDNave);
            return View(tipoCuotas);
        }

        // GET: TipoCuotas/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoCuotas tipoCuotas = db.TipoCuotas.Find(id);
            if (tipoCuotas == null)
            {
                return HttpNotFound();
            }
            ViewBag.NombreCuota = tipoCuotas.TipoCuota;
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", tipoCuotas.IDEspacio);
            ViewBag.IDNave = new SelectList(db.Naves, "IDNave", "Nave", tipoCuotas.IDNave);

            return View(tipoCuotas);
        }

        // POST: TipoCuotas/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] TipoCuotas tipoCuotas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoCuotas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", tipoCuotas.IDEspacio);
            ViewBag.IDNave = new SelectList(db.Naves, "IDNave", "Nave", tipoCuotas.IDNave);
            return View(tipoCuotas);
        }

        // GET: TipoCuotas/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoCuotas tipoCuotas = db.TipoCuotas.Find(id);
            if (tipoCuotas == null)
            {
                return HttpNotFound();
            }
            return View(tipoCuotas);
        }

        // POST: TipoCuotas/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TipoCuotas tipoCuotas = db.TipoCuotas.Find(id);
            db.TipoCuotas.Remove(tipoCuotas);
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
