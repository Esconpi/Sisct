using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ITaxProducerService : IServiceBase<Model.TaxProducer>
    {
        List<Model.TaxProducer> FindByTaxs(long companyid, string month, string year, Model.Log log = null);
    }
}
