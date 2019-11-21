using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("access")]
    public class Access : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Funcionalidade")]
        [ForeignKey("Functionality")]
        public int FunctionalityId { get; set; }

        private Functionality functionality;
        public Functionality Functionality
        {
            get => LazyLoader.Load(this, ref functionality);
            set => functionality = value;
        }

        [Required(ErrorMessage = "Obrigatório")]
        [Display(Name = "Perfil")]
        [ForeignKey("Profile")]
        public int ProfileId { get; set; }

        private Profile profile;
        public Profile Profile
        {
            get => LazyLoader.Load(this, ref profile);
            set => profile = value;
        }

        [Display(Name = "Liberado")]
        public bool Active { get; set; }
       
    }
}
