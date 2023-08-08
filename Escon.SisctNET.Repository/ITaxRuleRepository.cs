using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ITaxRuleRepository : IRepository<Model.TaxRule>
    {
        List<Model.TaxRule> FindByCompany(long companyId, Model.Log log = null);
    }
}
