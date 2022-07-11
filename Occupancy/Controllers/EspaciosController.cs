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
    public class EspaciosController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        //[Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        // GET: Espacios
        public ActionResult Index()
        {
            using (Repositorio<Users> obj = new Repositorio<Users>())
            {
                var u = User.Identity.GetUserId();
                var value = obj.Retrive(x => x.IDASPNETUSER == u).IDUser;
                var valueIdDepto = obj.Retrive(x => x.IDASPNETUSER == u).IDDepto;
                Session["ID_User"] = value;
                Session["ID_Area"] = valueIdDepto;

            }
            int idArea = (int)Session["ID_Area"];
            //
            if (User.IsInRole("SuperAdmin") || User.IsInRole("AdminAuditor"))
            {
                var espacios = db.Espacios.Include(e => e.Departamentos);
                ViewBag.nombreDepartamento = "Departamento: Todos";
                    
                return View(espacios.ToList());
            }
            else if (User.IsInRole("AdminArea") || User.IsInRole("FuncionarioA"))
            {
                var espacios = db.Espacios.Include(e => e.Departamentos)
                    .Where(e => e.IDDepto == idArea);
                Departamentos departamento = db.Departamentos.Find(idArea);
                if (departamento != null)
                {
                    ViewBag.nombreDepartamento = "Departamento: "+departamento.Departamento;
                }
                else
                {
                    ViewBag.nombreDepartamento = "Departamento: ";
                }
                return View(espacios.ToList());
            }
            else
            {
                return RedirectToAction("InvalidProfile", "Home");
            }
        }

        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        // GET: Espacios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Espacios espacios = db.Espacios.Find(id);
            if (espacios == null)
            {
                return HttpNotFound();
            }
            return View(espacios);
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: Espacios/Create
        public ActionResult Create()
        {
            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento");
            return View();
        }

        // POST: Espacios/Create 
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDEspacio,Espacio,IDDepto,Direccion,CP,NumLocales")] Espacios espacios)
        {
            if (ModelState.IsValid)
            {
                db.Espacios.Add(espacios);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento");
            return View(espacios);
        }

        // GET: Espacios/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Espacios espacios = db.Espacios.Find(id);
            if (espacios == null)
            {
                return HttpNotFound();
            }            
            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento", espacios.IDDepto);

            return View(espacios);
        }

        // POST: Espacios/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDEspacio,Espacio,IDDepto,Direccion,CP,NumLocales")] Espacios espacios)
        {
            if (ModelState.IsValid)
            {
                db.Entry(espacios).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDDepto = new SelectList(db.Departamentos, "IDDepto", "Departamento", espacios.IDDepto);
            return View(espacios);
        }

        // GET: Espacios/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Espacios espacios = db.Espacios.Find(id);
            if (espacios == null)
            {
                return HttpNotFound();
            }
            return View(espacios);
        }

        // POST: Espacios/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Espacios espacios = db.Espacios.Find(id);
            db.Espacios.Remove(espacios);
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
