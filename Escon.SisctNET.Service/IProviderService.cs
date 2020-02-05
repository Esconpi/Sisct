using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IProviderService : IServiceBase<Provider>
    {
        Provider FindByDocument(int document, Model.Log log = null);

        Provider FindByName(string name, Model.Log log = null);

        List<Provider> FindByCompanyId(int companyId, Model.Log log = null);
    }
}
