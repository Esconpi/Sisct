using System;

namespace Escon.SisctNET.Web.Tax
{
    public class Calculation
    {
        //  Formação de código

        public string Code(string document, string ncm, string uf, string aliquot)
        {
            return document + ncm + uf + aliquot;
        }


        // Cálculos Tributação dos Produtos na Entrada
        
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
            return ((aliqInterna - valorFecop) / 100) * valorAgregado;
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

        public decimal Limite(decimal baseCalc, decimal percentual)
        {
            return (baseCalc * percentual) / 100;
        }

        public decimal Percentual(decimal baseCalc, decimal baseCalcVenda)
        {
            return (baseCalc * 100) / baseCalcVenda;
        }

        public decimal ExcedenteMaximo(decimal baseCalc, decimal limite)
        {
            return baseCalc - limite;
        }

        public decimal ExcedenteMinimo(decimal baseCalc, decimal limite)
        {
            return limite - baseCalc;
        }

        public decimal Imposto(decimal baseCalc, decimal percentual)
        {
            return Math.Round((baseCalc * percentual) / 100, 2);
        }

        public decimal Diferenca(decimal percentual1, decimal percentual2)
        {
            return percentual1 - percentual2;
        }

        public decimal Base1(decimal baseCalc, decimal aliquota)
        {
            return baseCalc * (aliquota / 100);
        }

        public decimal Base2(decimal baseCalc, decimal base1)
        {
            return baseCalc - base1;
        }

        public decimal Base3(decimal base2, decimal aliqInterna)
        {
            return base2 / (1 - (aliqInterna / 100));
        }

        public decimal BaseDifal(decimal base3, decimal aliqInterna)
        {
            return base3 * (aliqInterna / 100);
        }

        public decimal Icms(decimal baseDifal, decimal base1)
        {
            return baseDifal - base1;
        }
    }
}
