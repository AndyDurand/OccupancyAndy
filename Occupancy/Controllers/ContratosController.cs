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
using C1.Web.Mvc;
using C1.Web.Mvc.Grid;
using C1.Web.Mvc.Serialization;

namespace Occupancy.Controllers
{
    [Authorize]
    public class ContratosController : Controller

    {
        private OccupancyEntities db = new OccupancyEntities(); // no puedo tenerlo en readOnly
        int nYearNow, nMonthNow, nDayNow;
        int[,] arrayMonthD = new int[12, 2];
        string sPeriodNow;
        // -- Constructor con la fecha actual en numero
        public ContratosController()
        {
            nYearNow = System.DateTime.Now.Year;
            nMonthNow = System.DateTime.Now.Month;
            nDayNow = System.DateTime.Now.Day;
            sPeriodNow = "";            
            InitArrayInt(arrayMonthD, 12, 2);
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
            using (Repositorio<Users> obj = new Repositorio<Users>())
            {
                var u = User.Identity.GetUserId();
                var value = obj.Retrive(x => x.IDASPNETUSER == u).IDUser;
                var valueIdDepto = obj.Retrive(x => x.IDASPNETUSER == u).IDDepto;
                Session["ID_User"] = value;
                Session["ID_Area"] = valueIdDepto;                
            }
            int idArea = (int)Session["ID_Area"];

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
        // -- Obtener info general en diccionario de datos  -- -- -- -- -- -- -- -- -- -- -- -- -- -- void GeneralInfo(Contratos contrato)  -- -- --    
        public void GeneralInfo(Contratos contrato)
        {
            Session["ID_Contrato"] = contrato.IDContrato;
            Session["ID_Espacio"] = contrato.Locales.IDEspacio;
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

        // -- Obtener el saldo de la cuenta, general -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- float SaldoAccount(IEnumerable<Movimientos>)  -- -- --    
        public float SaldoAccount(IEnumerable<Movimientos> listMovs)
        {
            // En los movimientos de Abono, debo considerar que tengan Folio de Recibo y Fecha de Pago
            float sumaC, sumaA;
            sumaC = sumaA = 0;
            foreach (var objM in listMovs)
            {
                if (objM.Estatus == "ACTIVO")
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

        // -- Actualizar el saldo de cada movimiento o "documento", de acuerdo a los pagos parciales  -- -- -- public void SaldoAccountReview(Contratos contrato) -- -- -- --    
        public void SaldoAccountReview(Contratos contrato)
        {
            // llamada solo para provisional
            IEnumerable<Movimientos> listMovs = contrato.Movimientos.ToList();
            int idMov = 0;
            float sumaC, sumaA, saldoDoc, sumaAbonosDoc;
            sumaC = sumaA = saldoDoc = sumaAbonosDoc = 0;
            string sPeriodCompare;
            IEnumerable<Movimientos> listMovsPeriodo;
            Movimientos mov;
            // no están ordenados, pero los filtro y comparo en el foreach interno
            listMovs.OrderBy(objM => objM.Periodo).ThenBy(objM => objM.IDTipoMovimiento);

            foreach (var objM in listMovs)
            {
                // por periodo
                sPeriodCompare = objM.Periodo; 
                if (objM.Estatus == "ACTIVO")
                {
                    if (objM.TipoMovimiento.Naturaleza.Contains("CARGO"))
                    {
                        idMov = objM.IDMovimiento;
                        listMovsPeriodo = listMovs.Where( m=> m.Periodo == sPeriodCompare);   
                        if (listMovsPeriodo.Count() > 0)
                        {
                            foreach (var obj in listMovsPeriodo)
                            {   
                                if (obj.TipoMovimiento.Naturaleza.Contains("ABONO") && obj.Pagado == true && obj.FechaPago != null && obj.Periodo == sPeriodCompare ) 
                                {
                                    sumaAbonosDoc += obj.ImporteTotal;
                                }
                            }
                            saldoDoc = objM.ImporteTotal - sumaAbonosDoc;
                            // 
                            if (idMov != 0 && saldoDoc != objM.ImporteTotal )
                            {
                                mov = db.Movimientos.Find(idMov);
                                if (mov != null)
                                {
                                    mov.Saldo = saldoDoc;
                                    db.SaveChanges();

                                }
                            }
                        }                        
                    }
                }                
            }
            return;
        }

        // -- GET EditMovs desde Contratos  -- -- -- -- -- -- -- -- -- -- -- -- -- -- EditMovs(int) GET-- -- -- -- -- -- -- -- -- -- -- -- 
        [Authorize(Roles = "SuperAdmin, AdminConsulta, AdminArea, FuncionarioA")]
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
            // Si no tiene movimientos, ni saldo inicial, debo mostrar la tabla vacía
            if (contrato.Movimientos.Count() > 0     ) 
            {                
                ViewBag.Saldo = SaldoAccount(contrato.Movimientos.ToList());
            }
            else ViewBag.Saldo = 0;

            // Revisar movimientos tipo 3 Renta del Mes. Llenado de listMeses. El POST es con el botón Agregar el Mes en el Modal
            AddMonth(id); 
            // La vista EditMovs recibe un objeto Contrato
            return View(contrato);
        }

        // -- GET MovsDetails desde Contratos, Estado de cuenta, detalle de movimientos  -- -- -- -- -- -- -- MovsDetails(int) GET-- -- -- -- --
        [Authorize(Roles = "SuperAdmin, AdminConsulta, AdminArea, FuncionarioA")]
        // AdminAuditor, AdminConsulta,
        public ActionResult MovsDetails(int? id) 
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
            // Si no tiene movimientos, ni saldo inicial, debo mostrar la tabla vacia
            if (contrato.Movimientos.Count() > 0) 
            {               
                
                SaldoAccountReview(contrato);
                ViewBag.Saldo = SaldoAccount(contrato.Movimientos.ToList());

            }
            else ViewBag.Saldo = 0;
            
            return View(contrato);
        }


        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- Boolean IdentificarMeses(IEnumerable<Movimientos> listMovs, int[,] monthArray)
        public Boolean IdentificarMeses(IEnumerable<Movimientos> listMovs, int[,] monthArray)
        {
            // Recorrer Movimientos, revisar cargos por renta mes , identificar en el array bidimenssional qué meses están cargados
            // [Mes, 0] (1) true or  (0) false
            Boolean existen = false;
            int nMes;
            string sYear, sMonth;          
            foreach (var objM in listMovs)
            {
                if (objM.Estatus == "ACTIVO" && ( objM.IDTipoMovimiento == 3 || objM.IDTipoMovimiento == 2 || objM.IDTipoMovimiento == 1 ))
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
            // -- lo descarto, no puede quedarse con un importe como saldo, se precisan los movimientos
            // IDTipoMovimiento, Importe y Observaciones son los campos leidos            
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
                    Movimientos mov = new Movimientos
                    {
                        IDContrato = contrato.IDContrato
                    };
                        
                        //();                   
                    //mov.IDContrato = contrato.IDContrato;  

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
        private void AddMonth(int? id)
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
                return  y.ToString() + "0" + n.ToString();
            else
                return y.ToString() + n.ToString();
        }

        // --  POST AddMonth  -- -- -- -- -- -- -- -- -- -- -- -- -- -- --AddMonth() POST -- -- -- -- -- -- -- 
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMonth( string obs, string sMes)
        {
            
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
                            // expresado en porcentaje 2.5, sobre el importe total de corriente más el adicional,
                            //fRecargos = (float)((fCorriente * contrato.Locales.TipoCuotas.PorcentajeRecargoMensual / 100));
                            fRecargos = (float)((fCorriente + fAdicional) * contrato.Locales.TipoCuotas.PorcentajeRecargoMensual / 100);
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
                mov.Saldo = mov.ImporteTotal;
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
        private void InitArrayInt(int[,] array, int fil, int col)
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
        
        // -- Calcular MESES de SALDO DEUDOR.- Por referencia los arrays, el numero de meses a agregar
        private int GeneraArrayDebitMonths(ref float[,] array, int numMeses, int col,  int nMesInicia, float porcRecInicial, int nYear, ref string[,] arrayOP )
        {
            // Array de cálculo para meses corrientes, pero van con Recargo
            // [0, 0] Corriente //  [0, 1] Adicional //  [0, 2] Porc Recargo // [0, 3] Recargos // [0, 4] Redondeo // [0, 5] Total importe // [0, 6] Mes // [0, 7] Año
            // Array de cálculo para meses de Rezago, 
            // [0, 0] Rezago //  [0, 1] Adicional //  [0, 2] Porc Recargo // [0, 3] Recargo Rezago //  [0, 4] Redondeo //  [0, 5] Total importe // [0, 6] Mes // [0, 7] Año

            float fTotal, fRedondeo;
            int nTotalParteEntera;
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);

            if (contrato == null)
            {
                return 1;
            }
            float fPorcentRec = (float)contrato.Locales.TipoCuotas.PorcentajeRecargoMensual; // primer valor de porc de recargos, 2.5               

            for (int i = 0; i < numMeses; i++)
            {

                // -- Derechos.                [0, 0] Corriente  o   [0, 0] Rezago 
                if (contrato.Locales.NumLocParaCobro != null || contrato.Locales.NumLocParaCobro != 0)  //contrato.Locales.PorMetraje == true, dato que primero podria validar
                {
                    array[i, 0] = (float)(contrato.Locales.ImporteRenta * contrato.Locales.NumLocParaCobro); // el no. de locales 
                }
                else
                {
                    array[i, 0] = (float)(contrato.Locales.ImporteRenta); // por omisión, 1
                }
                // -- Adicional corriente.     [0, 1] Adicional Corriente  o  [0, 1] Adicional Rezago
                if (contrato.Locales.TipoCuotas.PorcentajeAdicional != null)
                {
                    array[i, 1] = (float)((array[i, 0] * contrato.Locales.TipoCuotas.PorcentajeAdicional) / 100);
                }
                else array[i, 1] = 0;
                // -- Porcentaje Recargos.     [0, 2] Porc Recargo  -- -- -- -- -- -- -- -- --                     
                if (i == 0) // primer mes que se añade, porcRecInicial + fPorcentRec;
                    array[i, 2] = fPorcentRec;
                else   
                    array[i, 2] = array[i-1, 2] + fPorcentRec;

                //                          [0, 3] Recargos  o  [0, 3] Recargo Rezago 
                // -- Recargos: siempre sobre el importe total de corriente más el adicional, va con redondeo
                array[i, 3] = (array[i, 0] + array[i, 1]) * array[i, 2] / 100;
                array[i, 3] = (float)Math.Round(array[i, 3], 2);

                //  -- Total importe: corriente + adic +  recargo
                //                           [0, 4] Redondeo  o  [0, 5] Total importe 
                fTotal = array[i, 0] + array[i, 1] + array[i, 3];
                nTotalParteEntera = (int)fTotal;
                fRedondeo = (float)Math.Round(fTotal - nTotalParteEntera, 2);
                if (fRedondeo > 0.6)
                {
                    nTotalParteEntera++;
                }
                array[i, 4] = fRedondeo;
                array[i, 5] = nTotalParteEntera;
                //
                // -- Año, Mes      // [0, 6] Mes // [0, 7] Año
                array[i, 6] = nMesInicia;  
                array[i, 7] = nYear;

                // aObsPer          // [0, 0] Observaciones //  [0, 1] Periodo   ------------ otro array obs
                arrayOP[i, 0] = "SALDO DEUDOR. MES CON RECARGOS: " + MonthName(nMesInicia) + " " + nYear.ToString();
                arrayOP[i, 1] = PeriodName((int)array[i, 6], (int)array[i, 7]);

                //
                nMesInicia--;
            }
            return 0;
        }
        //

        // --  GET  Agregar  Saldo DEUDOR con número de meses - -- -- -- -- -- -- -- -- -- -- AddMonthsDebitBalance() GET  -- -- -- -- 
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

        // --  POST  Agregar SALDO DEUDOR o SALDO a FAVOR, con número de meses - -- -- -- -- -- -- -- -- -- -- AddMonthsDebitBalance() POST -- -- --
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMonthsDebitBalance(int nMeses, string tipoSaldo)
        {
            // Agregar Saldo inicial deudor.- Movimientos tipo cargo por saldo inicial corriente y saldo inicial rezago
            // tipo salfo "SALDO DEUDOR" o "SALDO A FAVOR"; separo los módulos
            // nMeses.- umero de Meses que adeuda, sea en Corriente o en Rezago; nMeses validado en el input del form, min 1, max 24
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);
            int idC = 0;
            float fPorcRec;
            int numMesesCorriente = 0; 
            int numMesesRezago = 0;
            int numMesesRezagoOtro = 0;
            int nMesInicial = 0;
            int nresul;

            // Array de cálculo para meses corrientes, pero van con Recargo
            // [0, 0] Corriente //  [0, 1] Adicional //  [0, 2] Porc Recargo // [0, 3] Recargos // [0, 4] Redondeo // [0, 5] Total importe // [0, 6] Mes // [0, 7] Año
            int nCol = 8;
            float[,] aRecargosCorriente = new float[12, nCol];
            // Array de cálculo para meses de Rezago, con Recargo
            // [0, 0] Rezago //  [0, 1] Adicional //  [0, 2] Porc Recargo // [0, 3] Recargo Rezago //  [0, 4] Redondeo //  [0, 5] Total importe // [0, 6] Mes // [0, 7] Año
            float[,] aRecargosRezago = new float[12, nCol];
            float[,] aRecargosRezagoOtro = new float[12, nCol];
            // Arrays Obs y Periodos
            // [0, 0] Observaciones //  [0, 1] Periodo
            string[,] aObsPerCorriente = new string[12, 2];
            string[,] aObsPerRezago = new string[12, 2];
            string[,] aObsPerRezagoOtro= new string[12, 2];
            float fPorcentajeR;
           
            //nYearNow     nMonthNow    nDayNow            
            if (contrato == null)
            {
                return HttpNotFound();
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
                
                fPorcentajeR = (float)contrato.Locales.TipoCuotas.PorcentajeRecargoMensual; // primer valor de porc de recargos, 2.5                               
                nMesInicial = nMonthNow - 1;

                // ok, primero el saldo deudor, hacer los cargos de los n meses
                InitArrayDebit(aRecargosCorriente, 12, nCol);
                InitArrayDebit(aRecargosRezago, 12, nCol);
                InitArrayDebit(aRecargosRezagoOtro, 12, nCol);

                InitArrayObsPer(aObsPerCorriente, 12, 2);
                InitArrayObsPer(aObsPerRezago, 12, 2);
                InitArrayObsPer(aObsPerRezagoOtro, 12, 2);
                // -- 1.-  Evaluar el número de Meses a cargar de saldo deudor --
                // -- 2.- Generar los movimientos en los arrays  --------------------------------------------------------------------------------------------------------                
                // 150% son 5 años = 60 meses
                if (nMeses == nMonthNow) // los meses a cargar son todos Corriente, se asumen los meses inmediatos anteriores ( --> no se toma en cuenta el mes actual)
                {
                    numMesesCorriente = nMeses - 1;
                    numMesesRezago = 1;   // si no considero el mes actual, sería un mes de rezago, dic del año anterior

                    // Llenar array de meses deudores de corriente,  array de observaciones y periodos
                    fPorcRec = 0;
                    nresul = GeneraArrayDebitMonths(ref aRecargosCorriente, numMesesCorriente, nCol, nMesInicial, fPorcRec, nYearNow, ref aObsPerCorriente);

                    // Llenar array de meses deudores de rezago, array de obs y periodos
                    // parámetro, en qué acumulado de porcentaje se quedó                    
                    fPorcentajeR = aRecargosCorriente[numMesesCorriente - 1, 2];
                    nresul = GeneraArrayDebitMonths(ref aRecargosRezago, numMesesRezago, nCol, 12, fPorcentajeR, nYearNow - 1, ref aObsPerRezago);

                }
                else if (nMeses < nMonthNow) // son meses Corriente  --
                {
                    numMesesCorriente = nMeses;
                    numMesesRezago = 0;

                    nresul = GeneraArrayDebitMonths(ref aRecargosCorriente, numMesesCorriente, nCol, nMesInicial, fPorcRec, nYearNow, ref aObsPerCorriente);
                }
                else if (nMeses > nMonthNow) // tiene meses de Rezago..  
                {
                    numMesesCorriente = nMonthNow - 1;
                    numMesesRezago = nMeses - numMesesCorriente;
                    if (numMesesRezago <= 12)
                    {
                        numMesesRezago = nMeses - nMonthNow;
                        nresul = GeneraArrayDebitMonths(ref aRecargosCorriente, numMesesCorriente, nCol, nMesInicial, fPorcRec, nYearNow, ref aObsPerCorriente);

                        // Llenar array de meses deudores de rezago, array de obs y periodos
                        // parámetro, en qué acumulado de porcentaje se quedó                    
                        fPorcentajeR = aRecargosCorriente[numMesesCorriente, 2];
                        nresul = GeneraArrayDebitMonths(ref aRecargosRezago, numMesesRezago, nCol, 12, fPorcentajeR, nYearNow - 1, ref aObsPerRezago);
                    }
                    else // más de 12 meses atrasados
                    {
                        numMesesRezagoOtro = numMesesRezago - 12;
                        numMesesRezago = 12;     // año inmediato anterior 

                        if (numMesesCorriente > 0)
                        {
                            fPorcRec = 0; // que inicie el porcentaje de recargos
                            nresul = GeneraArrayDebitMonths(ref aRecargosCorriente, numMesesCorriente, nCol, nMesInicial, fPorcRec, nYearNow, ref aObsPerCorriente);
                            fPorcentajeR = aRecargosCorriente[numMesesCorriente - 1, 2];    // parámetro, en qué acumulado de porcentaje de recargos se quedó                    
                        }
                        else
                        {
                            fPorcRec = fPorcentajeR; // inicia el porcentaje de recargos
                        }
                        // aRecargosRezago
                        // aRecargosRezagoOtro  
                        nresul = GeneraArrayDebitMonths(ref aRecargosRezago, numMesesRezago, nCol, 12, fPorcentajeR, nYearNow - 1, ref aObsPerRezago); // todo el año inm ant
                        fPorcentajeR = aRecargosRezago[numMesesRezago - 1, 2];    // parámetro, en qué acumulado de porcentaje de recargos se quedó                    

                        if (numMesesRezagoOtro > 0)
                        {
                            // Llenar array de meses deudores de rezago Otro, array de obs y periodos   ---  aRecargosRezagoOtro[]                              
                            nresul = GeneraArrayDebitMonths(ref aRecargosRezagoOtro, numMesesRezagoOtro, nCol, 12, fPorcentajeR, nYearNow - 2, ref aObsPerRezagoOtro);
                        }
                    }
                }
                // -- 3.- Recorrer arrays para añadir Movimientos.. 
                if (numMesesCorriente != 0)
                {
                    for (int i = 0; i < numMesesCorriente; i++)
                    {
                        //aRecargosCorriente[] .-2. saldo inicial corriente
                        Movimientos mov = new Movimientos
                        {
                            Estatus = "ACTIVO",
                            IDContrato = contrato.IDContrato,
                            IDTipoMovimiento = 2,
                            FechaEmision = System.DateTime.Now,
                            FechaVencimiento = System.DateTime.Now,
                            IDUser = (int)Session["ID_User"],
                            Corriente = aRecargosCorriente[i, 0],
                            Adicional = aRecargosCorriente[i, 1],
                            Recargos = aRecargosCorriente[i, 3],
                            Redondeo = aRecargosCorriente[i, 4],
                            ImporteTotal = aRecargosCorriente[i, 5],
                            Rezago = 0,  AdicionalRezago = 0, RecargoRezago = 0,
                            Multa = 0,  Honorarios = 0, Ejecucion = 0,
                            Observaciones = aObsPerCorriente[i, 0],
                            Periodo = aObsPerCorriente[i, 1],
                            Pagado = false,
                        };
                        mov.Saldo = mov.ImporteTotal;
                        db.Movimientos.Add(mov);
                    }
                    db.SaveChanges();
                }
                if (numMesesRezago != 0)
                {
                    for (int i = 0; i < numMesesRezago; i++)
                    {
                        //aRecargosRezago[] .-1. saldo inicial rezago
                        Movimientos mov = new Movimientos();
                        mov.Estatus = "ACTIVO";
                        mov.IDContrato = contrato.IDContrato;
                        mov.IDTipoMovimiento = 1;
                        mov.FechaEmision = System.DateTime.Now;
                        mov.FechaVencimiento = System.DateTime.Now;
                        mov.IDUser = (int)Session["ID_User"];
                        mov.Rezago = aRecargosRezago[i, 0];
                        mov.AdicionalRezago = aRecargosRezago[i, 1];
                        mov.RecargoRezago = aRecargosRezago[i, 3];
                        mov.Redondeo = aRecargosRezago[i, 4];
                        mov.ImporteTotal = aRecargosRezago[i, 5];
                        mov.Saldo = mov.ImporteTotal;
                        mov.Corriente = mov.Adicional = mov.Recargos = 0;
                        mov.Multa = mov.Honorarios = mov.Ejecucion = 0;
                        mov.Observaciones = aObsPerRezago[i, 0];
                        mov.Periodo = aObsPerRezago[i, 1];
                        mov.Pagado = false;
                        db.Movimientos.Add(mov);
                    }
                    db.SaveChanges();
                }
                if (numMesesRezagoOtro != 0)
                {
                    for (int i = 0; i < numMesesRezagoOtro; i++)
                    {
                        //aRecargosRezagoOtro[] .-1. saldo inicial rezago
                        Movimientos mov = new Movimientos();
                        mov.Estatus = "ACTIVO";
                        mov.IDContrato = contrato.IDContrato;
                        mov.IDTipoMovimiento = 1;
                        mov.FechaEmision = System.DateTime.Now;
                        mov.FechaVencimiento = System.DateTime.Now;
                        mov.IDUser = (int)Session["ID_User"];
                        mov.Rezago = aRecargosRezagoOtro[i, 0];
                        mov.AdicionalRezago = aRecargosRezagoOtro[i, 1];
                        mov.RecargoRezago = aRecargosRezagoOtro[i, 3];
                        mov.Redondeo = aRecargosRezagoOtro[i, 4];
                        mov.ImporteTotal = aRecargosRezagoOtro[i, 5];
                        mov.Saldo = mov.ImporteTotal;
                        mov.Corriente = mov.Adicional = mov.Recargos = 0;
                        mov.Multa = mov.Honorarios = mov.Ejecucion = 0;
                        mov.Observaciones = aObsPerRezagoOtro[i, 0];
                        mov.Periodo = aObsPerRezagoOtro[i, 1];
                        mov.Pagado = false;
                        db.Movimientos.Add(mov);
                    }
                    db.SaveChanges();
                }           


            }
            return RedirectToAction("EditMovs", "Contratos", new { id = idC });

        }


        // -- Calcular MESES de SALDO a FAVOR.- Por referencia los arrays, el numero de meses a agregar
        private int GeneraArrayAFavorMonths(ref float[,] array, int numMeses, int nMesInicia, int nYear, ref string[,] arrayOP)
        {
            float fTotal, fRedondeo;
            int nTotalParteEntera;
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);

            if (contrato == null)
            {
                return 1;
            }
            float fPorcentRec = (float)contrato.Locales.TipoCuotas.PorcentajeRecargoMensual; // primer valor de porc de recargos, 2.5               

            for (int i = 0; i < numMeses; i++)
            {

                // -- Derechos.                [0, 0] Corriente  o   [0, 0] Rezago 
                if (contrato.Locales.NumLocParaCobro != null || contrato.Locales.NumLocParaCobro != 0)  //contrato.Locales.PorMetraje == true, dato que primero podria validar
                {
                    array[i, 0] = (float)(contrato.Locales.ImporteRenta * contrato.Locales.NumLocParaCobro); // el no. de locales 
                }
                else
                {
                    array[i, 0] = (float)(contrato.Locales.ImporteRenta); // por omisión, 1
                }
                // -- Adicional corriente.     [0, 1] Adicional Corriente  o  [0, 1] Adicional Rezago
                if (contrato.Locales.TipoCuotas.PorcentajeAdicional != null)
                {
                    array[i, 1] = (float)((array[i, 0] * contrato.Locales.TipoCuotas.PorcentajeAdicional) / 100);
                }
                else array[i, 1] = 0;

                //  -- Total importe: corriente + adic +  recargo
                //                           [0, 4] Redondeo  o  [0, 5] Total importe 
                fTotal = array[i, 0] + array[i, 1] + array[i, 3];
                nTotalParteEntera = (int)fTotal;
                fRedondeo = (float)Math.Round(fTotal - nTotalParteEntera, 2);
                if (fRedondeo > 0.6)
                {
                    nTotalParteEntera++;
                }
                array[i, 4] = fRedondeo;
                array[i, 5] = nTotalParteEntera;
                //
                // -- Año, Mes      // [0, 6] Mes // [0, 7] Año
                array[i, 6] = nMesInicia;
                array[i, 7] = nYear;

                // aObsPer          // [0, 0] Observaciones //  [0, 1] Periodo   ------------ otro array obs
                arrayOP[i, 0] = "SALDO A FAVOR. MES PAGADO POR ADELANTADO: " + MonthName(nMesInicia) + " " + nYear.ToString();
                arrayOP[i, 1] = PeriodName((int)array[i, 6], (int)array[i, 7]);

                // son meses hacia adelante
                nMesInicia++;
            }
            return 0;
        }

        // AddMonthsAFavorBalance
        // --  GET  Agregar  Saldo DEUDOR con número de meses - -- -- -- -- -- -- -- -- -- -- AddMonthsAFavorBalance() GET  -- -- -- -- 
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult AddMonthsAFavorBalance(int? id)
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

        // --  POST  Agregar SALDO DEUDOR o SALDO a FAVOR, con número de meses - -- -- -- -- -- -- -- -- -- -- AddMonthsAFavorBalance() POST  -- -- --
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMonthsAFavorBalance(int nMeses, string tipoSaldo)
        {
            // Agregar Saldo inicial a favor.- Movimientos tipo cargo por renta mes, meses que pagaron por adelantado.
            // Numero de Meses que se agregarán en Corriente; nMeses validado en el input del form, min 1,  max 11, por ahora 

            int idC = 0;
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);
            int numMesesCorriente = 0;

            // Array de cálculo para meses corrientes, pero van con Recargo
            // [0, 0] Corriente //  [0, 1] Adicional //  [0, 2] Porc Recargo // [0, 3] Recargos // [0, 4] Redondeo // [0, 5] Total importe // [0, 6] Mes // [0, 7] Año
            int nCol = 8;
            float[,] aAFavorCorriente = new float[12, nCol];
            string[,] aObsPerCorriente = new string[12, 2]; 
            int nMesInicial;
            int nresul;

            if (contrato == null)
            {
                return HttpNotFound();
            }         

            if ((ModelState.IsValid))
            {
                idC = contrato.IDContrato;
                nMesInicial = nMonthNow + 1; // mes hacia adelante             
                // agrego los movimientos de cargo y de abono, de los meses a favor 
                // un mov como tipo 3 y como tipo 4, el tipo 3 pagado true
                InitArrayDebit(aAFavorCorriente, 12, nCol);
                InitArrayObsPer(aObsPerCorriente, 12, 2);

                //private int GeneraArrayAFavorMonths(ref float[,] array, int numMeses, int nMesInicia, int nYear, ref string[,] arrayOP)

                // -- 1.-  Evaluar el número de Meses a cargar de saldo deudor --
                // -- 2.- Generar los movimientos en los arrays  ------------------------------------------------------------------------
                int numMesesRestan = 12 - nMonthNow; //  

                if (nMeses >=  numMesesRestan  && numMesesRestan != 0) 
                {
                    numMesesCorriente = numMesesRestan;
                }
                else if (nMeses < numMesesRestan )
                {
                    numMesesCorriente = nMeses;

                }
                nresul = GeneraArrayAFavorMonths(ref aAFavorCorriente, numMesesCorriente, nMesInicial, nYearNow, ref aObsPerCorriente);

                // -- 3.- Recorrer arrays para añadir Movimientos.. 
                if (numMesesCorriente != 0)
                {
                    for (int i = 0; i < numMesesCorriente; i++)
                    {
                        //aRecargosCorriente[]
                        // Tipo movimiento 3, cargo renta mes
                        Movimientos mov = new Movimientos();
                        mov.Estatus = "ACTIVO";
                        mov.IDContrato = contrato.IDContrato;
                        mov.IDTipoMovimiento = 3;
                        mov.FechaEmision = System.DateTime.Now;
                        mov.FechaVencimiento = System.DateTime.Now;
                        mov.FechaPago = System.DateTime.Now;
                        mov.Pagado = true;
                        mov.IDUser = (int)Session["ID_User"];
                        mov.Corriente = aAFavorCorriente[i, 0];
                        mov.Adicional = aAFavorCorriente[i, 1];
                        mov.Recargos = aAFavorCorriente[i, 3];
                        mov.Redondeo = aAFavorCorriente[i, 4];
                        mov.ImporteTotal = aAFavorCorriente[i, 5];
                        mov.Saldo = 0;
                        mov.Recargos = mov.Rezago = mov.AdicionalRezago = mov.RecargoRezago = 0;
                        mov.Multa = mov.Honorarios = mov.Ejecucion = 0;
                        mov.Observaciones = aObsPerCorriente[i, 0];
                        mov.Periodo = aObsPerCorriente[i, 1];
                        db.Movimientos.Add(mov);
                        //4
                        mov = new Movimientos();
                        mov.Estatus = "ACTIVO";
                        mov.IDContrato = contrato.IDContrato;
                        mov.IDTipoMovimiento = 4;
                        mov.FechaEmision = System.DateTime.Now;
                        mov.FechaVencimiento = null;
                        mov.FechaPago = System.DateTime.Now;
                        mov.Pagado = true;
                        mov.IDUser = (int)Session["ID_User"];
                        mov.Corriente = aAFavorCorriente[i, 0];
                        mov.Adicional = aAFavorCorriente[i, 1];
                        mov.Recargos = aAFavorCorriente[i, 3];
                        mov.Redondeo = aAFavorCorriente[i, 4];
                        mov.ImporteTotal = aAFavorCorriente[i, 5];
                        mov.Saldo = 0;
                        mov.Recargos = mov.Rezago = mov.AdicionalRezago = mov.RecargoRezago = 0;
                        mov.Multa = mov.Honorarios = mov.Ejecucion = 0;
                        mov.Observaciones = "SALDO A FAVOR. MES PAGADO POR ADELANTADO. " + MonthName((int)aAFavorCorriente[i, 6]) + " " + ((int)aAFavorCorriente[i, 7]).ToString();
                        mov.Periodo = aObsPerCorriente[i, 1];
                        db.Movimientos.Add(mov);
                    }
                    db.SaveChanges();
                }                            
            }
            return RedirectToAction("EditMovs", "Contratos", new { id = idC });

        }

