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
    public class PersonasController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        //[Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        // GET: Personas
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
            int idDepto = (int)Session["ID_Area"];
            //
            if (User.IsInRole("SuperAdmin") || User.IsInRole("AdminAuditor"))
            {
                var padron = db.Personas.Include(p => p.Departamentos);
                ViewBag.nombreDepartamento = "Departamento: Todos";                

                return View(padron.ToList());
            }
            else if (User.IsInRole("AdminArea") || User.IsInRole("FuncionarioA"))
            {
                var padron = db.Personas.Include(p => p.Departamentos)                
                    .Where(p => p.IDDepto == idDepto);
                Departamentos departamento = db.Departamentos.Find(idDepto);
                if (departamento != null)
                {
                    ViewBag.nombreDepartamento = "Departamento: " + departamento.Departamento;
                }
                else
                {
                    ViewBag.nombreDepartamento = "Departamento: ";
                }
                return View(padron.ToList());
            }
            else
            {
                return RedirectToAction("InvalidProfile", "Home");
            }

        }

        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        // GET: Personas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Personas personas = db.Personas.Find(id);
            if (personas == null)
            {
                return HttpNotFound();
            }
            return View(personas);
        }

        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        // GET: Personas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Personas/Create
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "")] Personas personas)
        {
            if (ModelState.IsValid)
            {

                int idDepto = (int)Session["ID_Area"];
                personas.IDDepto = idDepto;

                db.Personas.Add(personas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(personas);
        }

        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        // GET: Personas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Personas personas = db.Personas.Find(id);
            if (personas == null)
            {
                return HttpNotFound();
            }
            ViewBag.nombrePersona = personas.Nombre + " " + personas.APaterno + " " + personas.AMaterno;
            return View(personas);
        }

        // POST: Personas/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] Personas personas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(personas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(personas);
        }

        [Authorize(Roles = "SuperAdmin, AdminArea")]
        // GET: Personas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Personas personas = db.Personas.Find(id);
            if (personas == null)
            {
                return HttpNotFound();
            }
            return View(personas);
        }

        [Authorize(Roles = "SuperAdmin, AdminArea")]
        // POST: Personas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Personas personas = db.Personas.Find(id);
            db.Personas.Remove(personas);
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
