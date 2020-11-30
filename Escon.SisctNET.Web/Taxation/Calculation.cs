
namespace Escon.SisctNET.Web.Taxation
{
    public class Calculation
    {
        // Cálculos Tributação dos produtos na entrada

        public decimal baseCalc(decimal vBaseCalc, decimal vDesc)
        {
            return vBaseCalc + vDesc;
        }

        public decimal valorIcms(decimal icmsCTe , decimal vIcms)
        {
            return icmsCTe + vIcms;
        }

        public decimal ValorAgregadoMva(decimal baseCalc, decimal mva)
        {
            return (baseCalc * mva / 100) + baseCalc;
        }

        public decimal ValorAgregadoBcr(decimal bcr, decimal valorAgregado)
        {
            return (bcr / 100) * valorAgregado ;
        }

        public decimal valorAgregadoAliqInt(decimal aliqInterna, decimal valorFecop,decimal valorAgregado)
        {
            return ((aliqInterna-valorFecop) / 100) * valorAgregado;
        }

        public decimal valorFecop(decimal fecop, decimal valorAgregado)
        {
            return (fecop / 100) * valorAgregado;
        }

        public decimal diferencialAliq(decimal aliIntena, decimal aliquota)
        {
            return aliIntena - aliquota;
        }

        public decimal icmsApurado(decimal dif, decimal baseCalc)
        {
            return (dif / 100) * baseCalc;
        }

        public decimal valorAgregadoPautaProd(decimal baseCalc, decimal quantParaCalc)
        {
            return baseCalc / quantParaCalc;
        }

        public decimal valorAgregadoPautaAto(decimal qtd, decimal price)
        {
            return qtd * price;
        }

        public decimal totalIcms(decimal valorAgregadoAliqInt, decimal valorIcms)
        {
            return valorAgregadoAliqInt - valorIcms;
        }

        public decimal totalIcmsPauta(decimal icmsPauta, decimal valorFecop)
        {
            return icmsPauta + valorFecop;
        }

    }
}
