using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("base")]
    public class Base : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Valor")]
        public decimal? Value { get; set; }
    }
}