        // -- GET EditMovs desde Contratos  -- -- -- -- -- -- -- -- -- -- -- -- -- -- OPartialFull(int) GET-- -- -- -- -- -- -- -- -- -- -- -- 
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult OPartialFull(int? id)
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
            // Si no tiene movimientos, ni saldo inicial, debo mostrar la tabla vacía
            if (contrato.Movimientos.Count() > 0)
            {
                ViewBag.Saldo = SaldoAccount(contrato.Movimientos.ToList());
            }
            else ViewBag.Saldo = 0;

            // Revisar movimientos tipo 3 Renta del Mes. Llenado de listMeses. El POST es con el botón Agregar el Mes en el Modal
            //AddMonth(id);

            // Revisar movimientos documentos por cobrar, y el habilitar el botón de abonos en la vista solo si "el tipo de cuota es por Local"
            // -------    ----------------  LlenaListDebeImporte(id);  // llenado con los meses que debe, con importes
            // -------    ----------------  LlenaListDebeSaldo(id);  /// si es tipo cuota por local puede abonar
            // int [,] arrayMonthDebe =  new int[12, 2];                
            LlenaListDebeImporte(id);

            // ------para la prueba del controlFlexGrid,
            //IEnumerable<Movimientos> listMovs = contrato.Movimientos.ToList().Where(m => m.Pagado == false);

