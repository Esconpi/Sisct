using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("cfopType")]
    public class CfopType : EntityBase
    {
        public string Name { get; set; }

        public bool active { get; set; }
    }
}
