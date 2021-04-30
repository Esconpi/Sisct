using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("access")]
    public class Access : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Funcionalidade")]
        [ForeignKey("Functionality")]
        public Nullable<long> FunctionalityId { get; set; }

        public virtual Functionality Functionality { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Pefil")]
        [ForeignKey("Profile")]
        public Nullable<long> ProfileId { get; set; }

        public virtual Profile Profile { get; set; }

        [Display(Name = "Liberado")]
        public bool Active { get; set; }

    }
}
