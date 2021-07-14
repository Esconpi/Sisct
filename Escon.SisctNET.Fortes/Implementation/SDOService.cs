using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Fortes.Implementation
{
    public class SDOService : ContextDataBase.SqlContextFortesDb, ISDOService
    {
        public SDOService()
            :base()
        {
        }

        /**
        * Campos tabela SDO
        * EMP_Codigo, CON_Codigo, EST_Codigo, CRS_Codigo, Data, Debito, Credito
        */

        public List<List<string>> GetDisponibilidadeFinanceira(List<AccountPlan> accountPlans, Company company, DateTime inicio, DateTime fim, string connectionString)
        {
            List<List<string>> saldos = new List<List<string>>();

            accountPlans = accountPlans.Where(_ => _.AccountPlanType.AccountPlanTypeGroup.Name.Equals("Disponibilidade Financeira")).ToList();

            decimal saldoAnteriorCaixa = 0, saldoAtualCaixa = 0, saldoAnteriorBanco = 0, saldoAtualBanco = 0, saldoAnteriorOutras = 0, saldoAtualOutras = 0;

            var contasCaixa = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Caixa"))
                .Select(_ => _.Code)
                .ToList();

            var contasBanco = accountPlans
               .Where(_ => _.AccountPlanType.Name.Equals("Banco"))
               .Select(_ => _.Code)
               .ToList();

            var contasOutras = accountPlans
              .Where(_ => _.AccountPlanType.Name.Equals("Outras"))
              .Select(_ => _.Code)
              .ToList();

            foreach (var conta in accountPlans)
            {
                var saldoAnterior = GetPreviousBalance(company, conta.Code, inicio, connectionString);
                var saldoAtual = GetCurrentBalance(company, conta.Code, fim, connectionString);

                if (contasCaixa.Contains(conta.Code))
                {
                    saldoAnteriorCaixa += saldoAnterior;
                    saldoAtualCaixa += saldoAtual;
                }
                else if (contasBanco.Contains(conta.Code))
                {
                    saldoAnteriorBanco += saldoAnterior;
                    saldoAtualBanco += saldoAtual;
                }
                else if (contasOutras.Contains(conta.Code))
                {
                    saldoAnteriorOutras += saldoAnterior;
                    saldoAtualOutras += saldoAtual;
                }
            }

            List<string> saldoCaixa = new List<string>();
            saldoCaixa.Add("Caixa");
            saldoCaixa.Add(saldoAnteriorCaixa.ToString());
            saldoCaixa.Add(saldoAtualCaixa.ToString());
            saldos.Add(saldoCaixa);

            List<string> saldoBanco = new List<string>();
            saldoBanco.Add("Banco");
            saldoBanco.Add(saldoAnteriorBanco.ToString());
            saldoBanco.Add(saldoAtualBanco.ToString());
            saldos.Add(saldoBanco);

            List<string> saldoOutras = new List<string>();
            saldoOutras.Add("Outras");
            saldoOutras.Add(saldoAnteriorOutras.ToString());
            saldoOutras.Add(saldoAtualOutras.ToString());
            saldos.Add(saldoOutras);


            return saldos;
        }

        public List<List<string>> GetDespesasOperacionais(List<AccountPlan> accountPlans, Company company, DateTime inicio, DateTime fim, string connectionString)
        {
            List<List<string>> saldos = new List<List<string>>();

            accountPlans = accountPlans.Where(_ => _.AccountPlanType.AccountPlanTypeGroup.Name.Equals("Despesas Operacionais")).ToList();

            decimal proLabore = 0, comissao = 0, combustivel = 0, encargo = 0, tFederal = 0, tEstadual = 0, tMunicipal = 0, agua = 0, aluguel = 0, 
                servico = 0, seguro = 0, frete = 0, despesas = 0, outras = 0;

            var contasProLabore = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Pró-Labore"))
                .Select(_ => _.Code)
                .ToList();

            var contasComissao = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Comissões, Salários, Ordenados"))
                .Select(_ => _.Code)
                .ToList();

            var contasCombustivel = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Combustíveis e Lubrificantes"))
                .Select(_ => _.Code)
                .ToList();

            var contasEncargo = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Encargos Sociais"))
                .Select(_ => _.Code)
                .ToList();

            var contasTFederal = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Tributos Federais"))
                .Select(_ => _.Code)
                .ToList();

            var contasTEstadual = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Tributos Estaduais"))
                .Select(_ => _.Code)
                .ToList();

            var contasTMunicipal = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Tributos Municipais"))
                .Select(_ => _.Code)
                .ToList();

            var contasAgua = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Água, Luz, Telefone"))
                .Select(_ => _.Code)
                .ToList();

            var contasAluguel = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Alugueis"))
                .Select(_ => _.Code)
                .ToList();

            var contasServico = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Serviços Profissionais"))
                .Select(_ => _.Code)
                .ToList();

            var contasSeguro = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Seguros"))
                .Select(_ => _.Code)
                .ToList();

            var contasFrete = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Fretes e Carretos"))
                .Select(_ => _.Code)
                .ToList();

            var contasDespesas = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Despesas Financeiras"))
                .Select(_ => _.Code)
                .ToList();

            var contasOutras = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Outras Despesas"))
                .Select(_ => _.Code)
                .ToList();

            foreach (var conta in accountPlans)
            {
                var saldoAntarior = GetPreviousBalance(company, conta.Code, inicio, connectionString);
                var saldoAtual = GetCurrentBalance(company, conta.Code, fim, connectionString);

                decimal saldo = 0;

                if(saldoAtual > saldoAntarior)
                    saldo = saldoAtual - saldoAntarior;

                
                if (contasProLabore.Contains(conta.Code))
                {
                    proLabore += saldo;
                }
                else if (contasComissao.Contains(conta.Code)) 
                {
                    comissao += saldo;
                }
                else if (contasCombustivel.Contains(conta.Code))
                {
                    combustivel += saldo;
                }
                else if (contasEncargo.Contains(conta.Code))
                {
                    encargo += saldo;
                }
                else if (contasTFederal.Contains(conta.Code))
                {
                    tFederal += saldo;
                }
                else if (contasTEstadual.Contains(conta.Code))
                {
                    tEstadual += saldo;
                }
                else if (contasTMunicipal.Contains(conta.Code))
                {
                    tMunicipal += saldo;
                }
                else if (contasAgua.Contains(conta.Code))
                {
                    agua += saldo;
                }
                else if (contasAluguel.Contains(conta.Code))
                {
                    aluguel += saldo;
                }
                else if (contasServico.Contains(conta.Code))
                {
                    servico += saldo;
                }
                else if (contasSeguro.Contains(conta.Code))
                {
                    seguro += saldo;
                }
                else if (contasFrete.Contains(conta.Code))
                {
                    frete += saldo;
                }
                else if (contasDespesas.Contains(conta.Code))
                {
                    despesas += saldo;
                }
                else if (contasOutras.Contains(conta.Code))
                {
                    outras += saldo;
                }
            }

            List<string> saldoProLabore = new List<string>();
            saldoProLabore.Add("01 - Pró-Labore");
            saldoProLabore.Add(proLabore.ToString());
            saldos.Add(saldoProLabore);

            List<string> saldoComissao = new List<string>();
            saldoComissao.Add("02 - Comissões, Salários, Ordenadose");
            saldoComissao.Add(comissao.ToString());
            saldos.Add(saldoComissao);
                
            List<string> saldoCombustivel = new List<string>();
            saldoCombustivel.Add("03 - Combustíveis e Lubrificantee");
            saldoCombustivel.Add(combustivel.ToString());
            saldos.Add(saldoCombustivel);

            List<string> saldoEncargo = new List<string>();
            saldoEncargo.Add("04 - Encargos Sociaise");
            saldoEncargo.Add(encargo.ToString());
            saldos.Add(saldoEncargo);

            List<string> saldoTFederal = new List<string>();
            saldoTFederal.Add("05 - Tributos Federais");
            saldoTFederal.Add(tFederal.ToString());
            saldos.Add(saldoTFederal);

            List<string> saldoTEstadual = new List<string>();
            saldoTEstadual.Add("06 - Tributos Estaduais");
            saldoTEstadual.Add(tEstadual.ToString());
            saldos.Add(saldoTEstadual);

            List<string> saldoTMunicipal = new List<string>();
            saldoTMunicipal.Add("07 - Tributos Municipais");
            saldoTMunicipal.Add(tMunicipal.ToString());
            saldos.Add(saldoTMunicipal);

            List<string> saldoAgua = new List<string>();
            saldoAgua.Add("08 - Água, Luz, Telefone");
            saldoAgua.Add(agua.ToString());
            saldos.Add(saldoAgua);

            List<string> saldoAluguel = new List<string>();
            saldoAluguel.Add("09 - Alugueis");
            saldoAluguel.Add(aluguel.ToString());
            saldos.Add(saldoAluguel);

            List<string> saldoServico = new List<string>();
            saldoServico.Add("10 - Serviços Profissionais");
            saldoServico.Add(servico.ToString());
            saldos.Add(saldoServico);

            List<string> saldoSeguro = new List<string>();
            saldoSeguro.Add("11 - Seguros");
            saldoSeguro.Add(seguro.ToString());
            saldos.Add(saldoSeguro);

            List<string> saldoFrete = new List<string>();
            saldoFrete.Add("12 - Fretes e Carretos");
            saldoFrete.Add(frete.ToString());
            saldos.Add(saldoFrete);

            List<string> saldoDespesas = new List<string>();
            saldoDespesas.Add("13 - Despesas Financeiras");
            saldoDespesas.Add(despesas.ToString());
            saldos.Add(saldoDespesas);

            List<string> saldoOutras = new List<string>();
            saldoOutras.Add("14 - Outras Despesas");
            saldoOutras.Add(outras.ToString());
            saldos.Add(saldoOutras);

            return saldos;
        }

        public List<List<string>> GetEstoqueMercadorias(List<AccountPlan> accountPlans, Company company, DateTime inicio, DateTime fim, string connectionString)
        {
            List<List<string>> saldos = new List<List<string>>();

            accountPlans = accountPlans.Where(_ => _.AccountPlanType.AccountPlanTypeGroup.Name.Equals("Estoque de Mercadorias")).ToList();


            decimal saldoAnteriorCaixa = 0, saldoAtualCaixa = 0, saldoAnteriorBanco = 0, saldoAtualBanco = 0, saldoAnteriorOutras = 0, saldoAtualOutras = 0;

            var contasTributadas = accountPlans
                .Where(_ => _.AccountPlanType.Name.Equals("Tributadas"))
                .Select(_ => _.Code)
                .ToList();

            var contasNTributadas = accountPlans
               .Where(_ => _.AccountPlanType.Name.Equals("Não Tributadas"))
               .Select(_ => _.Code)
               .ToList();

            var contasOutras = accountPlans
              .Where(_ => _.AccountPlanType.Name.Equals("Outras"))
              .Select(_ => _.Code)
              .ToList();

            foreach (var conta in accountPlans)
            {
                var saldoAnterior = GetPreviousBalance(company, conta.Code, inicio, connectionString);
                var saldoAtual = GetCurrentBalance(company, conta.Code, fim, connectionString);

                if (contasTributadas.Contains(conta.Code))
                {
                    saldoAnteriorCaixa += saldoAnterior;
                    saldoAtualCaixa += saldoAtual;
                }
                else if (contasNTributadas.Contains(conta.Code))
                {
                    saldoAnteriorBanco += saldoAnterior;
                    saldoAtualBanco += saldoAtual;
                }
                else if (contasOutras.Contains(conta.Code))
                {
                    saldoAnteriorOutras += saldoAnterior;
                    saldoAtualOutras += saldoAtual;
                }
            }

            List<string> saldoTributadas = new List<string>();
            saldoTributadas.Add("Tributadas");
            saldoTributadas.Add(saldoAnteriorCaixa.ToString());
            saldoTributadas.Add(saldoAtualCaixa.ToString());
            saldos.Add(saldoTributadas);

            List<string> saldoNTributadas = new List<string>();
            saldoNTributadas.Add("Não Tributadas");
            saldoNTributadas.Add(saldoAnteriorBanco.ToString());
            saldoNTributadas.Add(saldoAtualBanco.ToString());
            saldos.Add(saldoNTributadas);

            List<string> saldoOutras = new List<string>();
            saldoOutras.Add("Outras");
            saldoOutras.Add(saldoAnteriorOutras.ToString());
            saldoOutras.Add(saldoAtualOutras.ToString());
            saldos.Add(saldoOutras);


            return saldos;
        }

        public decimal GetPreviousBalance(Company company, string conta, DateTime inicio, string connectionString)
        {
            decimal saldo = 0;

            try
            {
                using (_SqlConnection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    _SqlConnection.Open();

                    using (_SqlCommand = new System.Data.SqlClient.SqlCommand())
                    {
                        _SqlCommand.Connection = _SqlConnection;
                        _SqlCommand.CommandText = $"select top 1 * from SDO " +
                            $"where CON_Codigo = '{conta}' and EMP_Codigo = '{company.Code}' and EST_Codigo = '{company.Document.Substring(8,4)}'  and Data < cast('{inicio.ToString("yyyy/MM/dd")}' as Date) " +
                            $"order by Data desc";

                        using (_SqlDataReader = _SqlCommand.ExecuteReader())
                        {
                            while (_SqlDataReader.Read())
                            {
                                decimal debito = Convert.ToDecimal(_SqlDataReader["Debito"]);
                                decimal credito = Convert.ToDecimal(_SqlDataReader["Credito"]);

                                if (debito >= credito)
                                    saldo = debito - credito;
                            }
                        }
                    }

                    _SqlConnection.Close();
                }

                base.Dispose();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }


            return saldo;
        }

        public decimal GetCurrentBalance(Company company, string conta, DateTime fim, string connectionString)
        {
            decimal saldo = 0;

            try
            {
                using (_SqlConnection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    _SqlConnection.Open();

                    using (_SqlCommand = new System.Data.SqlClient.SqlCommand())
                    {
                        _SqlCommand.Connection = _SqlConnection;
                        _SqlCommand.CommandText = $"select top 1 * from SDO " +
                           $"where CON_Codigo = '{conta}' and EMP_Codigo = '{company.Code}' and EST_Codigo = '{company.Document.Substring(8, 4)}' and Data <= cast('{fim.ToString("yyyy/MM/dd")}' as Date) " +
                           $"order by Data desc";

                        using (_SqlDataReader = _SqlCommand.ExecuteReader())
                        {
                            while (_SqlDataReader.Read())
                            {
                                decimal debito = Convert.ToDecimal(_SqlDataReader["Debito"]);
                                decimal credito = Convert.ToDecimal(_SqlDataReader["Credito"]);

                                if (debito >= credito)
                                    saldo = debito - credito;
                            }
                        }
                    }

                    _SqlConnection.Close();
                }

                base.Dispose();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return saldo;
        }

        public decimal GetSaldoAnual(Company company, string conta, DateTime inicio, DateTime fim, string connectionString)
        {
            decimal saldo = 0;

            try
            {
                using (_SqlConnection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    _SqlConnection.Open();

                    using (_SqlCommand = new System.Data.SqlClient.SqlCommand())
                    {
                        _SqlCommand.Connection = _SqlConnection;
                        _SqlCommand.CommandText = $"select CON_Codigo, sum(Debito) as Debito, sum(Credito) as Credito from SDO " +
                           $"where CON_Codigo = '{conta}' and EMP_Codigo = '{company.Code}' and EST_Codigo = '{company.Document.Substring(8, 4)}' and Data >= cast('{inicio.ToString("yyyy/MM/dd")}' as Date)" +
                           $" and Data <= cast('{fim.ToString("yyyy/MM/dd")}' as Date) group by CON_Codigo";

                        using (_SqlDataReader = _SqlCommand.ExecuteReader())
                        {
                            while (_SqlDataReader.Read())
                            {
                                decimal debito = Convert.ToDecimal(_SqlDataReader["Debito"]);
                                decimal credito = Convert.ToDecimal(_SqlDataReader["Credito"]);
                                if (debito >= credito)
                                    saldo = debito - credito;
                            }
                        }
                    }

                    _SqlConnection.Close();
                }

                base.Dispose();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return saldo;
        }
     
    }
}
