using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Escon.SisctNET.Model
{
    [Table("internalaliquotconfaz")]
    public class InternalAliquotConfaz : EntityBase
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
        [Display(Name = "Anexo")]
        [ForeignKey("Annex")]
        public long AnnexId { get; set; }

        private Annex annex;
        public Annex Annex
        {
            get => LazyLoader.Load(this, ref annex);
            set => annex = value;
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
