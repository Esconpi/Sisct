using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("ncmconvenio")]
    public class NcmConvenio : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Ncm")]
        public string Ncm { get; set; }

        [Display(Name = "Cest")]
        public string Cest { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data Inicio")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Data Fim")]
        [DataType(DataType.Date)]
        public DateTime? DateEnd { get; set; }

        [Display(Name = "Anexo")]
        [ForeignKey("Annex")]
        public long? AnnexId { get; set; }

        private Annex annex;
        public Annex Annex
        {
            get => LazyLoader.Load(this, ref annex);
            set => annex = value;
        }
    }
}
