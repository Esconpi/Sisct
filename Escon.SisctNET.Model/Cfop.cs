using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("cfop")]
    public class Cfop : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }


        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Cfop")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Ativo")]
        public bool Active { get; set; }

        [Display(Name = "Tipo")]
        [ForeignKey("CfopType")]
        public long? CfopTypeId { get; set; }

        private CfopType cfopType;

        public CfopType CfopType
        {
            get => LazyLoader.Load(this, ref cfopType);
            set => cfopType = value;
        }
    }
}
