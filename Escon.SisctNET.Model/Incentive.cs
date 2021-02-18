using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("incentive")]
    public class Incentive : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public Nullable<int> CompanyId { get; set; }

        public virtual Company Company { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Data Inicio")]
        public DateTime DateStart { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Data Fim")]
        public DateTime DateEnd { get; set; }

        [Display(Name = "Ativo")]
        public bool Active { get; set; }

    }
}
