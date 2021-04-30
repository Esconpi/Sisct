using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("county")]
    public class County : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Código")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Estado")]
        [ForeignKey("State")]
        public long StateId { get; set; }

        private State state;
        public State State
        {
            get => LazyLoader.Load(this, ref state);
            set => state = value;
        }
    }
}
