using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Escon.SisctNET.Model
{
    [Table("ncm")]
    public class Ncm : EntityBase
    {

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Ncm")]
        public string Code { get; set; }

        [Display(Name = "Ativa")]
        public bool Active { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

    }
}
