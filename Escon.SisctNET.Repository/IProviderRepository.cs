using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProviderRepository : IRepository<Model.Provider>
    {
        Provider FindByDocument(string document, Model.Log log = null);

        Provider FindByName(string name, Model.Log log = null);

        List<Provider> FindByCompany(long companyId, Model.Log log = null);

        List<Provider> FindByCompany(long companyId, string year, string month, Model.Log log = null);

        Provider FindByDocumentCompany(long companyId, string document, Model.Log log = null);

        List<string> FindByContribuinte(long companyId, string type, Model.Log log = null);

        Provider FindByRaiz(string raiz, Model.Log log = null);

        void Create(List<Model.Provider> providers, Model.Log log = null);

        void Update(List<Model.Provider> providers, Model.Log log = null);
    }
}
