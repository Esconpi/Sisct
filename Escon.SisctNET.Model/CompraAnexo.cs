using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{

    [Table("compraanexo")]
    public class CompraAnexo : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [ForeignKey("TaxAnexo")]
        public long TaxAnexoId { get; set; }

        private TaxAnexo taxAnexo;

        public TaxAnexo TaxAnexo
        {
            get => LazyLoader.Load(this, ref taxAnexo);
            set => taxAnexo = value;
        }

        public decimal? Base {get;set ;}

        public decimal? Aliquota { get; set; }

        public decimal? Icms { get; set; }
    }
}
