using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("suspension")]
    public class Suspension : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Suspensão")]
        public DateTime DateStart { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Reativação")]
        public DateTime DateEnd { get; set; }
    }
}
