
using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Fortes.Implementation
{
    public class AccountPlanService : ContextDataBase.SqlContextFortesDb, IAccountPlanService
    {
        public AccountPlanService()
        {

        }

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
                        _SqlCommand.CommandText = $"select * from con where emp_codigo = {company.Code}";
                        using (_SqlDataReader = _SqlCommand.ExecuteReader())
                        {
                            while (_SqlDataReader.Read())
                            {
                                AccountPlan accountPlan = new AccountPlan()
                                {
                                    AccountPlanTypeId = 4,
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
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return accountPlans;
        }
    }
}
