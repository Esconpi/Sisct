using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("typeclient")]
    public class TypeClient : EntityBase
    {
        public string Name { get; set; }
    }
}
