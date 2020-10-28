using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("chapter")]
    public class Chapter : EntityBase
    {
        [Display(Name = "Capítulo")]
        public string Name { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}
