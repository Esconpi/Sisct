using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("taxationtypencm")]
    public class TaxationTypeNcm : EntityBase
    {
        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}
