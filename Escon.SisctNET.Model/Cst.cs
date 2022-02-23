using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("cst")]
    public class Cst : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "CST")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Identificador")]
        public bool Ident { get; set; }

        [Display(Name = "Tipo")]
        public bool Type { get; set; }

        [Display(Name = "Tipo de Tributação")]
        [ForeignKey("TaxationTypeNcm")]
        [Required(ErrorMessage = "Obrigatório")]
        public long TaxationTypeNcmId { get; set; }

        private TaxationTypeNcm taxationTypeNcm;
        public TaxationTypeNcm TaxationTypeNcm
        {
            get => LazyLoader.Load(this, ref taxationTypeNcm);
            set => taxationTypeNcm = value;
        }
    }
}
