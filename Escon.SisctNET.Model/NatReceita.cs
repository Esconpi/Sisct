using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("natreceita")]
    public class NatReceita : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Código")]
        public string Code { get; set; }

        [Display(Name = "Código AC")]
        public string CodigoAC { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Cst ")]
        [ForeignKey("Cst")]
        public int CstId { get; set; }

        private Cst cst;
        public Cst Cst
        {
            get => LazyLoader.Load(this, ref cst);
            set => cst = value;
        }
    }
}
