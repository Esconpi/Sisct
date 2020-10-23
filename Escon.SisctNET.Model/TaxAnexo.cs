using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("taxanexo")]
    public class TaxAnexo : EntityBase
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

        [Display(Name = "Mes")]
        public string MesRef { get; set; }

        [Display(Name = "Ano")]
        public string AnoRef { get; set; }


        //  ENTRADAS
        public decimal? BaseCompra4 { get; set; }

        public decimal? IcmsCompra4 { get; set; }

        public decimal? BaseCompra7 { get; set; }

        public decimal? IcmsCompra7 { get; set; }

        public decimal? BaseCompra12 { get; set; }

        public decimal? IcmsCompra12 { get; set; }        

        public decimal? BaseDevoFornecedor4 { get; set; }

        public decimal? IcmsDevoFornecedor4 { get; set; }

        public decimal? BaseDevoFornecedor7 { get; set; }

        public decimal? IcmsDevoFornecedor7 { get; set; }

        public decimal? BaseDevoFornecedor12 { get; set; }

        public decimal? IcmsDevoFornecedor12 { get; set; }


        //  SAÍDAS
        public decimal? BaseVenda4 { get; set; }

        public decimal? IcmsVenda4 { get; set; }

        public decimal? BaseVenda7 { get; set; }

        public decimal? IcmsVenda7 { get; set; }

        public decimal? BaseVenda12 { get; set; }

        public decimal? IcmsVenda12 { get; set; }

        public decimal? BaseDevoCliente4 { get; set; }

        public decimal? IcmsDevoCliente4 { get; set; }

        public decimal? BaseDevoCliente12 { get; set; }

        public decimal? IcmsDevoCliente12 { get; set; }
    }
}
