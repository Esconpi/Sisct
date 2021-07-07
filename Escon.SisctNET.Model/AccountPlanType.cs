using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("accountplantype")]
    public class AccountPlanType : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Required]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "Ativo")]
        public bool Active { get; set; }

        [Display(Name = "Grupo do Tipo de Conta")]
        [ForeignKey("AccountPlanTypeGroup")]
        public long AccountPlanTypeGroupId { get; set; }

        private AccountPlanTypeGroup accountPlanTypeGroup;
        public AccountPlanTypeGroup AccountPlanTypeGroup
        {
            get => LazyLoader.Load(this, ref accountPlanTypeGroup);
            set => accountPlanTypeGroup = value;
        }

    }
}
