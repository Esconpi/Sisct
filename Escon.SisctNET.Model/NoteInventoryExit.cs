using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("noteinventoryexit")]
    public class NoteInventoryExit: EntityBase
    {
        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public Nullable<int> CompanyId { get; set; }

        public virtual Company Company { get; set; }

        [Display(Name = "Chave")]
        public string Chave { get; set; }

        [Display(Name = "Nº Nota")]
        public string Nnf { get; set; }

        [Display(Name = "Mes")]
        public string MesRef { get; set; }

        [Display(Name = "Ano")]
        public string AnoRef { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Emissão")]
        public DateTime Dhemi { get; set; }
    }
}
