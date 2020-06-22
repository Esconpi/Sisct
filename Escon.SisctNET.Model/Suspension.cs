﻿using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

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

        [Display(Name = "Data Inicio")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Data Fim")]
        public DateTime? DateEnd { get; set; }
    }
}
