using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("person")]
    public class Person : EntityBase
    {

        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        [Display(Name = "Nome")]
        public string FirstName { get; set; }

        [Display(Name = "Sobrenome")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        [Display(Name = "Documento")]
        public string Document { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Formato inválido!")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        [Display(Name = "Senha")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [NotMapped]
        public string PasswordConfirm { get; set; }

        [Display(Name = "Ativo")]
        public bool Active { get; set; }

        [Display(Name = "Perfil")]
        [ForeignKey("Profile")]
        [Required(ErrorMessage = "Obrigatório")]
        public int ProfileId { get; set; }

        private Profile profile;
        public Profile Profile
        {
            get => LazyLoader.Load(this, ref profile);
            set => profile = value;
        }
    }
}