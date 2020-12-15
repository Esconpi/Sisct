using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProviderRepository : IRepository<Model.Provider>
    {
        Provider FindByDocument(int document, Model.Log log = null);

        Provider FindByName(string name, Model.Log log = null);

        List<Provider> FindByCompany(int companyId, Model.Log log = null);

        List<Provider> FindByCompany(int companyId, string year, string month, Model.Log log = null);

        Provider FindByDocumentCompany(int companyId, string document, Model.Log log = null);

        List<string> FindByContribuinte(int companyId, string type, Model.Log log = null);

        Provider FindByRaiz(string raiz, Model.Log log = null);

        void Create(List<Model.Provider> providers, Model.Log log = null);

        void Update(List<Model.Provider> providers, Model.Log log = null);
    }
}
