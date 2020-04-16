﻿using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Escon.SisctNET.Model
{
    [Table("productincentivo")]
    public class ProductIncentivo : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Código")]
        public string Code { get; set; }

        [Display(Name = "Ncm")]
        public string Ncm { get; set; }

        [Display(Name = "Descrição")]
        public string Name { get; set; }

        public bool Active { get; set; }

        [Display(Name = "Tributação")]
        public string TypeTaxation { get; set; }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        private Company company;
        public Company Company

        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        public string Month { get; set; }

        public string Year { get; set; }

        public decimal ? Percentual { get; set; }
    }
}
