﻿using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("natreceita")]
    public class NatReceita : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Código")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Código AC")]
        public string CodigoAC { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Cst ")]
        [ForeignKey("Cst")]
        public long CstId { get; set; }

        private Cst cst;
        public Cst Cst
        {
            get => LazyLoader.Load(this, ref cst);
            set => cst = value;
        }
    }
}
