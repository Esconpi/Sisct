using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("csosn")]
    public class Csosn : EntityBase
    {
        [Display(Name = "Código")]
        public string Code { get; set; }

        [Display(Name = "Nome")]
        public string Name { get; set; }
    }
}
