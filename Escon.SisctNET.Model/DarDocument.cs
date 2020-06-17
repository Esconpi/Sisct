using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("dardocument")]
    public class DarDocument : EntityBase
    {
        public string MessageType { get; set; }

        public string Message { get; set; }

        public string BarCode { get; set; }

        public string DigitableLine { get; set; }

        public int ControlNumber { get; set; }

        public int DocumentNumber { get; set; }
    }
}