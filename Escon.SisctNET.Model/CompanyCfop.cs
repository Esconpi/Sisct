using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("companyCfop")]
    public class CompanyCfop : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Cfop")]
        [ForeignKey("Cfop")]
        public int CfopId { get; set; }

        private Cfop cfop;
        public Cfop Cfop
        {
            get => LazyLoader.Load(this, ref cfop);
            set => cfop = value;
        }

        [Display(Name = "Ativo")]
        public bool Active { get; set; }

        [Display(Name = "Tipo")]
        [ForeignKey("CfopType")]
        public int ? CfopTypeId { get; set; }

        private CfopType cfopType;

        public CfopType CfopType
        {
            get => LazyLoader.Load(this, ref cfopType);
            set => cfopType = value;
        }

    }
}
