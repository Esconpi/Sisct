using System.Collections.Generic;

namespace Escon.SisctNET.Fortes
{
    public interface IEnterpriseService
    {
        List<Model.Company> GetCompanies(int lastCodigo, string connectionString);
    }
}