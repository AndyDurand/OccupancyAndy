//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Occupancy.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Secciones
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Secciones()
        {
            this.Locales = new HashSet<Locales>();
        }
    
        public int IDSeccion { get; set; }
        public string Seccion { get; set; }
        public int IDEspacio { get; set; }
    
        public virtual Espacios Espacios { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Locales> Locales { get; set; }
    }
}
