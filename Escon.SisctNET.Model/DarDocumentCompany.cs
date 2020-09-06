namespace Escon.SisctNET.Model
{
    public class DarDocumentCompany
    {
        public int Id { get; set; }

        public string Document { get; set; }

        public string SocialName { get; set; }


        public int? PeriodReference { get; set; }

        public string DarCode { get; set; }

        public string DarDescription { get; set; }

        public decimal? Value { get; set; }

        public bool? PaidOut { get; set; }
    }
}
