using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Escon.SisctNET.Model
{
    [Table("ncm")]
    public class Ncm : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Ncm")]
        public string Code { get; set; }

        [Display(Name = "Ativa")]
        public bool Active { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Data Inicial")]
        [DataType(DataType.Date)]
        public DateTime? DateStart { get; set; }

        [Display(Name = "Data Inicial")]
        [DataType(DataType.Date)]
        public DateTime? DateStartReal { get; set; }

        [Display(Name = "Data Final")]
        [DataType(DataType.Date)]
        public DateTime? DateEnd { get; set; }

        [Display(Name = "Data Final")]
        [DataType(DataType.Date)]
        public DateTime? DateEndReal { get; set; }

        [Display(Name = "Cofins")]
        public decimal? Cofins { get; set; }

        [Display(Name = "Cofins")]
        public decimal? CofinsReal { get; set; }

        [Display(Name = "Pis")]
        public decimal? Pis { get; set; }

        [Display(Name = "Pis")]
        public decimal? PisReal { get; set; }

        [Display(Name = "Natureza Receita")]
        public string NatReceita { get; set; }

        [Display(Name = "Natureza Receita")]
        public string NatReceitaReal { get; set; }

        [Display(Name = "Cst Entrada")]
        [ForeignKey("Cst")]
        public int? CstEntradaId { get; set; }

        private Cst cstEntrada;
        public Cst CstEntrada
        {
            get => LazyLoader.Load(this, ref cstEntrada);
            set => cstEntrada = value;
        }

        [Display(Name = "Cst Entrada")]
        [ForeignKey("Cst")]
        public int? CstEntradaRealId { get; set; }

        private Cst cstEntradaReal;
        public Cst CstEntradaReal
        {
            get => LazyLoader.Load(this, ref cstEntradaReal);
            set => cstEntradaReal = value;
        }

        [Display(Name = "Cst Saida")]
        [ForeignKey("Cst")]
        public int? CstSaidaId { get; set; }

        private Cst cstSaida;
        public Cst CstSaida
        {
            get => LazyLoader.Load(this, ref cstSaida);
            set => cstSaida = value;
        }

        [Display(Name = "Cst Saida")]
        [ForeignKey("Cst")]
        public int? CstSaidaRealId { get; set; }

        private Cst cstSaidaReal;

        public Cst CstSaidaReal
        {
            get => LazyLoader.Load(this, ref cstSaidaReal);
            set => cstSaidaReal = value;
        }

        public bool Status { get; set; }

        public bool StatusReal { get; set; }

    }
}
