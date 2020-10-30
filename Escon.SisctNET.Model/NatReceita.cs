using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("natreceita")]
    public class NatReceita : EntityBase
    {
        [Display(Name = "Código")]
        public string Code { get; set; }

        [Display(Name = "Código AC")]
        public string CodigoAC { get; set; }

        [Display(Name = "CST")]
        public string Cst { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}
