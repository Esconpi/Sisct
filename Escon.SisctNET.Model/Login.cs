using System.ComponentModel.DataAnnotations;

namespace Escon.SisctNET.Model
{
    public class Login
    {
        [Key]
        public long? Id { get; set; }

        [Display(Name = "Email")]
        [Required]
        public string Email { get; set; }

        [Display(Name = "Senha")]
        [Required]
        public string Password { get; set; }

        [Display(Name = "Chave Acesso")]
        [Required]
        public string AccessKey { get; set; }
    }
}
