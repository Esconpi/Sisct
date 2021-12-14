using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ITaxSupplementService : IServiceBase<Model.TaxSupplement>
    {
        List<Model.TaxSupplement> FindByTaxSupplement(long taxAnexo, Model.Log log = null);
    }
}
