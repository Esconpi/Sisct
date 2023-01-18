using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IClientService : IServiceBase<Client>
    {
        Client FindByDocument(string document, Model.Log log = null);

        Client FindByName(string name, Model.Log log = null);

        List<Client> FindByCompany(long companyId, Model.Log log = null);

        List<Client> FindByCompany(long companyId, string year, string month, Model.Log log = null);

        Client FindByDocumentCompany(long companyId, string document, Model.Log log = null);

        List<string> FindByContribuinte(long companyId, string type, Model.Log log = null);

        Client FindByRaiz(string raiz, Model.Log log = null);
    }
}
