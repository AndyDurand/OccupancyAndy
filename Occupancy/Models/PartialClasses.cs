using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Occupancy.Models
{
    [MetadataType(typeof(ContratosMetadata))]
    public partial class Contratos
    {
    }

    [MetadataType(typeof(DepartamentosMetadata))]
    public partial class Departamentos
    {
    }

    [MetadataType(typeof(EspaciosMetadata))]
    public partial class Espacios
    {

    }
        
    [MetadataType(typeof(GirosMetadata))]
    public partial class Giros
    {

    }

    [MetadataType(typeof(LocalesMetadata))]
    public partial class Locales
    {

    }


    [MetadataType(typeof(MovimientosMetadata))]
    public partial class Movimientos
    {
    }

    [MetadataType(typeof (NavesMetadata)) ]
    public partial class Naves
    {

    }

    [MetadataType(typeof(PermisosMetadata))]
    public partial class Permisos
    {
    }

    [MetadataType(typeof(PersonasMetadata))]
    public partial class Personas
    {
    }


    //SeccionesMetadata
    [MetadataType(typeof (SeccionesMetadata))]
    public partial class Secciones
    {

    }

    [MetadataType(typeof(TipoCuotasMetadata))]
    public partial class TipoCuotas
    {
    }

    [MetadataType(typeof(TipoOcupacionUsoMetadata))]
    public partial class TipoOcupacionUso
    {
    }

    //[MetadataType(typeof(TipoMovimientoMetadata))]
    //public partial class TipoMovimiento
    //{
    //}


}