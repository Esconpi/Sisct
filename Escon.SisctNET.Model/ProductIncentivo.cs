using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("productincentivo")]
    public class ProductIncentivo : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        public string Arquivo { get; set; }

        [Display(Name = "Código")]
        public string Code { get; set; }

        [Display(Name = "Ncm")]
        public string Ncm { get; set; }

        [Display(Name = "Cest")]
        public string Cest { get; set; }

        [Display(Name = "Descrição")]
        public string Name { get; set; }

        [Display(Name = "Data Inicial")]
        [DataType(DataType.Date)]
        public DateTime ? DateStart { get; set; }

        [Display(Name = "Data Final")]
        [DataType(DataType.Date)]
        public DateTime ? DateEnd { get; set; }

        public bool Active { get; set; }

        [Display(Name = "Tributação")]
        public string TypeTaxation { get; set; }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        private Company company;
        public Company Company

        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        public string Month { get; set; }

        public string Year { get; set; }

        [Display(Name = "Incentivo %")]
        public decimal ? Percentual { get; set; }

        [Display(Name = "Cst")]
        [ForeignKey("Cst")]
        public int? CstId { get; set; }

        private Cst cst;
        public Cst Cst
        {
            get => LazyLoader.Load(this, ref cst);
            set => cst = value;
        }

        [Display(Name = "BCR")]
        public bool Bcr { get; set; }

        [Display(Name = "BCR %")]
        public decimal? PercentualBcr { get; set; }

        public decimal? PercentualInciso { get; set; }
    }
}
