using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("client")]
    public class Client : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Cliente")]
        public string Name { get; set; }

        [Display(Name = "CNPJ")]
        public string Document { get; set; }

        [Display(Name = "CNPJ Raiz")]
        public string CnpjRaiz { get; set; }

        [Display(Name = "IE")]
        public string Ie { get; set; }

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

        [Display(Name = "Tipo")]
        [ForeignKey("TypeClient")]
        public int ? TypeClientId { get; set; }

        private TypeClient typeClient;
        public TypeClient TypeClient
        {
            get => LazyLoader.Load(this, ref typeClient);
            set => typeClient = value;
        }

        [Display(Name = "Diferido")]
        public bool Diferido { get; set; }

        [Display(Name = "Percentual do Diferimento")]
        public decimal ? Percentual { get; set; }
    }
}
