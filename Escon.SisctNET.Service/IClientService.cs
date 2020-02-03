using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IClientService : IServiceBase<Client>
    {
        Client FindByDocument(int document, Model.Log log = null);

        Client FindByName(string name, Model.Log log = null);

        List<Client> FindByCompanyId(int companyId, Model.Log log = null);

        Client FindByDocumentCompany(int companyId, string document, Model.Log log = null);

        List<Client> FindByLast(int companyId, int count, Model.Log log = null);
    }
}
