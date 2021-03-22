
namespace Escon.SisctNET.Web.Taxation
{
    public class Calculation
    {
        // Cálculos Tributação dos produtos na entrada

        public string Code(string document, string ncm, string uf, string aliquot)
        {
            return document + ncm + uf + aliquot;
        }

        public decimal BaseCalc(decimal vProd, decimal vFrete, decimal vSeg, decimal vOutro, decimal vDesc, decimal vIPI, decimal frete_prod)
        {
            return vProd + vFrete + vSeg + vOutro - vDesc + vIPI + frete_prod;
        }

        public decimal BaseCalc(decimal vBaseCalc, decimal vDesc)
        {
            return vBaseCalc + vDesc;
        }

        public decimal ValorIcms(decimal icmsCTe , decimal vIcms)
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

        public decimal ValorAgregadoAliqInt(decimal aliqInterna, decimal valorFecop,decimal valorAgregado)
        {
            return ((aliqInterna-valorFecop) / 100) * valorAgregado;
        }

        public decimal ValorFecop(decimal fecop, decimal valorAgregado)
        {
            return (fecop / 100) * valorAgregado;
        }

        public decimal DiferencialAliq(decimal aliIntena, decimal aliquota)
        {
            return aliIntena - aliquota;
        }

        public decimal IcmsApurado(decimal dif, decimal baseCalc)
        {
            return (dif / 100) * baseCalc;
        }

        public decimal ValorAgregadoPautaProd(decimal baseCalc, decimal quantParaCalc)
        {
            return baseCalc / quantParaCalc;
        }

        public decimal ValorAgregadoPautaAto(decimal qtd, decimal price)
        {
            return qtd * price;
        }

        public decimal TotalIcms(decimal valorAgregadoAliqInt, decimal valorIcms)
        {
            return valorAgregadoAliqInt - valorIcms;
        }

        public decimal TotalIcmsPauta(decimal icmsPauta, decimal valorFecop)
        {
            return icmsPauta + valorFecop;
        }

    }
}
