using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ITaxRuleService : IServiceBase<Model.TaxRule>
    {
        List<Model.TaxRule> FindByCompany(long companyId, Model.Log log = null);
    }
}
