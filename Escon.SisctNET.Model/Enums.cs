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
        [Display(Name = "Produto Fora do Incentivo")] ProdutoFI = 5,
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
        [Display(Name = "XML x SPED")] Xml = 1,
        [Display(Name = "SPED x XML")] Sped = 2,
        [Display(Name = "DIFERENÇA DE TOTAIS")] DifereValor = 3,
        [Display(Name = "XML x SPED SisCT")] SisCT = 4,
        [Display(Name = "Excel x SPED")] Malha = 5
    }

    public enum Archive
    {
        [Display(Name = "NFe Xml")] XmlNFe = 1,
        [Display(Name = "CTe Xml")] XmlCTe = 2,
        [Display(Name = "NFe Sped")] SpedNFe = 3,
        [Display(Name = "CTe Sped")] SpedCTe = 4
    }
}