            // && m => m.IDTipoMovimiento == 3  ||  1 || 2
            ///ViewBag.Docs = contrato.Movimientos.ToList().Where(m =>  m.Pagado == false );

            // La vista EditMovs recibe un objeto Contrato
            return View(contrato);
            //  ahora recibo <Movimientos>
            //return View(contrato.Movimientos); 
            //return View();
            //return View(listMovs);
        }

        //-- Revisar movimientos documentos por cobrar
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        private void LlenaListDebeImporte(int? id)
        {
            
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contratos contratos = db.Contratos.Find(id);            
            
            if (contratos.Movimientos.Count() > 0)  
            {
                // Recorrer movimientos para tomar los meses con saldo, 
                IEnumerable<Movimientos> listMovimientos = contratos.Movimientos.ToList();                
                //InitArrayInt(arrayMonthD, 12, 2);
                //
                string[,] arraySMonthCorr = new string[12, 2];
                InitArrayObsPer(arraySMonthCorr, 12, 2);

                // el mes?
                //int[,] arrayMonthC = new int[12, 2];
                //InitArrayInt(arrayMonthC, 12, 2);

                // todos los datos?
                //int nCol = 8;
                //float[,] aMovDeudorCorriente = new float[12, nCol];
                //float[,] aMovDeudorRezago = new float[12, nCol];
                //// [0, 0] Corriente //  [0, 1] Adicional //  [0, 2] Porc Recargo // [0, 3] Recargos // [0, 4] Redondeo // [0, 5] Total importe // [0, 6] Mes // [0, 7] Año                                
                //// [0, 0] Rezago //  [0, 1] Adicional //  [0, 2] Porc Recargo // [0, 3] Recargo Rezago //  [0, 4] Redondeo //  [0, 5] Total importe // [0, 6] Mes // [0, 7] Año

                //InitArrayDebit(aMovDeudorCorriente, 12, nCol);
                //InitArrayDebit(aMovDeudorRezago, 12, nCol);

                ///-- aqui 
                Boolean existen = false;
                int nMes, i = 0;
                string sYear, sMonth;
                foreach (var objM in listMovimientos)
                {
                    if (objM.Estatus == "ACTIVO" && (objM.IDTipoMovimiento == 3 || objM.IDTipoMovimiento == 2 ) && objM.Pagado == false)
                    {
                        //objM.Periodo.Substring(5, 2).to

                        //aMovDeudorCorriente[i, 0] = (float) objM.Corriente;
                        //aMovDeudorCorriente[i, 1] = (float)objM.Adicional;
                        //aMovDeudorCorriente[i, 3] = (float)objM.Recargos; 
                        //aMovDeudorCorriente[i, 4] = (float)objM.Redondeo;
                        //aMovDeudorCorriente[i, 5] = (float)objM.ImporteTotal;
                        //aMovDeudorCorriente[i, 6] = (float)objM.Periodo.Substring(5, 2);
                        //aMovDeudorCorriente[i, 7] = (float)objM.Periodo.Substring(1,4);

                        // revisar Periodo de ese registro                        
                        sYear = objM.Periodo.Substring(0, 4);
                        sMonth = objM.Periodo.Substring(4, 2);
                        nMes = int.Parse(sMonth);

                        if (int.Parse(sYear) == nYearNow)  // corriente
                        {
                            //arraySMonthCorr[nMes - 1, 0] = objM.Periodo;  // en la posicion del mes
                            arrayMonthD[nMes - 1, 0] = 1;
                            arrayMonthD[nMes - 1, 1] = objM.IDMovimiento; 
                            //arrayMonth[nMes - 1, 0] = 1; // en col 0:  marcar el mes [n-1, 0]
                            //arrayMonth[nMes - 1, 1] = objM.ImporteTotal;
                            existen = true;
                        }
                        //}
                    }
                }
                ViewBag.Year = nYearNow;
                // List con los meses que debe
                List<SelectListItem> listMonthDebe = new List<SelectListItem>();
                SelectListItem itemMonth;
                
                // if (arraySMonthCorr[n, 0] !=  "") 
                if (existen)
                {
                    for (int n = 0; n < 12; n++)
                    {
                        switch (n)
                        {
                            case 0:
                                if (arrayMonthD[n, 0] == 1) 
                                {
                                    itemMonth = new SelectListItem { Text = "ENERO", Value = "01", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }                                
                                break;
                            case 1:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "FEBRERO", Value = "02", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                            case 2:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "MARZO", Value = "03", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                            case 3:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "ABRIL", Value = "04", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                            case 4:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "MAYO", Value = "05", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                            case 5:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "JUNIO", Value = "06", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                            case 6:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "JULIO", Value = "07", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                            case 7:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "AGOSTO", Value = "08", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                            case 8:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "SEPTIEMBRE", Value = "09", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                            case 9:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "OCTUBRE", Value = "10", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                            case 10:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "NOVIEMBRE", Value = "11", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                            case 11:
                                if (arrayMonthD[n, 0] == 1)
                                {
                                    itemMonth = new SelectListItem { Text = "DICIEMBRE", Value = "12", Disabled = false };
                                    listMonthDebe.Add(itemMonth);
                                }
                                break;
                        }
                    }

                    ViewBag.sMes = 1;   
                    ViewBag.listMesesDebe = new SelectList(listMonthDebe, "Value", "Text", ViewBag.sMes);
                    /// 

                }
            }
        }

        // -------------
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        public ActionResult PayOrder(int? id)
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

            //FlexGridBase<Movimientos> model = contrato.Movimientos.Select()
            //model


            ViewBag.Pendientes = contrato.Movimientos.ToList().Where(m => m.Pagado == false);

            return View();
        }


