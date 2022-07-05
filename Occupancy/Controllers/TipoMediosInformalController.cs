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
    public class TipoMediosInformalController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: TipoMediosInformal
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            return View(db.TipoMedioInformal.ToList());
        }

        // GET: TipoMediosInformal/Details/5
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoMedioInformal tipoMedioInformal = db.TipoMedioInformal.Find(id);
            if (tipoMedioInformal == null)
            {
                return HttpNotFound();
            }
            return View(tipoMedioInformal);
        }

        // GET: TipoMediosInformal/Create
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: TipoMediosInformal/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Create([Bind(Include = "IDTipoMedioInformal,Medio,Placas")] TipoMedioInformal tipoMedioInformal)
        {
            if (ModelState.IsValid)
            {
                db.TipoMedioInformal.Add(tipoMedioInformal);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tipoMedioInformal);
        }

        // GET: TipoMediosInformal/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoMedioInformal tipoMedioInformal = db.TipoMedioInformal.Find(id);
            if (tipoMedioInformal == null)
            {
                return HttpNotFound();
            }
            return View(tipoMedioInformal);
        }

        // POST: TipoMediosInformal/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDTipoMedioInformal,Medio,Placas")] TipoMedioInformal tipoMedioInformal)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoMedioInformal).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoMedioInformal);
        }

        // GET: TipoMediosInformal/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoMedioInformal tipoMedioInformal = db.TipoMedioInformal.Find(id);
            if (tipoMedioInformal == null)
            {
                return HttpNotFound();
            }
            return View(tipoMedioInformal);
        }

        // POST: TipoMediosInformal/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TipoMedioInformal tipoMedioInformal = db.TipoMedioInformal.Find(id);
            db.TipoMedioInformal.Remove(tipoMedioInformal);
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
