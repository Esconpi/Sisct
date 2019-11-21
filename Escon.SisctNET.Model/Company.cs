using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("company")]
    public class Company : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Ativa")]
        public bool Active { get; set; }

        [Display(Name = "UF")]
        public bool Status { get; set; }

        [Display(Name = "Incentivada")]
        public bool Incentive { get; set; }

        [Display(Name = "Razão Social")]
        public string SocialName { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Nome Fantasia")]
        public string FantasyName { get; set; }

        [Display(Name = "Cnpj/CPF")]
        public string Document { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Código")]
        public string Code { get; set; }

        [Display(Name = "Matriz")]
        [ForeignKey("CompanyMatrix")]
        public int? CompanyId { get; set; }

        private Company companyMatrix;
        public Company CompanyMatrix
        {
            get => LazyLoader.Load(this, ref companyMatrix);
            set => companyMatrix = value;
        }

    }
}