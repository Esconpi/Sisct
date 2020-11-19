using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IClientRepository : IRepository<Client>
    {
        Client FindByDocument(int document, Model.Log log = null);

        Client FindByName(string name, Model.Log log = null);

        List<Client> FindByCompany(int companyId, Model.Log log = null);

        List<Client> FindByCompany(int companyId, string year, string month, Model.Log log = null);

        Client FindByDocumentCompany(int companyId, string document, Model.Log log = null);

        List<string> FindByContribuinte(int companyId,string type, Model.Log log = null);

        Client FindByRaiz(string raiz, Model.Log log = null);

        void Create(List<Model.Client> clients, Model.Log log = null);

        void Update(List<Model.Client> clients, Model.Log log = null);

    }
}
