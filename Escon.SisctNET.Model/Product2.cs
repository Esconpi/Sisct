using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("product2")]
    public class Product2 : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Item")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Unidade")]
        public string Unity { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Preço")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [DataType(DataType.Date)]
        [Display(Name = "Data Inicio")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Data Fim")]
        [DataType(DataType.Date)]
        public DateTime? DateEnd { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        [Display(Name = "Grupo")]
        [ForeignKey("Group")]
        public long GroupId { get; set; }

        private Group group;
        public Group Group
        {
            get => LazyLoader.Load(this, ref group);
            set => group = value;
        }
    }
}
