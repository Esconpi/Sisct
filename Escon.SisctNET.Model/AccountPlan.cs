using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("accountplan")]
    public class AccountPlan : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Código")]
        public string Code { get; set; }

        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "Reduzida")]
        public int Reduced { get; set; }

        [Display(Name = "Analítica")]
        public bool Analytical { get; set; }

        [Display(Name = "Patrimonial")]
        public bool Patrimonial { get; set; }

        [Display(Name = "Ativa")]
        public bool Active { get; set; }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public long CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        [Display(Name = "Tipo de Conta")]
        [ForeignKey("AccountPlanType")]
        public long AccountPlanTypeId { get; set; }

        private AccountPlanType accountPlanType;
        public AccountPlanType AccountPlanType
        {
            get => LazyLoader.Load(this, ref accountPlanType);
            set => accountPlanType = value;
        }

    }
}
