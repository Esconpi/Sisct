using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("taxationncm")]
    public class TaxationNcm : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Ncm")]
        [ForeignKey("Ncm")]
        public int NcmId { get; set; }

        private Ncm ncm;
        public Ncm Ncm
        {
            get => LazyLoader.Load(this, ref ncm);
            set => ncm = value;
        }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        [Display(Name = "Tributado")]
        public bool Status { get; set; }

        public string Year { get; set; }

        public string Month { get; set; }
    }
}
