using System.Collections.Generic;

namespace Escon.SisctNET.Fortes
{
    public interface IEnterpriseService
    {
        List<Model.Company> GetCompanies(List<Model.State> states, string connectionString);
    }
}