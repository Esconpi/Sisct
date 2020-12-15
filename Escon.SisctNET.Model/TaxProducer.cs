using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("taxproducer")]
    public class TaxProducer : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        public string Arquivo { get; set; }

        [Display(Name = "Mes")]
        public string MesRef { get; set; }

        [Display(Name = "Ano")]
        public string AnoRef { get; set; }

        [Display(Name = "Chave")]
        public string Chave { get; set; }

        [Display(Name = "Nº Nota")]
        public string Nnf { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Emissão")]
        public DateTime Dhemi { get; set; }

        [Display(Name = "CNPJ/CPF")]
        public string Cnpj { get; set; }       

        [Display(Name = "Fornecedor")]
        public string Xnome { get; set; }

        [Display(Name = "Total Nota")]
        public decimal Vnf { get; set; }

        [Display(Name = "Base de Cálculo")]
        public decimal Vbasecalc { get; set; }

        public decimal Percentual { get; set; }

        public decimal Icms { get; set; }
    }
}
