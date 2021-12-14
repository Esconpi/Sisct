using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("taxsupplement")]
    public class TaxSupplement : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [ForeignKey("TaxAnexo")]
        public long TaxAnexoId { get; set; }

        private TaxAnexo taxAnexo;

        public TaxAnexo TaxAnexo
        {
            get => LazyLoader.Load(this, ref taxAnexo);
            set => taxAnexo = value;
        }

        [Display(Name = "Nº Nota")]
        public string Nnf { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Emissão")]
        public DateTime Dhemi { get; set; }


        [Display(Name = "Base")]
        public decimal? Base { get; set; }

        [Display(Name = "Aliquota")]
        public decimal? Aliquota { get; set; }

        [Display(Name = "Icms")]
        public decimal? Icms { get; set; }
    }
}
