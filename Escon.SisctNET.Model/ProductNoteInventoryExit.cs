using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("productnoteinventoryexit")]
    public class ProductNoteInventoryExit : NoteInventoryExit
    {

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

        [Display(Name = "Total")]
        public decimal Vbasecalc { get; set; }

    }
}
