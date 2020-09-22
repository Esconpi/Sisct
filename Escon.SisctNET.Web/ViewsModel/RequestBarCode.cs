namespace Escon.SisctNET.Web.ViewsModel
{
    public class RequestBarCode
    {
        public string CpfCnpjIE { get; set; }

        /// <summary>
        /// Formato 123,45
        /// </summary>
        public string ValorTotal { get; set; }

        /// <summary>
        /// Formato yyyyMM
        /// </summary>
        public string PeriodoReferencia { get; set; }

        /// <summary>
        /// Tipo de tributação
        /// </summary>
        public int TypeTaxation { get; set; }

        /// <summary>
        /// Documentos que devem ser gerados
        /// </summary>
        public RecipeCodeValue[] RecipeCodeValues { get; set; }

    }
}