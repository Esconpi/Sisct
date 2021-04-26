using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("estoque")]
    public class Estoque : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        [Display(Name = "Mes")]
        public string MesRef { get; set; }

        [Display(Name = "Ano")]
        public string AnoRef { get; set; }

        [Display(Name = "Quantidade")]
        public decimal Quantity { get; set; }

        [Display(Name = "Valor Unitário")]
        public decimal Value { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }
    }
}
