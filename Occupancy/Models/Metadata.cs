using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Occupancy.Models
{
    public class ContratosMetadata
    {
        [Display(Name = "Num Contrato")]
        public int IDContrato;

        [Display(Name = "Tipo de Ocupación")]
        public int IDTipoOcupacionUso;

        [StringLength(50)]
        [Display(Name = "Nombre Comercial")]
        public string NombreComercial;

        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]

        [Display(Name = "Fecha de Emisión")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public System.DateTime FechaContrato;

        [Display(Name = "Fecha de Inicio")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> FechaInicio;

        [Display(Name = "Fecha de Vencimiento")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> FechaVencim;

        [StringLength(50)]
        [Display(Name = "Nombre Fiador")]
        public string NombreFiador;

        [Display(Name = "Día Tope de Pago")]
        public Nullable<int> DiaFijoPago;

        [Display(Name = "Años de Vigencia")]
        [Range(0, 4)]
        public Nullable<int> NumYearsVigencia;

        [Display(Name = "Usuario Creó")]
        public int IDUser;

    }
    public class DepartamentosMetadata
    {
        [Display(Name = "Departamento")]
        public int IDDepto;

        [Display(Name = "Nombre Jefe")]
        public int NombreJefe;


    }
    public class EspaciosMetadata
    {
        [Display(Name = "Clave Espacio")]
        public int IDEspacio;


        [Display(Name = "Dirección")]
        public string Direccion;


        [Display(Name = "Inmueble - Espacio")]
        public string Espacio;

        [Display(Name = "Número de Locales")]
        public Nullable<int> NumLocales;

        [Display(Name = "C.P.")]        
        public string CP;

    }

    public class GirosMetadata
    {
        [StringLength(50)]
        [Display(Name = "Nombre del Giro")]
        public string Giro;
    }

    public class LocalesMetadata
    {
        [Display(Name = "Folio del Local")]
        public int IDLocal;

        [Display(Name = "Espacio")]
        public int IDEspacio;

        [Display(Name = "Estado")]
        public bool Ocupado;

        [Display(Name = "Nave")]
        public Nullable<int> IDNave;
        
        [Display(Name = "Sección")]
        public Nullable<int> IDSeccion;

        [Display(Name = "Tipo de Cuota")]
        [Required]
        public int IDTipoCuota;

        [Display(Name = "Nombre del Local ")]
        [Required]
        public string Local;

        [Display(Name = "Metros de Frente")]
        [Range(0, 9999.99)]                
        public float MFrente;

        [Display(Name = "Metros de Fondo")]
        [Range(0, 9999.99)]
        public float MFondo;

        [Display(Name = "Metros Cuadrados Totales")]
        public float MCuadTotales;

        [Display(Name = "Num. de Locales para Cobro")]
        public Nullable<int> NumLocParaCobro;

        [Display(Name = "Cuota Base de Renta")]
        [DataType(DataType.Currency)]
        [Range(1, 99999.99, ErrorMessage ="Capture el valor para Cuota Renta")]
        public float ImporteRenta;
                
        [Display(Name = "Aplica Cuota por metraje")]        
        public Nullable<bool> PorMetraje;
    }

    public class MovimientosMetadata
    {
        [Display(Name = "Importe Total")]
        [DataType(DataType.Currency)]
        public float ImporteTotal;

        //[StringLength(50)]
        [Display(Name = "Folio de Recibo")]
        public string FolioRecibo;

        [Display(Name = "Fecha de Emisión")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> FechaEmision;

        [Display(Name = "Fecha de Vencimiento")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> FechaVencimiento;

        [Display(Name = "Fecha de Pago")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> FechaPago;

        [Display(Name = "Num. Orden")]
        public Nullable<int> IDOrden;


        [Display(Name = "Adicional Rezago")]
        public Nullable<float> AdicionalRezago;

        [Display(Name = "Recargo Rezago")]
        public Nullable<float> RecargoRezago;

        [Display(Name = "Año")]
        public int Year;



    }

    public class NavesMetadata
    {
        [Display(Name = "Clave Nave")]
        public int IDNave;
    }

    public class PermisosMetadata
    {
        [Display(Name = "Num Permiso")]
        public int IDPermiso;

        [Display(Name = "Giro Informal")]
        public int IDGiroInformal;

        [Display(Name = "Tipo Medio")]
        public int IDTipoMedioInformal;

        [Display(Name = "Tipo Permiso Informal")]
        public int IDTipoPermisoInformal;

        [StringLength(50)]
        [Display(Name = "Nombre Comercial")]
        public string NombreComercial;


        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]

        [Display(Name = "Fecha de Emisión")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public System.DateTime FechaPermiso;

        [Display(Name = "Fecha de Inicio")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> FechaHrInicio;

        [Display(Name = "Fecha de Vencimiento")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> FechaHrFin;

        [Display(Name = "Día Tope de Pago")]
        public Nullable<int> DiaFijoPago;

        [Display(Name = "Usuario Creó")]
        public int IDUser;

    }
    public class PersonasMetadata
    {
        [Display(Name = "Folio Padrón")]
        public string IDPersona;

        [Display(Name = "Nombre")]
        public string Nombre;

        [StringLength(50)]
        [Display(Name = "Apellido Paterno")]
        public string APaterno;

        [StringLength(50)]
        [Display(Name = "Apellido Materno")]
        public string AMaterno;

        [Display(Name = "Dirección")]
        public string Direccion;

        [Display(Name = "Es Persona Física?")]
        public Nullable<bool> EsPersonaFisica;

        [Display(Name = "Celular")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Capture el número con el formato: 999-999-9999. Ejemplo 272 726 2222")]
        public string Movil;

        [Display(Name = "Teléfono Fijo")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Capture el número con el formato: 999-999-9999. Ejemplo 272 726 2222")]
        public string Phone { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Formato inválido de email.")]
        [DataType(DataType.EmailAddress)]
        //[RegularExpression("@[a-z0-9._%+-]+@[a-z0-9.-]+.[a-z]{2,4}")]
        public string Email;

        //[Display(Name = "Nombre")]
        //[NotMapped]
        //public string NombreCompleto
        //{
        //    set
        //    {
        //        NombreCompleto = Nombre + " " + APaterno + " " + AMaterno;
        //    }
        //}
    }


    public class SeccionesMetadata
    {
        [Display(Name = "Clave Sección")]
        public int IDSeccion;
    }

    public class TipoCuotasMetadata
    {

        [Display(Name = "Espacio")]
        public int IDEspacio;

        [Display(Name = "Nave")]
        public int IDNave;

        [Display(Name = "Nombre del Tipo Cuota")]
        public string TipoCuota;

        [Display(Name = "Cuota Base de Renta")]
        [DataType(DataType.Currency)]
        public float ImporteRenta;

        [Display(Name = "Porcentaje Adicional (%) ")]
        [Range(0, 99.99)]
        public float PorcentajeAdicional;

        [Display(Name = "Factor Adicional (en decimal)")]
        [Range(0, 99.99)]
        public float FactorAdicional;

        [Display(Name = "Factor UMA Diario (en decimal)")]
        [Range(0, 99.99)]
        public float FactorUMADiario;

        [Display(Name = "Factor UMA Mensual (en decimal)")]
        [Range(0, 99.99)]
        public float FactorUMAMensual;

        [Display(Name = "Factor UMA Recargo Diario (en decimal)")]
        [Range(0, 99.99)]
        public float FactorUMARecargoDiario;

        [Display(Name = "Factor UMA Recargo Mensual (en decimal)")]
        [Range(0, 99.99)]
        public float FactorUMARecargoMensual;

        [Display(Name = "Porcentaje Recargo Mensual (%)")]
        [Range(0, 99.99)]
        public float PorcentajeRecargoMensual;

        [Display(Name = "Fecha de Inicio")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> FechaAplicaInicial;

        [Display(Name = "Fecha de Vencimiento")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> FechaAplicaFinal;

        [Display(Name = "¿Aplica por metraje?")]
        public bool PorMetraje;


    }

    public class TipoOcupacionUsoMetadata
    {

        [Display(Name = "Tipo de Ocupación/Uso")]
        public string OcupacionUso;

        [Display(Name = "Generar Contrato")]
        public bool GeneraContrato;
    }

    //public class TipoMovimientoMetadata
    //{
    //    [Display(Name = "Tipo de Movimiento")]
    //    public string TipoMovimiento;

    //    [Display(Name = "Nombre Corto")]
    //    public bool NombreCorto;

    //}

}
