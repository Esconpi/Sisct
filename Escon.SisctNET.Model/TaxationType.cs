using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("taxationtype")]
    public class TaxationType : EntityBase
    {
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Tipo")]
        public string Type { get; set; }

        [Display(Name = "Ativa")]
        public bool Active { get; set; }
    }
}
