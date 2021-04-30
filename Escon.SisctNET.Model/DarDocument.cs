using System;
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

        public long DocumentNumber { get; set; }

        public bool Canceled { get; set; }

        [ForeignKey("Dar")]
        public long DarId { get; set; }

        public virtual Dar Dar { get; set; }

        [ForeignKey("Company")]
        public long CompanyId { get; set; }

        public virtual Company Company { get; set; }

        public int PeriodReference { get; set; }

        public DateTime DueDate { get; set; }

        public bool PaidOut { get; set; }

        public string BilletPath { get; set; }

        public decimal Value { get; set; }
    }
}