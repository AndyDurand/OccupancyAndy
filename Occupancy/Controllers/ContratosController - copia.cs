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
    public class ContratosController : Controller

    {
        private OccupancyEntities db = new OccupancyEntities(); // no puedo tenerlo en readOnly
        int nYearNow, nMonthNow, nDayNow;
        string sPeriodNow;
        // -- Constructor con la fecha actual en numero
        public ContratosController()
        {
            nYearNow = System.DateTime.Now.Year;
            nMonthNow = System.DateTime.Now.Month;
            nDayNow = System.DateTime.Now.Day;
            sPeriodNow = "";
            if (nMonthNow < 10)            
                sPeriodNow = nYearNow.ToString() + "0" + nMonthNow;            
            else            
                sPeriodNow = nYearNow.ToString() + nMonthNow;
                        
        }
        // GET: Contratos
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult Index()
        {

            if (User.IsInRole("AdminArea") || User.IsInRole("AdminAuditor")) 
            {
                var contratos = db.Contratos.Include(c => c.Giros).Include(c => c.Locales).Include(c => c.Personas).Include(c => c.TipoOcupacionUso);

                return View(contratos.ToList());
            }
            else return RedirectToAction("InvalidProfile", "Home");

        }

        // GET: Contratos -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- IndexAdmin() GET-- -- -- -- -- --
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult IndexAdmin()
        {

            //control para filtro
            using (Repositorio<Users> obj = new Repositorio<Users>())
            {
                var u = User.Identity.GetUserId();
                var value = obj.Retrive(x => x.IDASPNETUSER == u).IDUser;
                var valueIdDepto = obj.Retrive(x => x.IDASPNETUSER == u).IDDepto;
                Session["ID_User"] = value;
                Session["ID_Area"] = valueIdDepto;

            }
            int idArea = (int)Session["ID_Area"];

            // Roles SuperAdmin, AdminAuditor 
            if (User.IsInRole("SuperAdmin") || User.IsInRole("AdminAuditor"))
            {                
                // Para la Vista ListAdmin consultar solo los contratos Activos
                var contratos = db.Contratos.Include(c => c.Giros).Include(c => c.Locales).Include(c => c.Personas)
                    .Include(c => c.TipoOcupacionUso)
                    .Include(c => c.Movimientos)
                    .Where(c => c.Activo);
                
                // La vista indexAdmin recibe el IEnumerable de los Contratos
                return View(contratos.ToList());
            }
            // Rol AdminArea, FuncionarioA, FuncionarioB, su área
            else if (User.IsInRole("AdminArea") || User.IsInRole("FuncionarioA") || User.IsInRole("FuncionarioB"))
            {
                var contratos = db.Contratos.Include(c => c.Giros).Include(c => c.Locales).Include(c => c.Personas)
                    .Include(c => c.TipoOcupacionUso)
                    .Include(c => c.Movimientos)
                    .Where(c => c.Locales.IDEspacio == idArea);   //(c => c.Activo);

                // La vista indexAdmin recibe el IEnumerable de los Contratos
                return View(contratos.ToList());
            }
            else
            {
                return RedirectToAction("InvalidProfile", "Home");
            }
        }
        // -- Obtener info general en diccionario de datos
        public void GeneralInfo(Contratos contrato)
        {
            Session["ID_Contrato"] = contrato.IDContrato;
            ViewBag.IDContrato = contrato.IDContrato;
            ViewBag.IDPersona = contrato.IDPersona;
            ViewBag.Saldo = 0;
            ViewBag.nombreComercial = contrato.NombreComercial;
            ViewBag.personaNombre = contrato.Personas.Nombre + " " + contrato.Personas.APaterno + " " + contrato.Personas.AMaterno;
            ViewBag.localNombre = contrato.Locales.Local;
            ViewBag.localMcuadrados = contrato.Locales.MCuadTotales;
            ViewBag.localNumParaCobro = contrato.Locales.NumLocParaCobro;
            ViewBag.localCuotaRenta = contrato.Locales.ImporteRenta;   // Cuota Renta
            if (contrato.Locales.PorMetraje == true)  // dos categorías POR METRAJE, POR LOCAL
            {
                // este es el nombre específico que le dió el usuario 
                //ViewBag.localTipoCuota = accountQuery.First().Contratos.Locales.TipoCuotas.TipoCuota;
                ViewBag.localTipoCuota = "POR METRAJE";
                if (contrato.Locales.NumLocParaCobro != null)
                    // Mientras no haya una regla clara para el cobro de m2 por el factor de UMA.    
                    ViewBag.localImporteRenta = contrato.Locales.ImporteRenta * contrato.Locales.NumLocParaCobro;  // por el no. de Locales para Cobro
                else
                    ViewBag.localImporteRenta = contrato.Locales.ImporteRenta; // por omisión, 1                
            }
            else
            {
                ViewBag.localTipoCuota = "POR LOCAL";
                ViewBag.localImporteRenta = contrato.Locales.ImporteRenta;

            }
            // Adicional
            if (contrato.Locales.TipoCuotas.PorcentajeAdicional != null)
            {
                ViewBag.localCuotaRentaConAdic = Math.Round(ViewBag.localCuotaRenta + ((ViewBag.localCuotaRenta * contrato.Locales.TipoCuotas.PorcentajeAdicional) / 100));
                ViewBag.localImporteRentaConAdic = Math.Round(ViewBag.localImporteRenta + ((ViewBag.localImporteRenta * contrato.Locales.TipoCuotas.PorcentajeAdicional) / 100));
            }

            //ViewBag.localTipoCuota = contrato.Locales.TipoCuotas.TipoCuota;
            ViewBag.EspacioNombre = contrato.Locales.Espacios.Espacio;
            Session["ID_Espacio"] = contrato.Locales.Espacios.IDEspacio;

            ViewBag.naveNombre = contrato.Locales.Naves.Nave;
            if (contrato.Locales.IDSeccion != null )
            {
                ViewBag.seccionNombre = contrato.Locales.Secciones.Seccion;
            }
            else ViewBag.seccionNombre = " ";


            return;
        }

        // -- GET EditMovs desde Contratos y relaciones ejecutamos operaciones sobre Movimientos -- -- -- -- -- -- -- EditMovs(int) GET-- -- -- -- --
        [Authorize(Roles = "SuperAdmin, AdminAuditor, AdminConsulta, AdminArea, FuncionarioA")]
        public ActionResult EditMovs(int? id)
        {
                       
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }            
            Contratos contrato = db.Contratos.Find(id);            
            if (contrato == null)
            {
                return HttpNotFound();
            }
            // Info general 
            GeneralInfo(contrato);
            // Si no tiene movimientos, ni saldo inicial, debo mostrar la tabla aún así
            if (contrato.Movimientos.Count() > 0     ) //   != null)   
            {
                // Recorrer los movimientos para obtener el saldo de la cuenta
                ViewBag.Saldo = SaldoAccount(contrato.Movimientos.ToList());
            }
            else ViewBag.Saldo = 0;

            // Revisar movimientos tipo 3 Renta del Mes. Llenado de listMeses
            AddMonth(id); 
            // La vista EditMovs recibe un objeto Contrato
            return View(contrato);
        }


        // -- Obtener el saldo de la cuenta -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- float SaldoAccount(IEnumerable<Movimientos>)  -- -- --    
        public float SaldoAccount(IEnumerable<Movimientos> listMovs)
        {
            // En los movimientos de Abono, debo considerar que tengan Folio de Recibo y Fecha de Pago
            float sumaC, sumaA;
            sumaC = sumaA = 0;
            foreach (var objM in listMovs)
            {
                if (objM.Estatus  == "ACTIVO")
                    if (objM.TipoMovimiento.Naturaleza.Contains("CARGO"))
                    {
                        sumaC += objM.ImporteTotal;                                     
                    }
                    else if (objM.TipoMovimiento.Naturaleza.Contains("ABONO") && objM.Pagado == true && objM.FechaPago != null)
                    {
                        // Considerar el abono solo si está pagado                        
                        sumaA += objM.ImporteTotal;
                    }
            }
            return sumaC - sumaA;            
        }
        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- Boolean IdentificarMeses(IEnumerable<Movimientos> listMovs, int[,] monthArray)
        public Boolean IdentificarMeses(IEnumerable<Movimientos> listMovs, int[,] monthArray)
        {
            // Recorrer Movimientos, revisar cargos por renta mes, identificar en el array bidimenssional qué meses están cargados
            // [Mes, 0] (1) true or  (0) false
            Boolean existen = false;
            int nMes;
            string sYear, sMonth;          
            foreach (var objM in listMovs)
            {
                if (objM.Estatus == "ACTIVO" && objM.IDTipoMovimiento == 3)
                {
                    // aquí el movimiento de Mes, no necesita estar pagado
                    //if (objM.Pagado == true && objM.FechaPago != null && objM.Periodo != null)
                    //{
                        // revisar Periodo de ese registro                        
                        sYear = objM.Periodo.Substring(0, 4);
                        sMonth = objM.Periodo.Substring(4, 2);
                        nMes = int.Parse(sMonth);

                        if (int.Parse(sYear) == nYearNow)  // registro de mes de este año
                        {
                            monthArray[nMes-1, 0] = 1; // en col 0:  marcar el mes [n-1, 0]
                            existen = true;
                        }
                    //}
                }
            }
            return existen;
        }
        public void BuscarArbitrio()
        {

        }


        // --  GET  Agregar  Saldo Inicial  -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- AddBalance() GET  -- -- -- -- -- -- -- -- -- -- --       
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult AddBalance(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contratos contratos = db.Contratos.Find(id);
            if (contratos == null)
            {
                return HttpNotFound();
            }                      
            return View();
        }

        // --  POST  Agregar Saldo Inicial -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- AddBalance(importe, obs, tipoSaldo) POST  -- -- -- --     
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBalance(float importe, string obs, string tipoSaldo)
        {
            // Session["ID_User"]

            // Ya tengo  Session["ID_Contrato"]
            // IDTipoMovimiento, Importe y Observaciones son los campos leidos
            // idUser, idContrato, fechas System.DateTime.Now;            
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);
            int idC = 0;
            if (contrato == null)
            {
                return HttpNotFound();
            }
            if ((ModelState.IsValid))
            {
                idC = contrato.IDContrato;

                // Crear si importe es diferente de cero
                if (importe != 0)
                {
                    Movimientos mov = new Movimientos();                    
                    mov.IDContrato = contrato.IDContrato;  

                    if (tipoSaldo == "SALDO INICIAL REZAGO")
                    {
                        mov.IDTipoMovimiento = 1;
                        // Fecha vencimiento hoy, ya que que es un saldo de Rezago
                        mov.FechaVencimiento = System.DateTime.Now;
                        obs = obs + ". SALDO INICIAL REZAGO";
                    }
                    else if (tipoSaldo == "SALDO INICIAL CORRIENTE")
                    {
                        mov.IDTipoMovimiento = 2;
                        // Fecha vencimiento, fin de este mes
                        mov.FechaVencimiento = new System.DateTime(nYearNow, nMonthNow, System.DateTime.DaysInMonth(nYearNow,nMonthNow));
                        obs = obs + ". SALDO INICIAL CORRIENTE";
                    }
                    // saldo inicial a Favor 27/07/22
                    // ..
                    // 
                    mov.IDUser = (int)Session["ID_User"];
                    mov.ImporteTotal = importe;
                    mov.FechaEmision = System.DateTime.Now;   
                    mov.Observaciones = obs;
                    mov.Estatus = "ACTIVO";
                    // pendiente cómo desgloso el Saldo Inicial, sea de Corriente o de Rezago, ya que puede tener accesorios
                    mov.Corriente = mov.Adicional = mov.Recargos = mov.Rezago = mov.AdicionalRezago = mov.RecargoRezago =  0;
                    mov.Multa = mov.Honorarios = mov.Ejecucion = 0;
                    // El periodo no aplica aquí
                    // Agregar movimiento saldo
                    db.Movimientos.Add(mov);
                    db.SaveChanges();
                }                
            }           
            return RedirectToAction("EditMovs","Contratos", new { id = idC });             

        }

        // --  GET AddMonth  -- -- -- -- -- -- -- -- -- -- -- -- -- AddMonth() GET nou ya no es ActionResult, lo llamo desde el método GET de EditMovs()- -- -- -- -- -- --
        //public ActionResult AddMonth(int? id)     
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public void AddMonth(int? id)
        {
            ViewBag.algotxt = "desde AddMonth";
            // Añadir solo meses del año actual 
            // Del mes actual en adelante
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contratos contratos = db.Contratos.Find(id);
            if (contratos == null)
            {
                //return HttpNotFound();
            }
            // Permitir agregar mes del año actual, el Stored Procedure realiza inserción del nuevo Cargo del Mes
            // existen casos en los que puede pagar por adelantado, por lo que se debe generar el Cargo y la Orden de Pago que corresponda
            if (contratos.Movimientos.Count() >0 )  //// != null)
            {
                // Recorrer movimientos para ver si del año actual ya están cargados meses 
                IEnumerable<Movimientos> listMovimientos = contratos.Movimientos.ToList(); 
                int[,] arrayMonth = new int[12,2] { { 0,0}, { 0,0},{ 0,0},{ 0,0},{ 0,0},{ 0,0},{ 0,0},{ 0,0},{ 0,0},{ 0,0},{ 0,0},{ 0,0} };  // la columna 0 sería el checked para el list check de la vista, sigo en EditMovs, la columna 1 ya no la uso
                Boolean existeMeses = IdentificarMeses(listMovimientos, arrayMonth);

                ViewBag.Year = nYearNow;

                // Armar el List con los meses que no tiene cargados, pero mayores que el mes actual

                List<SelectListItem> listMonth= new List<SelectListItem>();
                SelectListItem itemMonth; 
                // mes actual,el mes es el indice [índice - 1], la columna 1 no la utilizo por ahora
                if (existeMeses)
                {                    
                    for (int n = 0; n < 12; n++)
                    {                        
                        switch (n)
                        {
                            case 0:
                                if (arrayMonth[n, 0] == 1) // col 0: encontrado en Movimientos, lo desabilito --- es  0,1
                                {
                                    // no los agrego, ya que el disabled no lo respeta 
                                    // itemMonth = new SelectListItem { Text = "ENERO", Value = "01", Disabled = true };
                                }
                                else
                                {                                     
                                    if (nMonthNow < 1)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "ENERO", Value = "01", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }                                    
                                }                                
                                break;
                            case 1:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {
                                    //itemMonth = new SelectListItem { Text = "FEBRERO", Value = "02", Disabled = true };
                                }
                                else
                                {
                                    if (nMonthNow < 2)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "FEBRERO", Value = "02", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }                                        
                                }                                
                                break;
                            case 2:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {
                                    //itemMonth = new SelectListItem { Text = "MARZO", Value = "03", Disabled = true };
                                }
                                else
                                {
                                    if (nMonthNow < 3)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "MARZO", Value = "03", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }                                        
                                }                                
                                break;
                            case 3:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {
                                    //itemMonth = new SelectListItem { Text = "ABRIL", Value = "04", Disabled = true };
                                }
                                else
                                {
                                    if (nMonthNow < 4)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "ABRIL", Value = "04", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }
                                        
                                }                                
                                break;
                            case 4:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {
                                    //itemMonth = new SelectListItem { Text = "MAYO", Value = "05", Disabled = true };
                                }
                                else
                                {
                                    if (nMonthNow < 5)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "MAYO", Value = "05", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }                                        
                                }                                
                                break;
                            case 5:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {
                                }
                                else
                                {
                                    if (nMonthNow < 6)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "JUNIO", Value = "06", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }
                                        
                                }                                
                                break;
                            case 6:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {                                    
                                }
                                else
                                {
                                    if (nMonthNow < 7)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "JULIO", Value = "07", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }                                        
                                }                                
                                break;
                            case 7:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {                                    
                                }
                                else
                                {
                                    if (nMonthNow < 8)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "AGOSTO", Value = "08", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }
                                        
                                }
                                
                                break;
                            case 8:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {
                                }
                                else
                                {
                                    if (nMonthNow < 9)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "SEPTIEMBRE", Value = "09", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }                                        
                                }                                
                                break;
                            case 9:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {                                    
                                }
                                else
                                {
                                    if (nMonthNow < 10)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "OCTUBRE", Value = "10", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }                                        
                                }                                
                                break;
                            case 10:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {                                    
                                }
                                else
                                {
                                    if (nMonthNow < 11)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "NOVIEMBRE", Value = "11", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }                                        
                                }                                
                                break;
                            case 11:
                                if (arrayMonth[n, 0] == 1) // encontrado en Movimientos
                                {                                    
                                }
                                else
                                {
                                    if (nMonthNow < 12)  // solo se consideraría mes por adelantado
                                    {
                                        itemMonth = new SelectListItem { Text = "DICIEMBRE", Value = "12", Disabled = false };
                                        listMonth.Add(itemMonth);
                                    }                                        
                                }                                
                                break;
                        }                       
                    }
                    
                    ViewBag.sMes = 1;    //los disabled no los está respetando ?                    
                    ViewBag.listMeses = new SelectList(listMonth, "Value", "Text", ViewBag.sMes);
                    
                }  
                // if ! existen movimientos tipo mes,
                else
                {
                    ViewBag.listMeses = new SelectList(new List<SelectListItem>
                        { new SelectListItem{Text="ENERO", Value="01", Disabled = false}, new SelectListItem{Text="FEBRERO", Value="02", Disabled = false },
                          new SelectListItem{Text="MARZO", Value="03", Disabled = false }, new SelectListItem{Text="ABRIL", Value="04", Disabled = false },
                          new SelectListItem{Text="MAYO", Value="05", Disabled = false }, new SelectListItem{Text="JUNIO", Value="06", Disabled = false },
                          new SelectListItem{Text="JULIO", Value="07", Disabled = false }, new SelectListItem{Text="AGOSTO", Value="08", Disabled = false },
                          new SelectListItem{Text="SEPTIEMBRE", Value="09", Disabled = false }, new SelectListItem{Text="OCTUBRE", Value="10", Disabled = false },
                          new SelectListItem{Text="NOVIEMBRE", Value="11", Disabled = false }, new SelectListItem{Text="DICIEMBRE", Value="12", Disabled = false },
                        }, "Value", "Text", ViewBag.sMes);
                }

            }    // if movimientos != null
            else
            {
                ViewBag.listMeses = new SelectList(new List<SelectListItem>
                        { new SelectListItem{Text="ENERO", Value="01", Disabled = false}, new SelectListItem{Text="FEBRERO", Value="02", Disabled = false },
                          new SelectListItem{Text="MARZO", Value="03", Disabled = false }, new SelectListItem{Text="ABRIL", Value="04", Disabled = false },
                          new SelectListItem{Text="MAYO", Value="05", Disabled = false }, new SelectListItem{Text="JUNIO", Value="06", Disabled = false },
                          new SelectListItem{Text="JULIO", Value="07", Disabled = false }, new SelectListItem{Text="AGOSTO", Value="08", Disabled = false },
                          new SelectListItem{Text="SEPTIEMBRE", Value="09", Disabled = false }, new SelectListItem{Text="OCTUBRE", Value="10", Disabled = false },
                          new SelectListItem{Text="NOVIEMBRE", Value="11", Disabled = false }, new SelectListItem{Text="DICIEMBRE", Value="12", Disabled = false },
                        }, "Value", "Text", ViewBag.sMes);

            }            
            return;
        }

        // -- Regresar el nombre del mes, recibe número del mes
        private string MonthName(int n)
        {
            string sMonthName = "";
            switch (n)
            {
                case 1:
                    sMonthName = "ENERO";
                    break;
                case 2:
                    sMonthName = "FEBRERO";
                    break;
                case 3:
                    sMonthName = "MARZO";
                    break;
                case 4:
                    sMonthName = "ABRIL";
                    break;
                case 5:
                    sMonthName = "MAYO";
                    break;
                case 6:
                    sMonthName = "JUNIO";
                    break;
                case 7:
                    sMonthName = "JULIO";
                    break;
                case 8:
                    sMonthName = "AGOSTO";
                    break;
                case 9:
                    sMonthName = "SEPTIEMBRE";
                    break;
                case 10:
                    sMonthName = "OCTUBRE";
                    break;
                case 11:
                    sMonthName = "NOVIEMBRE";
                    break;
                case 12:
                    sMonthName = "DICIEMBRE";
                    break;
            }
            return sMonthName;
        }

        // -- Regresar el string periodo, recibe número de mes
        public string PeriodName(int n, int y)
        {
            if (n < 10)
                return  y.ToString() + "0" + nMonthNow;
            else
                return y.ToString() + nMonthNow;
        }

        // --  POST AddMonth  -- -- -- -- -- -- -- -- -- -- -- -- -- -- --AddMonth() POST -- -- -- -- -- -- -- 
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMonth( string obs, string sMes)
        {
            
            // Ya tengo  Session["ID_Contrato"]            
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);
            int idC = 0;
            if (contrato == null)
            {
                return HttpNotFound();
            }
            if ((ModelState.IsValid))
            {
                //nYearNow  nMonthNow  nDayNow
                idC = contrato.IDContrato;                
                int nMes = int.Parse(sMes); //el Mes, parámetro del SelectList
                float fTotal, fRedondeo, fCorriente, fAdicional, fRecargos, fRezago,fAdicionalRezago, fRecargoRezago ;
                fTotal = fRedondeo = fCorriente = fAdicional = fRecargos = fRezago = fAdicionalRezago = fRecargoRezago = 0;
                int nTotal = 0;
                int nTipoMov = 3; // Renta del Mes
                DateTime dVencim = System.DateTime.Now;

                // Solo son meses del año actual, lo de los años anteriores debe ser Saldo Inicial Rezago
                if (nMes < nMonthNow)  //-- mes con Recargo
                {
                    nTipoMov = 3;
                    // Dias del mes seleccionado, debe ser de sMes
                    dVencim = new System.DateTime(nYearNow, int.Parse(sMes), System.DateTime.DaysInMonth(nYearNow, int.Parse(sMes)));
                    obs = obs + ".CARGO DE MES " + MonthName(int.Parse(sMes)) + " " + nYearNow.ToString() + " CON RECARGOS.";
                    // Derechos --
                    //if (contrato.Locales.PorMetraje == true)  // 
                    //{
                        if (contrato.Locales.NumLocParaCobro != null  || contrato.Locales.NumLocParaCobro != 0)
                        {
                            fRezago = (float)(contrato.Locales.ImporteRenta * contrato.Locales.NumLocParaCobro); // el no. de locales
                        }                                        
                        else
                        {
                            fRezago = (float)(contrato.Locales.ImporteRenta); // por omisión, 1
                        }
                    //}                        
                    //else fRezago = (float)(contrato.Locales.ImporteRenta); // la cuota                     
                    // Adicional rezago --
                    if (contrato.Locales.TipoCuotas.PorcentajeAdicional != null )
                    {
                        fAdicionalRezago = (float) ((fRezago * contrato.Locales.TipoCuotas.PorcentajeAdicional) / 100); //ok
                    }
                    else fAdicionalRezago = 0;
                    // Recargos --
                    if (contrato.Locales.TipoCuotas.PorcentajeRecargoMensual != null)
                    {
                        // expresado en porcentaje 2.5. Rezago va sobre los derechos, sin redondear, sin el adic
                        fRecargoRezago  = (float) (( fRezago * contrato.Locales.TipoCuotas.PorcentajeRecargoMensual / 100));
                        fRecargoRezago = (float)Math.Round(fRecargoRezago, 2);
                    }
                    else fRecargos = 0;
                    // Total --
                    fTotal = fRezago + fAdicionalRezago + fRecargoRezago;                    
                }
                else if ((nMes == nMonthNow) || (nMes > nMonthNow)) // -- Mes actual o mes adelantado 
                {                 
                    // Derechos
                    //if (contrato.Locales.PorMetraje == true)
                    //{
                        if (contrato.Locales.NumLocParaCobro != null || contrato.Locales.NumLocParaCobro != 0)
                        {
                            fCorriente = (float)(contrato.Locales.ImporteRenta * contrato.Locales.NumLocParaCobro); // el no. de locales 
                        }
                        else
                        {
                            fCorriente = (float)(contrato.Locales.ImporteRenta); // por omisión, 1
                        }
                    //}
                    //else fCorriente = (float)(contrato.Locales.ImporteRenta);
                    // Adicional corriente
                    if (contrato.Locales.TipoCuotas.PorcentajeAdicional != null)
                    {
                        fAdicional = (float)((fCorriente * contrato.Locales.TipoCuotas.PorcentajeAdicional) / 100);
                    }
                    else fAdicional = 0;
                    // ya pasó del día, mes con Recargo
                    if ((nMes == nMonthNow) && (nDayNow > contrato.DiaFijoPago))
                    {
                        nTipoMov = 3;
                        dVencim = new System.DateTime (nYearNow, nMonthNow, System.DateTime.DaysInMonth(nYearNow, nMonthNow));
                        obs = obs + ". MES " + MonthName(int.Parse(sMes)) + " " + nYearNow.ToString() + " CON RECARGOS";
                        if (contrato.Locales.TipoCuotas.PorcentajeRecargoMensual != null)
                        {
                            // expresado en porcentaje 2.5, 
                            fRecargos = (float)((fCorriente * contrato.Locales.TipoCuotas.PorcentajeRecargoMensual / 100));
                            fRecargos = (float)Math.Round(fRecargos, 2);
                        }
                        else fRecargos = 0;
                    }
                    else // mes adelantado
                    {                       
                        fRecargos = 0;
                        // Dias del mes seleccionado, debe ser de sMes
                        dVencim = new System.DateTime(nYearNow, int.Parse(sMes), System.DateTime.DaysInMonth(nYearNow, int.Parse(sMes) ));
                        obs = obs + ". MES " + MonthName(int.Parse(sMes)) + " " + nYearNow.ToString();
                    }
                    // Total --
                    fTotal = fCorriente + fAdicional + fRecargos;
                }
                Movimientos mov = new Movimientos();
                mov.IDContrato = contrato.IDContrato;
                mov.IDTipoMovimiento = nTipoMov;
                mov.Pagado = false;
                mov.IDUser = (int)Session["ID_User"];
                

                // -- Redondeo, hasta el final
                nTotal = (int)fTotal; //parte entera
                fRedondeo = (float)Math.Round (fTotal - nTotal, 2);
                if (fRedondeo > 0.6)
                    nTotal = nTotal + 1;
                mov.ImporteTotal = nTotal;
                mov.Redondeo = fRedondeo;
                // Desglose
                if (nMes < nMonthNow)
                {
                    mov.Rezago = fRezago;
                    mov.AdicionalRezago = fAdicionalRezago;
                    mov.RecargoRezago = fRecargoRezago;
                    mov.Corriente = mov.Adicional = mov.Recargos = 0;
                }
                else
                {
                    mov.Corriente = fCorriente;
                    mov.Adicional = fAdicional;
                    mov.Recargos = fRecargos;
                    mov.Rezago = mov.AdicionalRezago = mov.RecargoRezago = 0;
                }    
                // Accesorios los tengo en 0´s ..
                mov.Multa = mov.Honorarios = mov.Ejecucion = 0;
                mov.FechaEmision = System.DateTime.Now;
                // Fecha vencimiento
                mov.FechaVencimiento = dVencim;
                mov.Observaciones = obs; 
                mov.Estatus = "ACTIVO";                
                // Periodo
                mov.Periodo = nYearNow.ToString() + sMes;
                // Agregar movimiento mes
                db.Movimientos.Add(mov);
                db.SaveChanges();
            }
            return RedirectToAction("EditMovs", "Contratos", new { id = idC });
        }
        //
        // -- 
        private void InitArrayDebit(float[,] array, int fil, int col)
        {
            for (int i = 0; i < fil; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    array[i, j] = 0;
                }
            }
            return;
        }
        // --
        private void InitArrayObsPer(string[,] array, int fil, int col)
        {
            for (int i = 0; i < fil; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    array[i, j] = "";
                }
            }
            return;
        }
        //
        private int GeneraArrayDebitMonths(float[,] array, int nunMeses, int col, float porcRecInicial, int nYear)
        {
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);
            if (contrato == null)
            {
                return 1;
            }

            for (int i = 0; i < numMesesCorriente; i++)
            {

                // -- Derechos.                [0, 0] Corriente
                if (contrato.Locales.NumLocParaCobro != null || contrato.Locales.NumLocParaCobro != 0)  //contrato.Locales.PorMetraje == true, dato que primero podria validar
                {
                    aRecargosCorriente[i, 0] = (float)(contrato.Locales.ImporteRenta * contrato.Locales.NumLocParaCobro); // el no. de locales 
                }
                else
                {
                    aRecargosCorriente[i, 0] = (float)(contrato.Locales.ImporteRenta); // por omisión, 1
                }
                // -- Adicional corriente.     [0, 1] Adicional
                if (contrato.Locales.TipoCuotas.PorcentajeAdicional != null)
                {
                    aRecargosCorriente[i, 1] = (float)((aRecargosCorriente[i, 0] * contrato.Locales.TipoCuotas.PorcentajeAdicional) / 100);
                }
                else aRecargosCorriente[i, 1] = 0;
                // -- Porcentaje Recargos.     [0, 2] Porc Recargo  -- -- --                        
                if (i == 0) // primer mes que se añade
                    aRecargosCorriente[i, 2] = fPorcentajeR;
                else
                    aRecargosCorriente[i, 2] = aRecargosCorriente[i, 2] + fPorcentajeR;
                //                          [0, 3] Recargos
                // -- Recargos: siempre sobre el importe total de corriente más el adicional, va con redondeo
                aRecargosCorriente[i, 3] = (aRecargosCorriente[i, 0] + aRecargosCorriente[i, 1]) * aRecargosCorriente[i, 2] / 100;
                aRecargosCorriente[i, 3] = (float)Math.Round(aRecargosCorriente[i, 3], 2);

                //  -- Total importe: corriente + adic +  recargo
                //                           [0, 4] Redondeo // [0, 5] Total importe 
                fTotal = aRecargosCorriente[i, 0] + aRecargosCorriente[i, 1] + aRecargosCorriente[i, 3];
                nTotalParteEntera = (int)fTotal;
                fRedondeo = (float)Math.Round(fTotal - nTotalParteEntera, 2);
                if (fRedondeo > 0.6)
                {
                    nTotalParteEntera++;
                }
                aRecargosCorriente[i, 4] = fRedondeo;
                aRecargosCorriente[i, 5] = nTotalParteEntera;
                //
                // -- Año, Mes      // [0, 6] Mes // [0, 7] Año
                aRecargosCorriente[i, 6] = nMesAgrega;
                aRecargosCorriente[i, 7] = nYearNow;

                // aObsPer          // [0, 0] Observaciones //  [0, 1] Periodo
                aObsPer[i, 0] = "SALDO DEUDOR. MES CON RECARGOS: " + MonthName(nMesAgrega) + " " + nYearNow.ToString();
                aObsPer[i, 1] = PeriodName((int)aRecargosCorriente[i, 6], (int)aRecargosCorriente[i, 7]);
                // nTipoMov = 3;
                // dVencim = new System.DateTime(nYearNow, nMonthNow, System.DateTime.DaysInMonth(nYearNow, nMonthNow));

                //
                nMesAgrega--;
            }
            return 0;
        }
        //

        private void ProcesaArrayDebitMonths(float[,] array, int nunMeses, int col)
        {
            return;
        }



        // --  GET  Agregar  Saldo DEUDOR con número de meses - -- -- -- -- -- -- -- -- -- -- AddMonthsDebitBalance() GET  -- -- -- -- -- -- -- -- -- -- --   * * *   
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult AddMonthsDebitBalance(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contratos contratos = db.Contratos.Find(id);
            if (contratos == null)
            {
                return HttpNotFound();
            }
            return View();
        }

        // --  POST  Agregar Saldo Saldo DEUDOR con número de meses - -- -- -- -- -- -- -- -- -- -- AddMonthsDebitBalance() POST  * * *   
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMonthsDebitBalance(int nMeses, string tipoSaldo)
        {
            // solo me quedaré con nMeses; tipoSaldo;  puedo identificarlo como "SALDO DEUDOR" o "SALDO A FAVOR"
            // Numero de Meses que se deben sea en Corriente o en Rezago; nMeses validado en el input del form, min 1,  max 15, por ahora
            string obs = " ";
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);
            int idC = 0;
            int nTipoMov = 3;
            string sPeriodo = "";
            float fPorcRec;
            int numMesesCorriente = 0; 
            int numMesesRezago = 0;
            int numMesesRezagoOtro = 0;

            DateTime dVencim = System.DateTime.Now;

            // Array de cálculo para meses corrientes, pero van con Recargo
            // [0, 0] Corriente //  [0, 1] Adicional //  [0, 2] Porc Recargo // [0, 3] Recargos // [0, 4] Redondeo // [0, 5] Total importe // [0, 6] Mes // [0, 7] Año
            int nCol = 8;
            float[,] aRecargosCorriente = new float[12, nCol];
            // Array de cálculo para meses de Rezago, 
            // [0, 0] Rezago //  [0, 1] Adicional //  [0, 2] Porc Recargo // [0, 3] Recargo Rezago //  [0, 4] Redondeo //  [0, 5] Total importe // [0, 6] Mes // [0, 7] Año
            float[,] aRecargosRezago = new float[12, nCol];
            float[,] aRecargosRezagoOtro = new float[12, nCol];
            // Arrays Obs y Periodos
            // [0, 0] Observaciones //  [0, 1] Periodo
            string[,] aObsPer = new string[12, 2];


            InitArrayDebit(aRecargosCorriente, 12, nCol);
            InitArrayDebit(aRecargosRezago, 12, nCol);
            InitArrayDebit(aRecargosRezagoOtro, 12, nCol);
            InitArrayObsPer(aObsPer, 12, 2);

            //nYearNow     nMonthNow    nDayNow            
            if (contrato == null)
            {
                return HttpNotFound();
            }

            if (tipoSaldo == "SALDO DEUDOR")
            {
                // ok, primero el saldo deudor
            }
                        
            if ((ModelState.IsValid))
            {
                idC = contrato.IDContrato;

                if (contrato.Locales.TipoCuotas.PorcentajeRecargoMensual != null)
                {
                    fPorcRec = (float)contrato.Locales.TipoCuotas.PorcentajeRecargoMensual;
                }
                else
                {
                    return HttpNotFound();
                }


                // -- 1.-  Evalúo el número de Meses a cargar de saldo deudor --
                if (nMeses == nMonthNow ) // los meses a cargar son todos Corriente, se asumen los meses inmediatos anteriores
                {
                    numMesesCorriente = nMeses;
                    numMesesRezago = 0;
                    // nYearNow formará el sPeriodo, es por mes que se añade; el periodo formado por el año actual

                }
                else if (nMeses < nMonthNow) // son meses Corriente  --
                {
                    numMesesCorriente = nMeses; 
                    numMesesRezago = 0;
                    // sPeriodo, el periodo formado por el año actual
                }
                else if (nMeses > nMonthNow) // tiene meses de Rezago..         //* 15; 1
                {
                    numMesesCorriente = nMonthNow;  // sPeriodo del año actual  //* 1         // 9  15;   9   //     5;  10     // nMontNow 2; nMeses 15  =2 //  7; 15  = 7 
                    numMesesRezago = nMeses - nMonthNow;                        //* 14      // 15- 9 = 6   //     10-5=5    //  15-2 = 13 // 15-7= 8;
                    if (numMesesRezago <= 12)     //  15-1 = 14                        // 15  1;  1 //  15-1 = 14
                    {                        
                        numMesesRezago = nMeses - nMonthNow; // me quedo con ese num de meses de rezago
                        // sPeriodo, el periodo formado por el año actual - 1 

                    }
                    else  //  15 - 1=14  * -- * -- OJO
                    {
                        // no creo q deba añadir más mese de rezago serian ya 2 años de rezago?             
                        numMesesRezagoOtro = numMesesRezago - 12;   // 14-12 = 2
                        numMesesRezago = 12;                        // 12    
                        // sPeriodo, el periodo formado por el año actual -1 y -2
                    }

                }
                // -- 2.- Generar los movimientos en los arrays
                // [0, 0] Corriente //  [0, 1] Adicional //  [0, 2] Porc Recargo // [0, 3] Recargos // [0, 4] Redondeo // [0, 5] Total importe // [0, 6] Mes // [0, 7] Año                
                // 150% son 5 años = 60 meses
                float fPorcentajeR = (float)contrato.Locales.TipoCuotas.PorcentajeRecargoMensual; // primer valor de porc de recargos, 2.5               
                int nMesAgrega = 0;
                int nTotalParteEntera;
                float fTotal, fRedondeo;

                if (numMesesCorriente != 0 && numMesesRezago == 0)
                {
                    // fPorcRec
                    nMesAgrega = nMonthNow - 1;                 // -- -- -- -- --    

                    int nresul = GeneraArrayDebitMonths(aRecargosCorriente, nMesAgrega, nCol, fPorcRec, nYearNow);

                    for (int i = 0; i < numMesesCorriente; i++)
                    {                                               
                        
                        // -- Derechos.                [0, 0] Corriente
                        if (contrato.Locales.NumLocParaCobro != null || contrato.Locales.NumLocParaCobro != 0)  //contrato.Locales.PorMetraje == true, dato que primero podria validar
                        {
                            aRecargosCorriente[i, 0] = (float)(contrato.Locales.ImporteRenta * contrato.Locales.NumLocParaCobro); // el no. de locales 
                        }
                        else
                        {
                            aRecargosCorriente[i, 0] = (float)(contrato.Locales.ImporteRenta); // por omisión, 1
                        }
                        // -- Adicional corriente.     [0, 1] Adicional
                        if (contrato.Locales.TipoCuotas.PorcentajeAdicional != null)
                        {
                            aRecargosCorriente[i, 1] = (float)((aRecargosCorriente[i, 0] * contrato.Locales.TipoCuotas.PorcentajeAdicional) / 100);
                        }
                        else aRecargosCorriente[i, 1] = 0;
                        // -- Porcentaje Recargos.     [0, 2] Porc Recargo  -- -- --                        
                        if (i == 0) // primer mes que se añade
                            aRecargosCorriente[i, 2] = fPorcentajeR;
                        else
                            aRecargosCorriente[i, 2] = aRecargosCorriente[i, 2] + fPorcentajeR;
                        //                          [0, 3] Recargos
                        // -- Recargos: siempre sobre el importe total de corriente más el adicional, va con redondeo
                        aRecargosCorriente[i, 3] = (aRecargosCorriente[i, 0] + aRecargosCorriente[i, 1]) * aRecargosCorriente[i, 2] /100;
                        aRecargosCorriente[i, 3] = (float)Math.Round(aRecargosCorriente[i, 3], 2);

                        //  -- Total importe: corriente + adic +  recargo
                        //                           [0, 4] Redondeo // [0, 5] Total importe 
                        fTotal = aRecargosCorriente[i, 0] + aRecargosCorriente[i, 1] + aRecargosCorriente[i, 3];
                        nTotalParteEntera = (int) fTotal;
                        fRedondeo = (float)Math.Round(fTotal - nTotalParteEntera, 2);
                        if (fRedondeo > 0.6)
                        {
                            nTotalParteEntera++;
                        }
                        aRecargosCorriente[i, 4] = fRedondeo;
                        aRecargosCorriente[i, 5] = nTotalParteEntera;
                        //
                        // -- Año, Mes      // [0, 6] Mes // [0, 7] Año
                        aRecargosCorriente[i, 6] = nMesAgrega;
                        aRecargosCorriente[i, 7] = nYearNow;

                        // aObsPer          // [0, 0] Observaciones //  [0, 1] Periodo
                        aObsPer[i, 0] = "SALDO DEUDOR. MES CON RECARGOS: " + MonthName(nMesAgrega) + " " + nYearNow.ToString();
                        aObsPer[i, 1] = PeriodName( (int) aRecargosCorriente[i, 6], (int) aRecargosCorriente[i, 7] );
                        // nTipoMov = 3;
                        // dVencim = new System.DateTime(nYearNow, nMonthNow, System.DateTime.DaysInMonth(nYearNow, nMonthNow));
                        
                        //
                        nMesAgrega--;                    
                }
                if (numMesesCorriente == 0 && numMesesRezago != 0)
                {

                }                


                // -- 3.- Recorrer arrays para añadir Movimientos..

                
                // Crear si importe es diferente de cero
                if (nMeses != 0)    /// -----------****  nMeses.   si el contrato es por metraje (locales.NumLocParaCobro >=1; locales.PorMetraje == True), es decir n numeros de locales se les cobra
                    // se calculan Recargos, si no lo es, no se calcularán recargos
                {
                    
                        
                        Movimientos mov = new Movimientos();
                    mov.IDContrato = contrato.IDContrato;

                    if (tipoSaldo == "SALDO INICIAL REZAGO")
                    {
                        mov.IDTipoMovimiento = 1;
                        // Fecha vencimiento hoy, ya que que es un saldo de Rezago
                        mov.FechaVencimiento = System.DateTime.Now;
                        obs = obs + ". SALDO INICIAL REZAGO";
                    }
                    else if (tipoSaldo == "SALDO INICIAL CORRIENTE")
                    {
                        mov.IDTipoMovimiento = 2;
                        // Fecha vencimiento, fin de este mes
                        mov.FechaVencimiento = new System.DateTime(nYearNow, nMonthNow, System.DateTime.DaysInMonth(nYearNow, nMonthNow));
                        obs = obs + ". SALDO INICIAL CORRIENTE";
                    }
                    // saldo inicial a Favor 27/07/22
                    // ..
                    // 
                    mov.IDUser = (int)Session["ID_User"];
                    mov.ImporteTotal = 0;  //// importe;  //// -***** 
                    mov.FechaEmision = System.DateTime.Now;
                    mov.Observaciones = obs;
                    mov.Estatus = "ACTIVO";
                    // pendiente cómo desgloso el Saldo Inicial, sea de Corriente o de Rezago, ya que puede tener accesorios
                    mov.Corriente = mov.Adicional = mov.Recargos = mov.Rezago = mov.AdicionalRezago = mov.RecargoRezago = 0;
                    mov.Multa = mov.Honorarios = mov.Ejecucion = 0;
                    // 
                    //// Accesorios los tengo en 0´s ..
                    //mov.Multa = mov.Honorarios = mov.Ejecucion = 0;
                    //mov.FechaEmision = System.DateTime.Now;
                    //// Fecha vencimiento
                    //mov.FechaVencimiento = dVencim;
                    //mov.Observaciones = obs;
                    //mov.Estatus = "ACTIVO";
                    //// Periodo
                    //mov.Periodo = nYearNow.ToString() + sMes;
                    // Agregar movimiento mes
                    // Agregar movimiento saldo
                    db.Movimientos.Add(mov);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("EditMovs", "Contratos", new { id = idC });

        }

        


        //-- GET -- -- -- -- -- -- -- POST -- -- -- -- -- -- -- -- -- -- -- -- -- -- AddFullOrder()-- -- -- -- -- -- -- 
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult AddFullOrden(int id)
        {
            //int id = int.Parse(TempData["idmov"].ToString());            
            //TempData.Keep("idmov");

            // el espacio, para filtrar en tbl CruceOrden

            Movimientos movimientos = db.Movimientos.Find(id);
            if (movimientos == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid)
            {
                ViewBag.tipoMovim = movimientos.TipoMovimiento;
                ViewBag.totalMovim = movimientos.ImporteTotal;



            }
            return View();
        }
        // --  POST Generar ---->  orden de Pago importe completo para      un Cargo listado en los Movimientos --POST -- --AddFullOrder() -- -- -- -- -- 
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFullOrden(int idMov, float partial, string obs)
        {
            // Session["ID_User"]
            // Ya tengo  Session["ID_Contrato"]            
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);
            int idC = 0;
            if (contrato == null)
            {
                return HttpNotFound();
            }
            if ((ModelState.IsValid))
            {
                if (contrato == null)
                {
                    return HttpNotFound();
                }
                if ((ModelState.IsValid))
                {
                    //nYearNow  nMonthNow  nDayNow
                    idC = contrato.IDContrato;
                    //int nMes = int.Parse(sMes); //el Mes, parámetro del SelectList
                    //float fTotal, fRedondeo, fCorriente, fAdicional, fRecargos, fRezago, fAdicionalRezago, fRecargoRezago;
                    //fTotal = fRedondeo = fCorriente = fAdicional = fRecargos = fRezago = fAdicionalRezago = fRecargoRezago = 0;
                    //int nTotal = 0;
                    //int nTipoMov = 3;
                    //DateTime dVencim = System.DateTime.Now;
                    // a Movimientos y a Ordenes
                }

            }
            return RedirectToAction("EditMovs", "Contratos", new { id = idC });
        }
        // --  POST Generar ---->  orden de Pago Parcial para      un Cargo listado en los Movimientos
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPartialOrder(int idMov, float partial, string obs)
        {
            // Session["ID_User"]
            // Ya tengo  Session["ID_Contrato"]            
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);
            int idC = 0;
            if (contrato == null)
            {
                return HttpNotFound();
            }
            if ((ModelState.IsValid))
            {
                if (contrato == null)
                {
                    return HttpNotFound();
                }
                if ((ModelState.IsValid))
                {
                    //nYearNow  nMonthNow  nDayNow
                    idC = contrato.IDContrato;
                    //int nMes = int.Parse(sMes); //el Mes, parámetro del SelectList
                    //float fTotal, fRedondeo, fCorriente, fAdicional, fRecargos, fRezago, fAdicionalRezago, fRecargoRezago;
                    //fTotal = fRedondeo = fCorriente = fAdicional = fRecargos = fRezago = fAdicionalRezago = fRecargoRezago = 0;
                    //int nTotal = 0;
                    //int nTipoMov = 3;
                    //DateTime dVencim = System.DateTime.Now;
                    // a Movimientos y a Ordenes
                }

            }
            return RedirectToAction("EditMovs", "Contratos", new { id = idC });
        }


        // GET: Contratos/Details/5 -- -- --- --- -- -- --- --- 
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Details(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contratos contratos = db.Contratos.Find(id);
            if (contratos == null)
            {
                return HttpNotFound();
            }
            // Info general 
            GeneralInfo(contratos);
            return View(contratos);
        }

        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        // GET: Contratos/Create    -- -- -- -- -- Create desde Personas/Index -- con id de Persona.IDPersona
        public ActionResult Create(int? id)
        {
            // id es IDPersona, agregar Contrato
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Personas personas = db.Personas.Find(id);
            if (personas == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDPersona = personas.IDPersona;
            TempData["PersonaID"] = personas.IDPersona;
            ViewBag.nombrePersona = personas.Nombre + " " + personas.APaterno + " " + personas.AMaterno;           
            // de qué Espacio .
            // solo locales disponibles, no ocupados           
            int IDDeptoUser = (int)Session["ID_Area"];                    

            var girosQry = from g in db.Giros
                           orderby g.Giro
                           select g;
            ViewBag.IDGiro = new SelectList(girosQry.AsNoTracking(), "IDGiro", "Giro");

            var espaciosQry = from esp in db.Espacios
                              orderby esp.Espacio
                              where esp.IDEspacio == IDDeptoUser
                              select esp;
            ViewBag.IDEspacio = new SelectList(espaciosQry.AsNoTracking(), "IDEspacio", "Espacio");

            var localesQry = from l in db.Locales
                             orderby l.Local
                             where (l.Ocupado == false && l.IDEspacio == IDDeptoUser) 
                             select l;
            ViewBag.IDLocal = new SelectList(localesQry.AsNoTracking(), "IDLocal", "Local");
            
            ViewBag.IDTipoOcupacionUso = new SelectList(db.TipoOcupacionUso, "IDTipoOcupacionUso", "OcupacionUso");
            return View();
        }

        // POST: Contratos/Create -- -- -- -- -- -- -- -- --
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDContrato,IDPersona,IDLocal,IDGiro,IDTipoOcupacionUso,NombreComercial,EsContrato,Activo,FechaContrato,FechaInicio,FechaVencim,NombreFiador,DiaFijoPago,NumYearsVigencia,IDUser,Observaciones")] Contratos contratos)
        {
            if (ModelState.IsValid)
            {
                // Relación usuario crea
                contratos.IDUser = (int)Session["ID_User"];
                // Relación Persona
                contratos.IDPersona = (int)TempData["PersonaID"];
                db.Contratos.Add(contratos);
                Locales locales = db.Locales.Find(contratos.IDLocal);                
                if (locales == null)
                {
                    return HttpNotFound();
                }
                locales.Ocupado = true;
                db.Entry(locales).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index","Personas");
            }

            ViewBag.IDGiro = new SelectList(db.Giros, "IDGiro", "Giro", contratos.IDGiro);
            ViewBag.IDLocal = new SelectList(db.Locales, "IDLocal", "Local", contratos.IDLocal);
            ViewBag.IDPersona = new SelectList(db.Personas, "IDPersona", "Nombre", contratos.IDPersona);
            ViewBag.IDTipoOcupacionUso = new SelectList(db.TipoOcupacionUso, "IDTipoOcupacionUso", "OcupacionUso", contratos.IDTipoOcupacionUso);
            return View(contratos);
        }

        // GET: Contratos/Edit
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult Edit(int? id)
        {
            //Editar Giro, tipo de ocupacion, nombre comercial, fechas, observaciones
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contratos contratos = db.Contratos.Find(id);
            if (contratos == null)
            {
                return HttpNotFound();
            }
            Personas persona = db.Personas.Find(contratos.IDPersona);
            if (persona == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDPersona = persona.IDPersona;
            TempData["PersonaID"] = persona.IDPersona;
            ViewBag.nombrePersona = persona.Nombre + " " + persona.APaterno + " " + persona.AMaterno;
            // no edición de Persona del Contrato
            int IDDeptoUser = (int)Session["ID_Area"];
            var girosQry = from g in db.Giros
                           orderby g.Giro
                           select g;
            ViewBag.IDGiro = new SelectList(girosQry.AsNoTracking(), "IDGiro", "Giro");           

            var localesQry = from l in db.Locales
                             orderby l.Local
                             where (l.Ocupado == false && l.IDEspacio == IDDeptoUser)
                             select l;
            ViewBag.IDLocal = new SelectList(localesQry.AsNoTracking(), "IDLocal", "Local");

            ViewBag.IDTipoOcupacionUso = new SelectList(db.TipoOcupacionUso, "IDTipoOcupacionUso", "OcupacionUso");

            return View(contratos);
        }

        // POST: Contratos/Edit/5
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] Contratos contratos)
        {
            if (ModelState.IsValid)
            {
                // Relación usuario edita
                contratos.IDUser = (int)Session["ID_User"];
                // Relación Persona                

                db.Entry(contratos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDGiro = new SelectList(db.Giros, "IDGiro", "Giro", contratos.IDGiro);
            ViewBag.IDLocal = new SelectList(db.Locales, "IDLocal", "Local", contratos.IDLocal);
            ViewBag.IDPersona = new SelectList(db.Personas, "IDPersona", "Nombre", contratos.IDPersona);
            ViewBag.IDTipoOcupacionUso = new SelectList(db.TipoOcupacionUso, "IDTipoOcupacionUso", "OcupacionUso", contratos.IDTipoOcupacionUso);
            return View(contratos);
        }

        // GET: Contratos/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public ActionResult DeleteNo(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contratos contratos = db.Contratos.Find(id);
            if (contratos == null)
            {
                return HttpNotFound();
            }
            return View(contratos);
        }

        [Authorize(Roles = "SuperAdmin")]
        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Contratos contratos = db.Contratos.Find(id);
            db.Contratos.Remove(contratos);
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
