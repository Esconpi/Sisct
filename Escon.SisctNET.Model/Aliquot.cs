using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("aliquot")]
    public class Aliquot : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Uf de Origem")]
        [ForeignKey("State")]
        public long? StateOrigemId { get; set; }

        private State stateOrigem;
        public State StateOrigem
        {
            get => LazyLoader.Load(this, ref stateOrigem);
            set => stateOrigem = value;
        }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Uf de Destino")]
        [ForeignKey("State")]
        public long? StateDestinoId { get; set; }

        private State stateDestino;
        public State StateDestino
        {
            get => LazyLoader.Load(this, ref stateDestino);
            set => stateDestino = value;
        }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Aliquota de Origem")]
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
