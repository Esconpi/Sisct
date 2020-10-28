using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("base")]
    public class Base : EntityBase
    {
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "Valor")]
        public decimal? Value { get; set; }
    }
}
