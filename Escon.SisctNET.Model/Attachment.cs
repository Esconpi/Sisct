using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("attachment")]
    public class Attachment : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}
