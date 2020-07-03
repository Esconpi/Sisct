using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("section")]
    public class Section : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Seção")]
        public string Name { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Capítulo")]
        [ForeignKey("Chapter")]
        public int ? ChapterId { get; set; }

        private Chapter chapter;
        public Chapter Chapter
        {
            get => LazyLoader.Load(this, ref chapter);
            set => chapter = value;
        }
    }
}
