using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ITaxationPRepository : IRepository<Model.TaxationP>
    {
        List<Model.TaxationP> FindByCompany(long companyId, Model.Log log = null);

        List<Model.TaxationP> FindByCompanyActive(long companyId, Model.Log log = null);
    }
}
