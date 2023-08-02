using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Escon.SisctNET.Model
{
    [Table("taxrule")]
    public class TaxRule : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Ncm")]
        public string Ncm { get; set; }

        [Display(Name = "Ex.")]
        public string CodeException { get; set; }

        [Display(Name = "Nome")]
        public string NameException { get; set; }

        [Display(Name = "Data Inicial")]
        [DataType(DataType.Date)]
        public DateTime DateStart { get; set; }

        [Display(Name = "Data Final")]
        [DataType(DataType.Date)]
        public DateTime? DateEnd { get; set; }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public long CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }
    }
}
