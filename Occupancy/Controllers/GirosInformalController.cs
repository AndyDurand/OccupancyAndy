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
    public class GirosInformalController : Controller
    {
        private OccupancyEntities db = new OccupancyEntities();

        // GET: GirosInformal
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            return View(db.GiroInformal.ToList());
        }

        // GET: GirosInformal/Details/5
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiroInformal giroInformal = db.GiroInformal.Find(id);
            if (giroInformal == null)
            {
                return HttpNotFound();
            }
            return View(giroInformal);
        }

        // GET: GirosInformal/Create
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: GirosInformal/Create
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDGiroInformal,GiroInformal1")] GiroInformal giroInformal)
        {
            if (ModelState.IsValid)
            {
                db.GiroInformal.Add(giroInformal);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(giroInformal);
        }

        // GET: GirosInformal/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiroInformal giroInformal = db.GiroInformal.Find(id);
            if (giroInformal == null)
            {
                return HttpNotFound();
            }
            return View(giroInformal);
        }

        // POST: GirosInformal/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDGiroInformal,GiroInformal1")] GiroInformal giroInformal)
        {
            if (ModelState.IsValid)
            {
                db.Entry(giroInformal).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(giroInformal);
        }

        // GET: GirosInformal/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiroInformal giroInformal = db.GiroInformal.Find(id);
            if (giroInformal == null)
            {
                return HttpNotFound();
            }
            return View(giroInformal);
        }

        // POST: GirosInformal/Delete/5
        [Authorize(Roles = "SuperAdmin, AdminArea")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            GiroInformal giroInformal = db.GiroInformal.Find(id);
            db.GiroInformal.Remove(giroInformal);
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
