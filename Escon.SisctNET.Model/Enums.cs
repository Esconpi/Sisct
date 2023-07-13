using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Nenhum")] Nenhum,
        [Display(Name = "Substituição Tributária por MVA")] ST,
        [Display(Name = "Antecipação Parcial")] AP,
        [Display(Name = "Antecipação Total")] AT,
        [Display(Name = "Consumo")] CO,
        [Display(Name = "Consumo para Revenda")] COR,
        [Display(Name = "Imobilizado")] IM,
        [Display(Name = "Isento")] Isento,
        [Display(Name = "Não Tributado")] NT

    }

    public enum Type
    {
        [Display(Name = "Produto")] Produto,
        [Display(Name = "Produto Incentivado")] ProdutoI,
        [Display(Name = "Produto Não Incentivado")] ProdutoNI,
        [Display(Name = "Produto Dentro da Pauta")] ProdutoP,
        [Display(Name = "Produto Fora da Pauta")] ProdutoFP,
        [Display(Name = "Produto Fora do Incentivo")] ProdutoFI,
        [Display(Name = "Nota")] Nota,
        [Display(Name = "Nota Incentivada")] NotaI,
        [Display(Name = "Nota Não Incentivada")] NotaNI,
        [Display(Name = "Agrupado Analítico")] AgrupadoA,
        [Display(Name = "Agrupado Sintético")] AgrupadoS,
        [Display(Name = "GNRE de Fornecedor sem Inscrição Estadual")] GNRE,
        [Display(Name = "Notas com Icms ST de Empresas sem IE")] IcmsST,
        [Display(Name = "Icms Produtor Rural")] IcmsProdutor,
        [Display(Name = "Apuração Regime Especial Bebidas Alcoólicas")] RegimeBA,
        [Display(Name = "Regime Especial Bebidas Alcoólicas")] RegimeBA2,
        [Display(Name = "Resumo Geral")] Geral
    }

    public enum Opcao
    {
        [Display(Name = "NFe")] NFe,
        [Display(Name = "CTe")] CTe,
        [Display(Name = "Planilha")] Planilha
    }

    public enum Ordem
    {
        [Display(Name = "XML SEFAZ x SPED")] XmlSefaz,
        [Display(Name = "XML SEFAZ x EXCEL FSIST")] SefazXFsist,
        [Display(Name = "XML SEFAZ x SPED SisCT")] SisCTXS,
        [Display(Name = "XML EMPRESA x SPED")] XmlEmpresa,
        [Display(Name = "XML EMPRESA x EXCEL FSIST")] EmpresaXFsist,
        [Display(Name = "XML EMPRESA x SPED SisCT")] SisCTXE,
        [Display(Name = "SPED x XML SEFAZ")] SpedXS,
        [Display(Name = "SPED x XML EMPRESA")] SpedXE,
        [Display(Name = "EXCEL FSIST x XML SEFAZ")] FsistXSefaz,
        [Display(Name = "EXCEL FSIST x XML EMPRESA")] FsistXEmpresa,
        [Display(Name = "EXCEL MALHA x SPED")] Malha,
        [Display(Name = "DIFERENÇA DE TOTAIS")] DifereValor,
        [Display(Name = "DIFERENÇA DE CRÉDITOS")] DifereIcms,
        [Display(Name = "DIFERENÇA DE TOTAIS XML SEFAZ")] DifereValorSefaz,
        [Display(Name = "DIFERENÇA DE TOTAIS XML EMPRESA")] DifereValorEmpresa,
        [Display(Name = "Malha Cartão")] MalhaCartao
    }

    public enum OrdemCancellation
    {
        [Display(Name = "VERIFICAR NOTAS CANCELADAS SEFAZ")] VerificarSefaz,
        [Display(Name = "VERIFICAR NOTAS CANCELADAS EMPRESA")] VerificarEmpresa,
        [Display(Name = "NOTAS CANCELADAS SEFAZ")] NotasSefaz,
        [Display(Name = "NOTAS CANCELADAS EMPRESA")] NotasEmpresa
    }

    public enum Archive
    {
        [Display(Name = "NFe XML SEFAZ")] XmlNFeSefaz,
        [Display(Name = "NFe XML EMPRESA")] XmlNFeEmpresa,
        [Display(Name = "NFe Sped")] SpedNFe,
        [Display(Name = "CTe XML SEFAZ")] XmlCTeSefaz,
        [Display(Name = "CTe XML EMPRESA")] XmlCTeEmpresa,
        [Display(Name = "CTe Sped")] SpedCTe
    }
}