﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Escon.SisctNET.Model
{
    [Table("Cst")]
    public class Cst : EntityBase
    {
        [Display(Name = "CST")]
        public string code { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Identificador")]
        public bool Ident { get; set; }
    }
}
