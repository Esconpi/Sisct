using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service
{
    public interface ITaxProducerService : IServiceBase<Model.TaxProducer>
    {
        Task CreateRange(List<Model.TaxProducer> taxProducers, Model.Log log = null);

        Task UpdateRange(List<Model.TaxProducer> taxProducers, Model.Log log = null);

        List<Model.TaxProducer> FindByTaxs(long companyid, string month, string year, Model.Log log = null);
    }
}