        //-- GET -- -- -- -- -- -- -- POST -- -- -- -- -- -- -- -- -- -- -- -- -- -- AddFullOrder()-- -- -- -- -- -- -- 
        //[Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        //public ActionResult AddFullOrden(int id)
        //{
        //    //int id = int.Parse(TempData["idmov"].ToString());            
        //    //TempData.Keep("idmov");

        //    // el espacio, para filtrar en tbl CruceOrden

        //    Movimientos movimientos = db.Movimientos.Find(id);
        //    if (movimientos == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        ViewBag.tipoMovim = movimientos.TipoMovimiento;
        //        ViewBag.totalMovim = movimientos.ImporteTotal;



        //    }
        //    return View();
        //}
        // --  POST Generar ---->  orden de Pago importe completo para      un Cargo listado en los Movimientos --POST -- --AddFullOrder() -- -- -- -- -- 
        [Authorize(Roles = "SuperAdmin, AdminArea, FuncionarioA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFullOrden(string obs, string sMes)
        {
            Contratos contrato = db.Contratos.Find(Session["ID_Contrato"]);
            int idC = 0, idArb, idEsp;
            int idM;
            CruceOrden cruce;
            Espacios espacio;
            string periodo = nYearNow.ToString();
            Ordenes orden = new Ordenes();

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

                    int nMes = int.Parse(sMes); //el Mes, parámetro del SelectList

                    cruce = db.CruceOrden.Find(Session["ID_Espacio"]); // varios registris, Espacio y Id tipos de movimientos
                    idM = arrayMonthD[nMes - 1, 1];

                    if (cruce == null)
                    {
                        return HttpNotFound();
                    }
                    orden.IDUser = 1;
                    orden.ImporteTotal = 0;
                    orden.FechaEmision = System.DateTime.Now;
                    //ViewBag.listMesesDebe


                    /////////////////idArb = cruce.
                    // ahora todos son de corriente, los pendientes de pago, pero 

                    //int nMes = int.Parse(sMes); //el Mes, parámetro del SelectList

                    //float fTotal, fRedondeo, fCorriente, fAdicional, fRecargos, fRezago, fAdicionalRezago, fRecargoRezago;
                    //fTotal = fRedondeo = fCorriente = fAdicional = fRecargos = fRezago = fAdicionalRezago = fRecargoRezago = 0;
                    //int nTotal = 0;
                    //int nTipoMov = 3;
                    //DateTime dVencim = System.DateTime.Now;
                    // a Movimientos y a Ordenes
                }

            }
            return RedirectToAction("OPartialFull", "Contratos", new { id = idC });
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

        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
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
