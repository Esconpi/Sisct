using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("countingType")]
    public class CountingType : EntityBase
    {
        [Display(Name = "Nome")]
        public string Name { get; set; }
    }
}
