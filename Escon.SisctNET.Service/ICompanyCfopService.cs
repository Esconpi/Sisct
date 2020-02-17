using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ICompanyCfopService : IServiceBase<Model.CompanyCfop>
    {
        List<Model.CompanyCfop> FindByCompany(int companyId, Log log = null);

        Model.CompanyCfop FindByCompanyCfop(int companyId, int cfopId, Log log = null);

        List<CompanyCfop> FindByCfopActive(int companyId, string type, Log log = null);

    }
}
