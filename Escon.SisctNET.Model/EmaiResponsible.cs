using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("emailresponsible")]
    public class EmailResponsible : EntityBase
    {
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        public string Email { get; set; }

        public virtual Company Company { get; set; }
    }
}