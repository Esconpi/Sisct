using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("productnoteinventoryentry")]
    public class ProductNoteInventoryEntry : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Código do Produto")]
        public string Cprod { get; set; }

        [Display(Name = "Produto")]
        public string Xprod { get; set; }

        [Display(Name = "Valor")]
        public decimal Vprod { get; set; }

        [Display(Name = "Quantidade")]
        public decimal? Qcom { get; set; }

        [Display(Name = "Unidade")]
        public string Ucom { get; set; }

        [Display(Name = "Número do Item")]
        public string Nitem { get; set; }

    }
}
