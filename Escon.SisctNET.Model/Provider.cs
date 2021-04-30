using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("provider")]
    public class Provider : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Fornecedor")]
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
        public long CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        [Display(Name = "Tipo")]
        [ForeignKey("TypeClient")]
        public long? TypeClientId { get; set; }

        private TypeClient typeClient;
        public TypeClient TypeClient
        {
            get => LazyLoader.Load(this, ref typeClient);
            set => typeClient = value;
        }

        [Display(Name = "Diferido")]
        public bool Diferido { get; set; }

        [Display(Name = "Percentual do Diferimento")]
        public decimal? Percentual { get; set; }

        public string MesRef { get; set; }

        public string AnoRef { get; set; }
    }
}
