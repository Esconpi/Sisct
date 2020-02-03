using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProviderRepository : IRepository<Provider>
    {
        Provider FindByDocument(int document, Model.Log log = null);

        Provider FindByName(string name, Model.Log log = null);

        List<Provider> FindByCompanyId(int companyId, Model.Log log = null);
    }
}
