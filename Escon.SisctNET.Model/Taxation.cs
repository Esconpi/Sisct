﻿using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("taxation")]
    public class Taxation : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        public string Code { get; set; }

        [Display(Name = "CEST")]
        public string Cest { get; set; }

        [Display(Name = "Aliq. Inter")]
        public decimal? AliqInterna { get; set; }

        [Display(Name = "MVA")]
        public decimal? MVA { get; set; }

        [Display(Name = "BCR")]
        public decimal? BCR { get; set; }

        [Display(Name = "Aliq. Fecop")]
        public decimal? Fecop { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data Inicio")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Data Fim")]
        [DataType(DataType.Date)]
        public DateTime? DateEnd { get; set; }

        [Display(Name = "Aliquota")]
        public decimal? Picms { get; set; }

        [Display(Name = "Uf")]
        public string Uf { get; set; }

        public decimal? PercentualInciso { get; set; }

        public bool EBcr { get; set; }

        [Display(Name = "Tipo de Tributação")]
        [ForeignKey("TaxationType")]
        public long TaxationTypeId { get; set; }

        private TaxationType taxationType;
        public TaxationType TaxationType
        {
            get => LazyLoader.Load(this, ref taxationType);
            set => taxationType = value;
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

        [Display(Name = "NCM")]
        [ForeignKey("Ncm")]
        public long NcmId { get; set; }

        private Ncm ncm;
        public Ncm Ncm

        {
            get => LazyLoader.Load(this, ref ncm);
            set => ncm = value;
        }
    }
}
