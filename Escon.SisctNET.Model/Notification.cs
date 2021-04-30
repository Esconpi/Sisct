using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("notification")]
    public class Notification : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public long CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        [Display(Name = "Mês")]
        public string MesRef { get; set; }

        [Display(Name = "Ano")]
        public string AnoRef { get; set; }

        [Display(Name = "Percentual")]
        public decimal Percentual { get; set; }

        [Display(Name = "Descriçâo")]
        public string Description { get; set; }
    }
}
