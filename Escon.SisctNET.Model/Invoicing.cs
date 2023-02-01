using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("invoicing")]
    public class Invoicing : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Acima")]
        public decimal? Minimum { get; set; }

        [Display(Name = "Até")]
        public decimal? Maximum { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Empregado")]
        public int Employee { get; set; }
    }
}
