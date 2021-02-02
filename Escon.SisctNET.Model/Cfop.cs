using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("cfop")]
    public class Cfop : EntityBase
    {

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Cfop")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}
