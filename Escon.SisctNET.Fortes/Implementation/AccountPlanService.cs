using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Fortes.Implementation
{
    public class AccountPlanService : ContextDataBase.SqlContextFortesDb, IAccountPlanService
    {
        public AccountPlanService()
            : base()
        {

        }

        /**
      * Campos tabela CON
      * EMP_Codigo, Codigo, Reduzido, Analitica, Patrimonial, Resumir, Natureza, EST_Codigo, CRS_Codigo, EnglobaFilhas, CON_Codigo_Mae,
      * Grupo, AjusteLucroReal, NAL_Codigo, CdAlternat, NmAlternat, LivroCaixa, Nome, PCR_Codigo, Desativada, DFC, ExpurgoFCont, PCR2014_Codigo,
      * PCRPJLP_Codigo, PCRFIN_Codigo, PCRSEG_Codigo, PCRIIG_Codigo, PCRFII_Codigo, PCRSII_Codigo, PCREFPC_Codigo, PCRPP_Codigo, Codigo_COSIF, 
      * Prazo, NSB_Codigo, CON_Codigo_Vinculada, PCRPJLPF_Codigo, CPFCNPJBase, PCRPP2_Codigo, OutrasSPED, TIPO_LANC, BAN_CODIGO, AGE_CODIGO, CONTACORRENTENUMERO
      */

        public List<AccountPlan> GetAccountPlans(Company company, string connectionString)
        {
            List<AccountPlan> accountPlans = new List<AccountPlan>();
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
                                AccountPlan accountPlan = new AccountPlan()
                                {
                                    AccountPlanTypeId = 21,
                                    Active = false,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    Name = _SqlDataReader["Nome"].ToString(),
                                    Analytical = _SqlDataReader["Analitica"].ToString().Equals("0") ? false : true,
                                    Patrimonial = _SqlDataReader["Patrimonial"].ToString().Equals("0") ? false : true,
                                    Code = _SqlDataReader["Codigo"].ToString(),
                                    Reduced = !string.IsNullOrEmpty(_SqlDataReader["Reduzido"].ToString()) ? Convert.ToInt32(_SqlDataReader["Reduzido"].ToString()) : 0,
                                    CompanyId = company.Id
                                };

                                accountPlans.Add(accountPlan);
                            }
                        }
                    }

                    _SqlConnection.Close();
                }

                base.Dispose();
                base.Dispose();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return accountPlans;
        }
    }
}
