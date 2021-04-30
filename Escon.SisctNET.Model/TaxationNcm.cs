using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("taxationncm")]
    public class TaxationNcm : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Ncm")]
        [ForeignKey("Ncm")]
        public long NcmId { get; set; }

        private Ncm ncm;
        public Ncm Ncm
        {
            get => LazyLoader.Load(this, ref ncm);
            set => ncm = value;
        }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public long CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        public string Arquivo { get; set; }

        [Display(Name = "Tributado")]
        public bool Status { get; set; }

        public string Year { get; set; }

        public string Month { get; set; }

        [Display(Name = "Data Inicial")]
        [DataType(DataType.Date)]
        public DateTime? DateStart { get; set; }

        [Display(Name = "Data Final")]
        [DataType(DataType.Date)]
        public DateTime? DateEnd { get; set; }

        [Display(Name = "Cofins")]
        public decimal? Cofins { get; set; }

        [Display(Name = "Pis")]
        public decimal? Pis { get; set; }

        [Display(Name = "Natureza Receita")]
        public string NatReceita { get; set; }

        [Display(Name = "Código do Produto")]
        public string CodeProduct { get;set; }

        [Display(Name = "Cst Entrada")]
        [ForeignKey("Cst")]
        public long? CstEntradaId { get; set; }

        private Cst cstEntrada;
        public Cst CstEntrada
        {
            get => LazyLoader.Load(this, ref cstEntrada);
            set => cstEntrada = value;
        }

        [Display(Name = "Cst Saida")]
        [ForeignKey("Cst")]
        public long? CstSaidaId { get; set; }

        private Cst cstSaida;
        public Cst CstSaida
        {
            get => LazyLoader.Load(this, ref cstSaida);
            set => cstSaida = value;
        }

        public string Type { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Tipo Ncm")]
        [ForeignKey("TypeNcm")]
        public long TypeNcmId { get; set; }

        private TypeNcm typeNcm;
        public TypeNcm TypeNcm
        {
            get => LazyLoader.Load(this, ref typeNcm);
            set => typeNcm = value;
        }

        public string Product { get; set; }
    }
}
