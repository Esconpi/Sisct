using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Fortes.Implementation
{
    public class EnterpriseService : ContextDataBase.SqlContextFortesDb, IEnterpriseService
    {

        public EnterpriseService()
            : base()
        {

        }

        /**
         * Campos tabela EMP
         * Codigo, Nome, USU_Codigo, Modelo, RazaoSocial, NmFantasia, CNPJBase, CPF, CEI, FatAnualSup, 
         * FGTSContribSocial, Tributacao, LiminarContribSocial, CentralizaFGTS, logotipo, SetorPublico, 
         * ProdRural, DtIniAtiv, Desativada, OptanteSimples, IntegraPainel, MEI, Bloqueada, CNPJSCP,
         * Guid, DtAdesaoReinfProd, DtAdesaoReinfHomo, GuidProdReinf, GuidHomoReinf, AmbienteAtivoReinf, 
         * AdesaoFilialReinf, BloqueioESocial, ReintegradoReinf, DataReintegracaoReinf, ADESAOMANUAL
         */

        public List<Company> GetCompanies(List<Model.County> counties, string connectionString)
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
                        _SqlCommand.CommandText = $"select * from EMP order by cast(Codigo as int) asc";

                        using (_SqlDataReader = _SqlCommand.ExecuteReader())
                        {
                            while (_SqlDataReader.Read())
                            {
                                Company c = new Company();
                                c.Code = _SqlDataReader["Codigo"].ToString();
     
                                List<Company> establishments = new List<Company>();
                                establishments = GetEstablishment(c.Code, counties, connectionString);

                                if (!string.IsNullOrEmpty(_SqlDataReader["CNPJBase"].ToString()))
                                {
                                    string cnpj = _SqlDataReader["CNPJBase"].ToString();

                                    foreach(var establishment in establishments)
                                    {
                                        Company company = new Company();
                                        company.Active = false;
                                        company.Incentive = false;
                                        company.Status = true;
                                        company.Created = DateTime.Now;
                                        company.Updated = company.Created;
                                        company.SocialName = _SqlDataReader["RazaoSocial"].ToString().Replace("/","").Replace("\\", "").Replace("|", "");
                                        company.FantasyName = _SqlDataReader["NmFantasia"].ToString().Replace("/","").Replace("\\", "").Replace("|", "");
                                        company.Code = _SqlDataReader["Codigo"].ToString();
                                        string seqCnpj = establishment.Document;
                                        string digit = DigitCnpj(cnpj + seqCnpj);
                                        company.Document = cnpj + seqCnpj + digit;
                                        company.Ie = establishment.Ie;
                                        company.Logradouro = establishment.Logradouro;
                                        company.Number = establishment.Number;
                                        company.Complement = establishment.Complement;
                                        company.District = establishment.District;
                                        company.Cep = establishment.Cep;
                                        company.CountyId = establishment.CountyId;
                                        company.Phone = establishment.Phone;
                                        companies.Add(company);
                                    }
                                }
                                else
                                {
                                    
                                    foreach (var establishment in establishments)
                                    {
                                        Company company = new Company();
                                        company.Active = false;
                                        company.Incentive = false;
                                        company.Status = true;
                                        company.Created = DateTime.Now;
                                        company.Updated = company.Created;
                                        company.SocialName = _SqlDataReader["RazaoSocial"].ToString().Replace("/","").Replace("\\", "").Replace("|", "");
                                        company.FantasyName = _SqlDataReader["NmFantasia"].ToString().Replace("/","").Replace("\\", "").Replace("|", "");
                                        company.Code = _SqlDataReader["Codigo"].ToString();
                                        company.Document = _SqlDataReader["CPF"].ToString();
                                        company.Ie = establishment.Ie;
                                        company.Logradouro = establishment.Logradouro;
                                        company.Number = establishment.Number;
                                        company.Complement = establishment.Complement;
                                        company.District = establishment.District;
                                        company.Cep = establishment.Cep;
                                        company.CountyId = establishment.CountyId;
                                        company.Phone = establishment.Phone;
                                        companies.Add(company);
                                    }
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

        /**
         * Campos tabela EST
         * EMP_Codigo, Codigo, Nome, SeqCNPJ, EndLogradouro, EndNumero, EndComplemento, Bairro, CEP, MUN_UFD_Sigla,
         * MUN_Codigo, DDD, Fone, Fax, Email, IE, IM, CEFCodigoFGTS, FPAS, CNAE_Fiscal, GerouCAGED, DtEncerramento, 
         * AtividadeEconomica, Matriz, CentralizadorFGTS, AliquotaSAT, CodigoTerceiros, CodigoGPS, INSSAliqTerceiros, 
         * NaturezaJuridica, NrProprietarios, MesDataBase, ParticipaPAT, PercServicoProprio, PercAdministracaoCozinha, 
         * PercRefeicaoConvenio, PercRefeicoesTransportadas, PercCestaAlimento, PercAlimentacaoConvenio, INSSAliqContrib,
         * PessoaContato, CTA_Codigo, SociedSimples, IPI, ICMS, ISS, Telefonica, DTINIATIV, CentroCustoProsoft, NIRC, 
         * CNAE2_Codigo, EST_CODIGO_AG, Energia, Comunicacao, Tranporte, Varejista, CodigoWebContabil, COD_EST_CONTABIL,
         * Combustiveis, SUFRAMA, EST_CODIGO_CONTABIL, PrestadorServSaude, CNES, VeiculosUsados, ESOCIAL, SociedUniprofissional, CEI, CAD_ITR
         */
        
        public List<Company> GetEstablishment(string codeEmp, List<Model.County> counties, string connectionString)
        {
            List<Company> establishments = new List<Company>();
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
                                Company establishment = new Company();
                                establishment.Document = reader["SeqCNPJ"].ToString();
                                establishment.Ie = reader["IE"].ToString();
                                establishment.Logradouro = reader["EndLogradouro"].ToString();
                                establishment.Number = reader["EndNumero"].ToString();
                                establishment.Complement = reader["EndComplemento"].ToString();
                                establishment.District = reader["Bairro"].ToString();
                                establishment.Cep = reader["CEP"].ToString();
                                var city = GetCity(reader["MUN_Codigo"].ToString(), reader["MUN_UFD_Sigla"].ToString(), connectionString).ToUpper();
                                var county = counties.Where(_ => _.State.UF.ToUpper().Equals(reader["MUN_UFD_Sigla"].ToString().ToUpper()) && _.Name.ToUpper().Equals(city)).Select(_ => _.Id).FirstOrDefault();
                                establishment.CountyId = county.Equals(null) ? 216 : county;
                                establishment.Phone = "(" + reader["DDD"].ToString() + ")"  + " " + reader["Fone"].ToString();
                                establishments.Add(establishment);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return establishments;
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

        /**
         * Campos tabela MUN
         * UFD_Sigla, Codigo, Nome, CodMunDIPJ, CodMunGIA, CODMUNDIEFPA, CODMUNBAHIA, CodMunRJ, CODDMISS, CodMunSEF, CodMunGIMPB,
         * CodMunDIMESC, CodMunGIARS, CodMunDIEFES, CodMunMarituba, CodMunGoiania, CodMunDOTES
         */

        public string GetCity(string codeMun, string uf, string connectionString)
        {
            string city = "";
            try
            {
                using (System.Data.SqlClient.SqlConnection ctx = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    ctx.Open();
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand())
                    {
                        string query = $"select * from MUN where Codigo='{codeMun}' and UFD_Sigla='{uf}'";

                        cmd.Connection = ctx;
                        cmd.CommandText = query;
                        using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                city = reader["Nome"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return city;
        }
        
    }
}
