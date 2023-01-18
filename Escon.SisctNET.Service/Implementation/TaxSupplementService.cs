using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class TaxSupplementService : ITaxSupplementService
    {
        private readonly ITaxSupplementRepository _repository;

        public TaxSupplementService(ITaxSupplementRepository repository)
        {
            _repository = repository;
        }

        public TaxSupplement Create(TaxSupplement entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<TaxSupplement> Create(List<TaxSupplement> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<TaxSupplement> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<TaxSupplement> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public TaxSupplement FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<TaxSupplement> FindByTaxSupplement(long taxAnexo, Log log = null)
        {
            return _repository.FindByTaxSupplement(taxAnexo, log);
        }

        public TaxSupplement Update(TaxSupplement entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<TaxSupplement> Update(List<TaxSupplement> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
