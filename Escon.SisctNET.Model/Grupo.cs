﻿using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("grupo")]
    public class Grupo : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Imposto")]
        [ForeignKey("Tax")]
        public int TaxId { get; set; }

        private Tax tax;

        public Tax Tax
        {
            get => LazyLoader.Load(this, ref tax);
            set => tax = value;
        }

        public string Cnpj { get; set; }

        public string Nome { get; set; }

        public decimal? Vendas { get; set; }

        public decimal? Devolucao { get; set; }

        public decimal? Percentual { get; set; }

        public decimal? PercentualNIncentivo { get; set; }

        public decimal? BaseCalculo { get; set; }

        public decimal? BaseCalculoNIncentivo { get; set; }

        public decimal? Icms { get; set; }

        public decimal? IcmsNIncentivo { get; set; }
    }
}
