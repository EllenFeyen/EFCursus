//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EFCursus
{
    using System;
    using System.Collections.Generic;
    
    public partial class Boek
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Boek()
        {
            this.BoekenCursussen = new HashSet<BoekCursus>();
        }
    
        public int BoekNr { get; set; }
        public string ISBNNr { get; set; }
        public string Titel { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BoekCursus> BoekenCursussen { get; set; }
    }
}
