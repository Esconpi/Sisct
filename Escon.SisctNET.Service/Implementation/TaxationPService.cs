using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class TaxationPService : ITaxationPService
    {
        private readonly ITaxationPRepository _repository;

        public TaxationPService(ITaxationPRepository repository)
        {
            _repository = repository;
        }

        public TaxationP Create(TaxationP entity, Log log)
        {
            return _repository.Create(entity, log); 
        }

        public List<TaxationP> Create(List<TaxationP> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<TaxationP> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<TaxationP> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<TaxationP> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<TaxationP> FindByCompany(long companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);   
        }

        public List<TaxationP> FindByCompanyActive(long companyId, Log log = null)
        {
            return _repository.FindByCompanyActive(companyId, log);
        }

        public TaxationP FindById(long id, Log log)
        {
            return _repository.FindById(id, log);   
        }

        public TaxationP Update(TaxationP entity, Log log)
        {
            return _repository.Update(entity, log); 
        }

        public List<TaxationP> Update(List<TaxationP> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
