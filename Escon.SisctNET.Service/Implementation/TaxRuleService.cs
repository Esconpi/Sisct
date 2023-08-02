using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class TaxRuleService : ITaxRuleService
    {
        private readonly ITaxRuleRepository _repository;

        public TaxRuleService(ITaxRuleRepository repository)
        {
            _repository = repository;
        }

        public TaxRule Create(TaxRule entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<TaxRule> Create(List<TaxRule> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<TaxRule> entities, Log log)
        {
            _repository.Delete(entities, log);   
        }

        public List<TaxRule> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<TaxRule> FindAll(int page, int countrow, Log log)
        {
           return _repository.FindAll(page, countrow, log);   
        }

        public TaxRule FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public TaxRule Update(TaxRule entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<TaxRule> Update(List<TaxRule> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
