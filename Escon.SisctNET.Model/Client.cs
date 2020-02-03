using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    public class Client : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Fornecedor")]
        public string Name { get; set; }

        [Display(Name = "CNPJ")]
        public string Document { get; set; }

        [Display(Name = "Contribuinte")]
        public bool Taxpayer { get; set; }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        [Required(ErrorMessage = "Obrigatório")]
        public int CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }
    }
}
