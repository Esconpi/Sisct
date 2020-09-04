using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ICompanyCfopService : IServiceBase<Model.CompanyCfop>
    {
        void Create(List<Model.CompanyCfop> cfopCompanies, Model.Log log = null);

        List<Model.CompanyCfop> FindByCompany(int companyId, Log log = null);

        Model.CompanyCfop FindByCompanyCfop(int companyId, int cfopId, Log log = null);

        List<CompanyCfop> FindByCfopActive(int companyId, string type, string typeCfop, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevolucao(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopVendas(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopBonificacao(string company, Log log = null);

    }
}
