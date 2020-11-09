
namespace Escon.SisctNET.Web.Taxation
{
    public class Calculation
    {
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

        public decimal valorAgregadoPauta(decimal qtd, decimal price)
        {
            return qtd * price;
        }
    }
}
