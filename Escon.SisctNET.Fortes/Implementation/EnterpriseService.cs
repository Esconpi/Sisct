using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Fortes.Implementation
{
    public class EnterpriseService : ContextDataBase.SqlContextFortesDb, IEnterpriseService
    {

        public EnterpriseService()
            : base()
        {

        }

        public List<Company> GetCompanies(int lastCodigo, string connectionString)
        {
            List<Company> companies = new List<Company>();

            try
            {
                using (_SqlConnection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    _SqlConnection.Open();

                    using (_SqlCommand = new System.Data.SqlClient.SqlCommand())
                    {
                        _SqlCommand.Connection = _SqlConnection;
                        _SqlCommand.CommandText = $"select * from EMP where CAST(codigo as Int)>{lastCodigo} order by cast(Codigo as int) asc";

                        using (_SqlDataReader = _SqlCommand.ExecuteReader())
                        {
                            while (_SqlDataReader.Read())
                            {
                                Company c = new Company();
                                c.Code = _SqlDataReader["Codigo"].ToString();
                                if (!string.IsNullOrEmpty(_SqlDataReader["CNPJBase"].ToString()))
                                {
                                    List<string> establishment = new List<string>();

                                    string cnpj = _SqlDataReader["CNPJBase"].ToString();


                                    establishment = SeqCnpj(c.Code, connectionString);

                                    for (int i = 0; i < establishment.Count; i++)
                                    {
                                        Company company = new Company();
                                        company.Active = false;
                                        company.Incentive = false;
                                        company.Status = true;
                                        company.Created = DateTime.Now;
                                        company.Updated = company.Created;
                                        company.SocialName = _SqlDataReader["RazaoSocial"].ToString();
                                        company.FantasyName = _SqlDataReader["NmFantasia"].ToString();
                                        company.Code = _SqlDataReader["Codigo"].ToString();


                                        string seqCnpj = establishment[i];
                                        string digit = DigitCnpj(cnpj + seqCnpj);
                                        company.Document = cnpj + seqCnpj + digit;
                                        companies.Add(company);
                                    }
                                }
                                else
                                {
                                    Company company = new Company();

                                    company.Active = false;
                                    company.Incentive = false;
                                    company.Status = true;
                                    company.Created = DateTime.Now;
                                    company.Updated = company.Created;
                                    company.SocialName = _SqlDataReader["RazaoSocial"].ToString();
                                    company.FantasyName = _SqlDataReader["NmFantasia"].ToString();
                                    company.Code = _SqlDataReader["Codigo"].ToString();
                                    company.Document = _SqlDataReader["CPF"].ToString();
                                    companies.Add(company);
                                }

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

            return companies;
        }

        public List<string> SeqCnpj(string codeEmp, string connectionString)
        {
            List<string> establishment = new List<string>();
            try
            {
                using (System.Data.SqlClient.SqlConnection ctx = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    ctx.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand())
                    {
                        string query = $"select * from EST where EMP_codigo='{codeEmp}'";

                        cmd.Connection = ctx;
                        cmd.CommandText = query;
                        using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                establishment.Add(reader["SeqCNPJ"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return establishment;
        }

        public string DigitCnpj(string cnpj)
        {
            List<int> weightdigit1 = new List<int> { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            List<int> weightdigit2 = new List<int> { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var sumdigit1 = 0;
            var sumdigit2 = 0;
            var digit = "";

            for (int i = 0; i < cnpj.Length; i++)
            {
                var number = cnpj[i].ToString();
                sumdigit1 += (weightdigit1[i] * Convert.ToInt32(number));
            }
            var digit1 = sumdigit1 % 11;
            var cnpj2 = cnpj;
            if (digit1 >= 2)
            {
                cnpj2 += (11 - digit1);
                digit += (11 - digit1);
            }
            else
            {
                cnpj2 += 0;
                digit += 0;
            }

            for (int i = 0; i < cnpj2.Length; i++)
            {
                var number = cnpj2[i].ToString();
                sumdigit2 += (weightdigit2[i] * Convert.ToInt32(number));
            }

            var digit2 = sumdigit2 % 11;

            if (digit2 >= 2)
            {
                digit += (11 - digit2);
            }
            else
            {
                digit += 0;
            }

            return digit;
        }
    }
}
