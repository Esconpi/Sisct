using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("chapter")]
    public class Chapter : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Capítulo")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}
