﻿using System.ComponentModel.DataAnnotations;

namespace Escon.SisctNET.Model
{
    public enum OccorenceLog
    {
        Login = 1,
        Logout = 2,
        Create = 3,
        Update = 4,
        Delete = 5,
        Read = 6
    }

    public enum ConfigurationDataType
    {
        Int = 1,
        String = 2,
        Boolean = 3,
        Decimal = 4,
        DateTime = 5,
        Date = 6,
        Time = 7
    }

    public enum TypeTaxation
    {
        [Display(Name = "Nenhum")] Nenhum = 0,
        [Display(Name = "Substituição Tributária por MVA")] ST = 1,
        [Display(Name = "Antecipação Parcial")] AP = 2,
        [Display(Name = "Antecipação Total")] AT = 3,
        [Display(Name = "Consumo")] CO = 4,
        [Display(Name = "Consumo para Revenda")] COR = 5,
        [Display(Name = "Imobilizado")] IM = 6,
        [Display(Name = "Isento")] Isento = 7
        
    }

    public enum Type
    {
        [Display(Name = "Produto")] Produto = 1,
        [Display(Name = "Produto Incentivado")] ProdutoI = 2,
        [Display(Name = "Produto Não Incentivado")] ProdutoNI = 3,
        [Display(Name = "Produto Fora da Pauta")] ProdutoFP = 4,
        [Display(Name = "Produto Fora do Incentivo (Exceto ST)")] ProdutoFI = 5,
        [Display(Name = "Nota")] Nota = 6,
        [Display(Name = "Agrupado - Analítico")] AgrupadoA = 7,
        [Display(Name = "Agrupado - Sintético")] AgrupadoS = 8,
        [Display(Name = "Resumo Geral")] Geral = 9,
        [Display(Name = "GNRE de Fornecedor sem Inscrição Estadual")] GNRE = 10,
        [Display(Name = "Notas com Icms ST de Empresas sem IE")] IcmsST = 11
    }

    public enum Opcao
    {
        [Display(Name = "NFe")] NFe = 1,
        [Display(Name = "CTe")] CTe = 2,
        [Display(Name = "Planilha")] Planilha = 3
    }

    public enum Ordem
    {
        [Display(Name = "VERIFICAR NOTAS CANCELADAS SEFAZ")] VerificarSefaz = 1,
        [Display(Name = "VERIFICAR NOTAS CANCELADAS EMPRESA")] VerificarEmpresa = 2,
        [Display(Name = "XML SEFAZ x SPED")] XmlSefaz = 3,
        [Display(Name = "XML EMPRESA x SPED")] XmlEmpresa = 4,
        [Display(Name = "XML SEFAZ x SPED SisCT")] SisCTXS = 5,
        [Display(Name = "XML EMPRESA x SPED SisCT")] SisCTXE = 6,
        [Display(Name = "SPED x XML SEFAZ")] SpedXS = 7,
        [Display(Name = "SPED x XML EMPRESA")] SpedXE = 8,
        [Display(Name = "DIFERENÇA DE TOTAIS")] DifereValor = 9,
        [Display(Name = "EXCEL MALHA x SPED")] Malha = 10
        
    }

    public enum Archive
    {
        [Display(Name = "NFe XML SEFAZ")] XmlNFeSefaz = 1,
        [Display(Name = "NFe XML EMPRESA")] XmlNFeEmpresa = 2,
        [Display(Name = "CTe XML SEFAZ")] XmlCTeSefaz = 3,
        [Display(Name = "CTe XML EMPRESA")] XmlCTeEmpresa = 4,
        [Display(Name = "NFe Sped")] SpedNFe = 5,
        [Display(Name = "CTe Sped")] SpedCTe = 6
    }
}