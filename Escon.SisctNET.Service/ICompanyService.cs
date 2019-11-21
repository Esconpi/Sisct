using System.Collections.Generic;
using Escon.SisctNET.Model;

namespace Escon.SisctNET.Service
{
    public interface ICompanyService : IServiceBase<Model.Company>
    {
        void Create(List<Model.Company> companies, Model.Log log = null);

        Model.Company FindByCode(string code, Model.Log log = null);

        Model.Company FindByDocument(string document, Model.Log log = null);

        List<Model.Company> FindByCompanies(Model.Log log = null);

    }
}