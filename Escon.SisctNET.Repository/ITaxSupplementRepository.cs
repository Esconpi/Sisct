using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ITaxSupplementRepository : IRepository<Model.TaxSupplement>
    {
        List<Model.TaxSupplement> FindByTaxSupplement(long taxAnexo, Model.Log log = null);
    }
}
