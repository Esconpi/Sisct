using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Escon.SisctNET.Model
{
    [Table("dar")]
    public class Dar : EntityBase
    {
        [Display(Name = "Código")]
        public string Code { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Tipo")]
        public string Type { get; set; }


    }
}
