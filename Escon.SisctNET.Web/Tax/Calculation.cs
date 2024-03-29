﻿using System;

namespace Escon.SisctNET.Web.Tax
{
    public class Calculation
    {
        //  Formação de código

        public string Code(string document, string ncm, string uf, string aliquot)
        {
            return document + ncm + uf + aliquot;
        }

        public string CodeP(string document, string document2, string product, string ncm, string uf, string aliquot)
        {
            return document + document2 + product + ncm + uf + aliquot;
        }

        // Cálculos Tributação dos Produtos na Entrada

        public decimal BaseCalc(decimal vProd, decimal vFrete, decimal vSeg, decimal vOutro, decimal vDesc, decimal vIPI, decimal frete_prod)
        {
            return vProd + vFrete + vSeg + vOutro - vDesc + vIPI + frete_prod;
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

        public decimal ValorAgregadoAliqInt(decimal aliqInterna, decimal fecop,decimal valorAgregado)
        {
            return ((aliqInterna - fecop) / 100) * valorAgregado;
        }

        public decimal ValorFecop(decimal fecop, decimal valorAgregado)
        {
            return (fecop / 100) * valorAgregado;
        }

        public decimal DiferencialAliq(decimal aliqIntena, decimal aliquota)
        {
            return aliqIntena - aliquota;
        }

        public decimal IcmsApurado(decimal dif, decimal baseCalc)
        {
            return (dif / 100) * baseCalc;
        }

        public decimal TotalIcms(decimal valorAgregadoAliqInt, decimal valorIcms)
        {
            return valorAgregadoAliqInt - valorIcms;
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

        public decimal BCR(decimal aliqBcr, decimal aliq)
        {
            return aliqBcr / (aliq / 100);
        }

        public decimal IcmsBCR(decimal baseCalcBR, decimal bcr)
        {
            return baseCalcBR * (bcr / 100);
        }

        public decimal IcmsBCRIntra(decimal baseCalcDifal, decimal bcr, decimal aliqInterna)
        {
            return baseCalcDifal * (bcr / 100) * (aliqInterna / 100);
        }
    }
}
