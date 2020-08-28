using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ICompanyCfopRepository : IRepository<Model.CompanyCfop>
    {
        void Create(List<Model.CompanyCfop> cfopCompanies, Model.Log log = null);

        List<CompanyCfop> FindByCfopActive(int companyId, string type, string typeCfop, Log log = null);

        Model.CompanyCfop FindByCompanyCfop(int companyId, int cfopId, Model.Log log = null);

        List<Model.CompanyCfop> FindByCompany(int companyId, Log log = null);

        List<Model.CompanyCfop> FindByCfopActive(string company, string type, Log log = null);
    }
}
