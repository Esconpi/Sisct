using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("accountplantypegroup")]
    public class AccountPlanTypeGroup : EntityBase
    {
        [Required]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "Ativo")]
        public bool Active { get; set; }
    }
}
