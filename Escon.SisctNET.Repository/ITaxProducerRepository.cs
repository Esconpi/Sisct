using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ITaxProducerRepository : IRepository<Model.TaxProducer>
    {
        List<Model.TaxProducer> FindByTaxs(int companyid, string month, string year, Model.Log log = null);

        void Create(List<Model.TaxProducer> taxProducers, Model.Log log = null);

        void Update(List<Model.TaxProducer> taxProducers, Model.Log log = null);
    }
}
