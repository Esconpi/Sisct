using System;
using System.ComponentModel.DataAnnotations;

namespace Escon.SisctNET.Model
{
    public class EntityBase
    {
        [Key]
        [Display(Name = "Código")]
        public long Id { get; set; }

        [Display(Name = "Data Criação")]
        public DateTime Created { get; set; }

        [Display(Name = "Data Atualização")]
        public DateTime Updated { get; set; }
    }
}