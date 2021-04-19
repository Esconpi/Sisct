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

        [Display(Name = "CFOP")]
        public string Cfop { get; set; }

        [Display(Name = "Valor")]
        public decimal Vprod { get; set; }

        [Display(Name = "Quantidade")]
        public decimal? Qcom { get; set; }

        [Display(Name = "Unidade")]
        public string Ucom { get; set; }

        [Display(Name = "Número do Item")]
        public string Nitem { get; set; }

        [Display(Name = "NCM")]
        public string Ncm { get; set; }

        [Display(Name = "CEST")]
        public string Cest { get; set; }

        [Display(Name = "Valor Unitário")]
        public decimal Vuncom { get; set; }

        [Display(Name = "Valor IPI")]
        public decimal Vipi { get; set; }

        [Display(Name = "Valor Frete")]
        public decimal Vfrete { get; set; }

        [Display(Name = "Seguro")]
        public decimal Vseg { get; set; }

        [Display(Name = "Desconto")]
        public decimal Vdesc { get; set; }

        [Display(Name = "Outras Despesas")]
        public decimal Voutro { get; set; }

        [Display(Name = "Valor Frete")]
        public decimal Freterateado { get; set; }

        [Display(Name = "Nota")]
        [ForeignKey("NoteInventoryEntry")]
        public int? NoteInventoryEntryId { get; set; }

        private NoteInventoryEntry noteInventoryEntry;
        public NoteInventoryEntry NoteInventoryEntry
        {
            get => LazyLoader.Load(this, ref noteInventoryEntry);
            set => noteInventoryEntry = value;
        }

    }
}
