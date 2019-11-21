using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("state")]
    public class State : EntityBase
    {
        [Display(Name = "Uf de Origem")]
        public string UfOrigem { get; set; }

        [Display(Name = "Uf de Destino")]
        public string UfDestino { get; set; }

        [Display(Name = "Aliquota de Origem")]
        public decimal Aliquota { get; set; }

        [Display(Name = "Data Inicial")]
        [DataType(DataType.Date)]
        public DateTime DateStart { get; set; }

        [Display(Name = "Data Final")]
        [DataType(DataType.Date)]
        public DateTime? DateEnd { get; set; }

    }
}
