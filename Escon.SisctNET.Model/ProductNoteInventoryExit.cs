using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("productnoteinventoryexit")]
    public class ProductNoteInventoryExit : EntityBase
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

        [Display(Name = "Nota")]
        [ForeignKey("NoteInventoryExit")]
        public int? NoteInventoryExitId { get; set; }

        private NoteInventoryExit noteInventoryExit;
        public NoteInventoryExit NoteInventoryExit
        {
            get => LazyLoader.Load(this, ref noteInventoryExit);
            set => noteInventoryExit = value;
        }

    }
}
