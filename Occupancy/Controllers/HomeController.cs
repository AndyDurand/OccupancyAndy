using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Occupancy.Models;
using Microsoft.AspNet.Identity;

namespace Occupancy.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {
            return View();
        }

        //-- Configuración de parámetros
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Setup()
        {

            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Administración de Bienes Inmuebles y Vía Pública | Ayuntamiento de Orizaba.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Coordinación de Sistemas | Ayuntamiento de Orizaba.";

            return View();
        }
        public ActionResult InvalidProfile()
        {
            ViewBag.Message = "Usuario no autorizado para este módulo.";
            return View();
        }
        public ActionResult AskPermission()
        {
            ViewBag.Message = "Estimado funcionario, solicite el acceso con el jefe o coordinador de su área.";
            return View();
        }
    }
}