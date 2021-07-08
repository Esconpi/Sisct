using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Fortes.Implementation
{
    public class SDOService : ContextDataBase.SqlContextFortesDb, ISDOService
    {
        public SDOService()
            :base()
        {
        }

        /**
        * Campos tabela CON
        * EMP_Codigo, Codigo, Reduzido, Analitica, Patrimonial, Resumir, Natureza, EST_Codigo, CRS_Codigo, EnglobaFilhas, CON_Codigo_Mae,
        * Grupo, AjusteLucroReal, NAL_Codigo, CdAlternat, NmAlternat, LivroCaixa, Nome, PCR_Codigo, Desativada, DFC, ExpurgoFCont, PCR2014_Codigo,
        * PCRPJLP_Codigo, PCRFIN_Codigo, PCRSEG_Codigo, PCRIIG_Codigo, PCRFII_Codigo, PCRSII_Codigo, PCREFPC_Codigo, PCRPP_Codigo, Codigo_COSIF, 
        * Prazo, NSB_Codigo, CON_Codigo_Vinculada, PCRPJLPF_Codigo, CPFCNPJBase, PCRPP2_Codigo, OutrasSPED, TIPO_LANC, BAN_CODIGO, AGE_CODIGO, CONTACORRENTENUMERO
        */

        public List<List<string>> GetBalancete(Company company, DateTime inicio, DateTime fim, string connectionString)
        {
            List<List<string>> balancete = new List<List<string>>();

            try
            {
                using (_SqlConnection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    _SqlConnection.Open();

                    using (_SqlCommand = new System.Data.SqlClient.SqlCommand())
                    {
                        _SqlCommand.Connection = _SqlConnection;
                        _SqlCommand.CommandText = $"select * from CON where EMP_Codigo = '{company.Code}'";

                        using (_SqlDataReader = _SqlCommand.ExecuteReader())
                        {
                            while (_SqlDataReader.Read())
                            {
                                var saldoAnterior = GetPreviousBalance(company, _SqlDataReader["Codigo"].ToString(), inicio, connectionString);
                                var saldoAtual = GetCurrentBalance(company, _SqlDataReader["Codigo"].ToString(), inicio, fim, connectionString);
                                List<string> conta = new List<string>();
                                conta.Add(_SqlDataReader["Codigo"].ToString());
                                conta.Add(_SqlDataReader["Nome"].ToString());
                                conta.Add(saldoAnterior.ToString());
                                conta.Add(saldoAtual.ToString());
                                balancete.Add(conta);
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

            return balancete;
        }


        /**
        * Campos tabela SDO
        * EMP_Codigo, CON_Codigo, EST_Codigo, CRS_Codigo, Data, Debito, Credito
        */
        public decimal GetPreviousBalance(Company company, string conta, DateTime inicio, string connectionString)
        {
            decimal saldo = 0;

            try
            {
                using (System.Data.SqlClient.SqlConnection ctx = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    ctx.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand())
                    {
                        string query = $"select top 1 * from SDO " +
                           $"where SDO.CON_Codigo = '{conta}' and SDO.EMP_Codigo = '{company.Code}' and SDO.Data < '{inicio}' " +
                           $"order by SDO.Data desc";

                        cmd.Connection = ctx;
                        cmd.CommandText = query;
                        using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                decimal debito =  Convert.ToDecimal(reader["Debito"]);
                                decimal credito = Convert.ToDecimal(reader["Credito"]);

                                saldo = debito - credito;
                                
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }


            return saldo;
        }

        public decimal GetCurrentBalance(Company company, string conta, DateTime inicio, DateTime fim, string connectionString)
        {
            decimal saldo = 0;

            try
            {
                using (System.Data.SqlClient.SqlConnection ctx = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    ctx.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand())
                    {
                        string query = $"select top 1 * from SDO " +
                           $"where SDO.CON_Codigo = '{conta}' and SDO.EMP_Codigo = '{company.Code}' and SDO.Data < '{fim.AddDays(1)}'" +
                           $"order by sdo.Data desc";

                        cmd.Connection = ctx;
                        cmd.CommandText = query;
                        using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                decimal debito = Convert.ToDecimal(reader["Debito"]);
                                decimal credito = Convert.ToDecimal(reader["Credito"]);

                                saldo = debito - credito;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }


            return saldo;
        }

        List<List<string>> ISDOService.GetDisponibilidadeFinanceira(Company company, DateTime inicio, DateTime fim, string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
