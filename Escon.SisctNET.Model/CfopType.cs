using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("cfoptype")]
    public class CfopType : EntityBase
    {
        public string Name { get; set; }
    }
}
