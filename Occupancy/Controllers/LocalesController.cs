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
    public class LocalesController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: Locales
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
            //
            if (User.IsInRole("SuperAdmin") || User.IsInRole("AdminAuditor") || User.IsInRole("AdminConsulta"))
            {                 
                var locales = db.Locales
                    .Include(l => l.Espacios)
                    .Include(l => l.Naves)
                    .Include(l => l.Secciones)
                    .Include(l => l.TipoCuotas);
                ViewBag.nombreDepto = "";

                return View(locales.ToList());
            }
            else if (User.IsInRole("AdminArea") || User.IsInRole("FuncionarioA"))
            {
                // los locales de los Espacios que corresponden al idDepto
                var locales = db.Locales
                    .Include(l => l.Espacios)
                    .Include(l => l.Naves)
                    .Include(l => l.Secciones)
                    .Include(l => l.TipoCuotas)
                    .Where(l => l.Espacios.IDDepto == idDepto);
                if (locales.ToList().Count() > 1)
                {
                    // el nombre del Depto, en lugar del nombre del Espacio
                    // ViewBag.nombreEspacio = locales.ToList().First().Espacios.Espacio;                    
                }                
                Departamentos departamento = db.Departamentos.Find(idDepto);
                ViewBag.nombreDepto = departamento.Departamento;

                return View(locales.ToList());
            }
            else
            {
                return RedirectToAction("InvalidProfile", "Home");
            }             
        }

        // GET: Locales/Details/5
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Locales locales = db.Locales.Find(id);
            if (locales == null)
            {
                return HttpNotFound();
            }
            return View(locales);
        }
        // Llenado de Select List Tipo de Cuota, desde vista Create, dependiendo de idnave -- ---
        public JsonResult GetTipoCuotaList(int naveId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<TipoCuotas> tipoCuotasList = db.TipoCuotas
                                                .Where(x => x.IDNave == naveId)
                                                .ToList();
            return Json(tipoCuotasList, JsonRequestBehavior.AllowGet);
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

        // Llenado de Select List Secciones, desde vista Create, dependiendo de idEspacio -- ---
        public JsonResult GetSeccionesList(int espacioId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Secciones> seccionesList = db.Secciones 
                                   .Where(x => x.IDEspacio == espacioId)
                                   .ToList();
            return Json(seccionesList, JsonRequestBehavior.AllowGet);
        }

        // GET: Locales/Create -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Create()
        {
            int IDDeptoUser = (int)Session["ID_Depto"];
            // Naves del Espacio seleccionado, GetNavesList, desde Vista Create con el Json result
            // Secciones del idEspacio, desde vista Create con el Json result
            // TipoCuota por Nave. GetTipoCuotaList, desde vista Create con el Json result
            if (User.IsInRole("SuperAdmin") || User.IsInRole("AdminAuditor"))
            {
                //iniciales, después actualizados con el Json result    
                var espacioQry = from esp in db.Espacios
                                 orderby esp.Espacio                                 
                                 select esp;
                ViewBag.IDEspacio = new SelectList(espacioQry.AsNoTracking(), "IDEspacio", "Espacio");
                var navesQry = from nav in db.Naves
                               orderby nav.Nave
                               select nav;
                ViewBag.IDNave = new SelectList(navesQry.AsNoTracking(), "IDNave", "Nave");
                var seccionesQry = from sec in db.Secciones
                                   orderby sec.Seccion
                                   select sec;
                ViewBag.IDSeccion = new SelectList(seccionesQry.AsNoTracking(), "IDSeccion", "Seccion");                

                var tipocQry = from tc in db.TipoCuotas
                               orderby tc.TipoCuota
                               select tc;
                ViewBag.IDTipoCuota = new SelectList(tipocQry.AsNoTracking(), "IDTipoCuota", "TipoCuota");
                return View();

            }
            else if (User.IsInRole("AdminArea") || User.IsInRole("FuncionarioA"))
            {
                var espacioQry = from esp in db.Espacios
                                 orderby esp.Espacio
                                 where esp.IDDepto == IDDeptoUser
                                 select esp;
                ViewBag.IDEspacio = new SelectList(espacioQry.AsNoTracking(), "IDEspacio", "Espacio");
                var navesQry = from nav in db.Naves
                               orderby nav.Nave
                               where nav.Espacios.IDDepto == IDDeptoUser
                               select nav;
                ViewBag.IDNave = new SelectList(navesQry.AsNoTracking(), "IDNave", "Nave");
                var seccionesQry = from sec in db.Secciones
                                   orderby sec.Seccion
                                   where sec.Espacios.IDDepto == IDDeptoUser
                                   select sec;
                ViewBag.IDSeccion = new SelectList(seccionesQry.AsNoTracking(), "IDSeccion", "Seccion");
                //ViewBag.IDSeccion = GetSeccionesList();


                //TipoCuota por espacio y nave, por IDNave, para llenado inicial las del depto
                var tipocQry = from tc in db.TipoCuotas
                               orderby tc.TipoCuota
                               where tc.Espacios.IDDepto == IDDeptoUser
                               select tc;
                ViewBag.IDTipoCuota = new SelectList(tipocQry.AsNoTracking(), "IDTipoCuota", "TipoCuota");
                return View();
            }
            else return RedirectToAction("InvalidProfile", "Home");
            
        }

        // POST: Locales/Create -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "")] Locales locales)
        {
            if (ModelState.IsValid)
            {
                db.Locales.Add(locales);
                // -- El numero de locales para cobro, se debería establecer el criterio de m2 para establecerlo
                if (locales.MFrente != 0 && locales.MFondo != 0)
                {
                    locales.MCuadTotales = locales.MFrente * locales.MFondo;
                }
                else locales.MCuadTotales = 0;
                locales.Ocupado = false;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            // Llenado general de los List, cambian dinámicamente con el script de la vista y el Json result
            ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", locales.IDEspacio);
            ViewBag.IDNave = new SelectList(db.Naves, "IDNave", "Nave", locales.IDNave);
            ViewBag.IDSeccion = new SelectList(db.Secciones, "IDSeccion", "Seccion", locales.IDSeccion);
            ViewBag.IDTipoCuota = new SelectList(db.TipoCuotas, "IDTipoCuota", "TipoCuota", locales.IDTipoCuota);
            return View(locales);
        }

        // GET: Locales/Edit/5   -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Locales locales = db.Locales.Find(id);
            if (locales == null)
            {
                return HttpNotFound();
            }
            ViewBag.ubicacionEspacio = locales.Espacios.Espacio;
            if (locales.Ocupado == true)
                ViewBag.estado = "Ocupado";
            else
                ViewBag.estado = "Desocupado";
            //
            int IDDeptoUser = (int)Session["ID_Depto"];
            // Llenado general de los List, cambian dinámicamente con el script de la vista y el Json result
            //ViewBag.IDEspacio = new SelectList(db.Espacios, "IDEspacio", "Espacio", locales.IDEspacio);
            var navesQry = from nav in db.Naves 
                           orderby nav.Nave 
                           where nav.IDEspacio == locales.IDEspacio  //las naves ddel idespacio
                           select nav;
            ViewBag.IDNave = new SelectList(navesQry.AsNoTracking(), "IDNave", "Nave", locales.IDNave);
            
            var seccionesQry = from sec in db.Secciones
                           orderby sec.Seccion 
                           where sec.IDEspacio == locales.IDEspacio 
                               select sec;
            ViewBag.IDSeccion = new SelectList(seccionesQry.AsNoTracking(), "IDSeccion", "Seccion", locales.IDSeccion);

            // TipoCuota por espacio y nave  
            var tipocQry = from tc in db.TipoCuotas
                           orderby tc.TipoCuota
                           where tc.IDEspacio == locales.IDEspacio && tc.IDNave == locales.IDNave
                           select tc;
            ViewBag.IDTipoCuota = new SelectList(tipocQry.AsNoTracking(), "IDTipoCuota", "TipoCuota", locales.IDTipoCuota);
            return View(locales);
        }

        // POST: Locales/Edit/5        
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDLocal,IDEspacio,IDNave,IDSeccion,IDTipoCuota,Ocupado,Local,MFrente,MFondo,MCuadTotales,NumLocParaCobro,ImporteRenta")] Locales locales)
        {
            if (ModelState.IsValid)
            {
                db.Entry(locales).State = EntityState.Modified;
                // -- El numero de locales para cobro, se debería establecer el criterio de m2 para establecerlo
                if (locales.MFrente != 0 && locales.MFondo != 0)
                {
                    locales.MCuadTotales = locales.MFrente * locales.MFondo;
                }
                else locales.MCuadTotales = 0;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //
            int IDDeptoUser = (int)Session["ID_Depto"];
            // Llenado general de los List, cambian dinámicamente con el script de la vista y el Json result
            var navesQry = from nav in db.Naves
                           orderby nav.Nave
                           where nav.IDEspacio == IDDeptoUser
                           select nav;
            ViewBag.IDNave = new SelectList(navesQry.AsNoTracking(), "IDNave", "Nave", locales.IDNave);
            
            var seccionesQry = from sec in db.Secciones
                               orderby sec.Seccion
                               where sec.IDEspacio == IDDeptoUser
                               select sec;
            ViewBag.IDSeccion = new SelectList(seccionesQry.AsNoTracking(), "IDSeccion", "Seccion", locales.IDSeccion);
            
            // TipoCuota por espacio y nave
            var tipocQry = from tc in db.TipoCuotas
                           orderby tc.TipoCuota
                           where tc.IDEspacio == locales.IDEspacio && tc.IDNave == locales.IDNave
                           select tc;
            ViewBag.IDTipoCuota = new SelectList(tipocQry.AsNoTracking(), "IDTipoCuota", "TipoCuota", locales.IDTipoCuota);
            return View(locales);
        }

        // GET: Locales/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Locales locales = db.Locales.Find(id);
            if (locales == null)
            {
                return HttpNotFound();
            }
            return View(locales);
        }

        // POST: Locales/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Locales locales = db.Locales.Find(id);
            db.Locales.Remove(locales);
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
