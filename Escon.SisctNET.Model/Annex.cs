using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("annex")]
    public class Annex : EntityBase
    {

        [Display(Name = "Convênio")]
        public string Convenio { get; set; } 

        [Display(Name = "Data do Convênio")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Descrição do Anexo")]
        public string Description { get; set; }
    }
}
