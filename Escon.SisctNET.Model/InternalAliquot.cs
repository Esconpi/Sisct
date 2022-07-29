using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("internalaliquot")]
    public class InternalAliquot : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Estado")]
        [ForeignKey("State")]
        public long StateId { get; set; }

        private State state;
        public State State
        {
            get => LazyLoader.Load(this, ref state);
            set => state = value;
        }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Aliquota")]
        public decimal Aliquota { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Data Inicial")]
        [DataType(DataType.Date)]
        public DateTime DateStart { get; set; }

        [Display(Name = "Data Final")]
        [DataType(DataType.Date)]
        public DateTime? DateEnd { get; set; }
    }
}
