using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IClientRepository : IRepository<Client>
    {
        Client FindByDocument(int document, Model.Log log = null);

        Client FindByName(string name, Model.Log log = null);

        List<Client> FindByCompanyId(int companyId, Model.Log log = null);
    }
}
