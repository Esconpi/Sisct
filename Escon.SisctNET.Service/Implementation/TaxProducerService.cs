using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service.Implementation
{
    public class TaxProducerService : ITaxProducerService
    {
        private readonly ITaxProducerRepository _repository;

        public TaxProducerService(ITaxProducerRepository repository)
        {
            _repository = repository;
        }

        public TaxProducer Create(TaxProducer entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<TaxProducer> Create(List<TaxProducer> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public async Task CreateRange(List<TaxProducer> taxProducers, Log log = null)
        {
            await _repository.CreateRange(taxProducers, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<TaxProducer> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<TaxProducer> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public TaxProducer FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<TaxProducer> FindByTaxs(long companyid, string month, string year, Log log = null)
        {
            return _repository.FindByTaxs(companyid, month, year, log);
        }

        public TaxProducer Update(TaxProducer entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<TaxProducer> Update(List<TaxProducer> entities, Log log)
        {
            return _repository.Update(entities, log);
        }

        public async Task UpdateRange(List<TaxProducer> taxProducers, Log log = null)
        {
            await _repository.UpdateRange(taxProducers, log);
        }
    }
}
