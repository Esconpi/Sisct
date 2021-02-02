using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Escon.SisctNET.Model
{
    [Table("cest")]
    public class Cest : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Cest")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

    }
}
