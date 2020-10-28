using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("typencm")]
    public class TypeNcm : EntityBase
    {
        public string Name { get; set; }
    }
}
