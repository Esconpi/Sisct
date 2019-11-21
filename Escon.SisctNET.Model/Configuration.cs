using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("configuration")]
    public class Configuration : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        [Display(Name = "Valor")]
        public string Value { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        [Display(Name = "Tipo do Campo")]
        public int DataType { get; set; }

        [Display(Name = "Ativo")]
        public bool Active { get; set; }
    }
}
