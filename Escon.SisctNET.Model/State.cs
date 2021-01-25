using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("state")]
    public class State : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Código")]
        public int Code { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "UF")]
        public string UF { get; set; }
    }
}
