using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("Cst")]
    public class Cst : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "CST")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Identificador")]
        public bool Ident { get; set; }

        [Display(Name = "Tipo")]
        public bool Type { get; set; }
    }
}
