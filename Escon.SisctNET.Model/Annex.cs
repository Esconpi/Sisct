using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("annex")]
    public class Annex : EntityBase
    {
        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Convênio/Portaria")]
        public string Convenio { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Data do Convênio/Portaria")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}
