using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("tax")]
    public class Tax : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public long CompanyId { get; set; }

        private Company company;
        public Company Company
        {
            get => LazyLoader.Load(this, ref company);
            set => company = value;
        }

        public string Arquivo { get; set; }

        [Display(Name = "Mes")]
        public string MesRef { get; set; }

        [Display(Name = "Ano")]
        public string AnoRef { get; set; }

        //  ICMS
        public decimal? Compras { get; set; }

        public decimal? DevolucaoCompras { get; set; }

        public decimal? Vendas { get; set; }

        public decimal? VendasIncisoI { get; set; }

        public decimal? VendasIncisoII { get; set; }

        public decimal? VendasNcm { get; set; }

        public decimal? VendasNContribuinte { get; set; }

        public decimal? DevolucaoVendas { get; set; }

        public decimal? DevolucaoVendasIncisoI { get; set; }

        public decimal? DevolucaoVendasIncisoII { get; set; }

        public decimal? DevolucaoNcm { get; set; }

        public decimal? DevolucaoNContribuinte { get; set; }

        public decimal? TransferenciaInter { get; set; }

        public decimal? TransferenciaEntrada { get; set; }

        public decimal? TransferenciaSaida { get; set; }

        public decimal? Suspensao { get; set; }

        public decimal? Credito { get; set; }

        public decimal? Debito { get; set; }

        public decimal? VendasNContribuinteFora { get; set; }

        public decimal? VendasContribuinte1 { get; set; }

        public decimal? VendasContribuinte2 { get; set; }

        public decimal? ReceitaNormal1 { get; set; }

        public decimal? ReceitaST1 { get; set; }

        public decimal? ReceitaIsento1 { get; set; }

        public decimal? ReceitaNormal2 { get; set; }

        public decimal? ReceitaST2 { get; set; }

        public decimal? ReceitaIsento2 { get; set; }

        public decimal? ReceitaNormal3 { get; set; }

        public decimal? ReceitaST3 { get; set; }

        public decimal? ReceitaIsento3 { get; set; }

        public decimal? VendasIncentivada { get; set; }

        public decimal? VendasNIncentivada { get; set; }

        public decimal? VendasClientes { get; set; }

        public decimal? VendasInternas1 { get; set; }

        public decimal? VendasInterestadual1 { get; set; }

        public decimal? SaidaInterna1 { get; set; }

        public decimal? SaidaPortInterna1 { get; set; }

        public decimal? SaidaInterestadual1 { get; set; }

        public decimal? SaidaPortInterestadual1 { get; set; }

        public decimal? VendasInternas2 { get; set; }

        public decimal? VendasInterestadual2 { get; set; }

        public decimal? SaidaInterna2 { get; set; }

        public decimal? SaidaPortInterna2 { get; set; }

        public decimal? SaidaInterestadual2 { get; set; }

        public decimal? SaidaPortInterestadual2 { get; set; }

        public bool Icms { get; set; }

        public bool PisCofins { get; set; }

        //  PIS e COFINS
        public decimal? PisRetido { get; set; }

        public decimal? CofinsRetido { get; set; }

        // Lucro Presumido
        public decimal? Receita1 { get; set; }

        public decimal? Receita2 { get; set; }

        public decimal? Receita3 { get; set; }

        public decimal? Receita4 { get; set; }

        public decimal? ReceitaMono { get; set; }

        public decimal? Devolucao1Entrada { get; set; }

        public decimal? Devolucao2Entrada { get; set; }

        public decimal? Devolucao3Entrada { get; set; }

        public decimal? Devolucao4Entrada { get; set; }
        
        public decimal? DevolucaoNormalEntrada { get; set; }

        public decimal? Devolucao1Saida { get; set; }

        public decimal? Devolucao2Saida { get; set; }

        public decimal? Devolucao3Saida { get; set; }

        public decimal? Devolucao4Saida { get; set; }

        public decimal? DevolucaoNormalSaida { get; set; }

        public decimal? CsllRetido { get; set; }

        public decimal? CsllFonte { get; set; }

        public decimal? IrpjRetido { get; set; }

        public decimal? IrpjFonteServico { get; set; }

        public decimal? IrpjFonteFinanceira { get; set; }

        public decimal? Bonificacao { get; set; }

        public decimal? ReceitaAF { get; set; }

        public decimal? CapitalIM { get; set; }

        public decimal? OutrasReceitas { get; set; }

        public decimal? ReducaoIcms { get; set; }

        public decimal? IrpjPago { get; set; }

        public decimal? CsllPago { get; set; }

        // Lucro Real
        public decimal? Compra { get; set; }

        public decimal? CompraTI { get; set; }

        public decimal? DevolucaoCompra { get; set; }

        public decimal? Energia { get; set; }

        public decimal? AluguelPredio { get; set; }

        public decimal? AluguelME { get; set; }

        public decimal? DespesasF { get; set; }

        public decimal? DespesasME { get; set; }

        public decimal? DespesasA { get; set; }

        public decimal? SaldoCredorPis { get; set; }

        public decimal? SaldoCredorCofins { get; set; }

        public decimal? Venda { get; set; }

        public decimal? VendaTI { get; set; }

        public decimal? DevolucaoVenda { get; set; }

        public decimal? PrestacaoServico { get; set; }

        public decimal? ReceitaFinanceira { get; set; }

        public decimal? Capital { get; set; }

        public decimal? ReceitaAluguel { get; set; }

        public decimal? Juros { get; set; }

        public decimal? Perda { get; set; }

        //  Simples Nacional

        public decimal? VendaNormal { get; set; }

        public decimal? VendaST { get; set; }

        public decimal? VendaMonofasico { get; set; }

        public decimal? DevoNormal { get; set; }

        public decimal? DevoST { get; set; }

        public decimal? DevoMonofasico { get; set; }


    }
}
