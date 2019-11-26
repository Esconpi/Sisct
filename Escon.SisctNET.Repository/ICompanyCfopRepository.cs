
using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ICompanyCfopRepository : IRepository<Model.CompanyCfop>
    {
        Model.CompanyCfop FindByCompanyCfop(int companyId, int cfopId , Model.Log log = null);

        List<Model.CompanyCfop> FindByCompany(int companyId, Log log = null);
    }
}
