using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class TaxationTypeNcmService : ITaxationTypeNcmService
    {
        private readonly ITaxationTypeNcmRepository _repository;

        public TaxationTypeNcmService(ITaxationTypeNcmRepository repository)
        {
            _repository = repository;
        }

        public TaxationTypeNcm Create(TaxationTypeNcm entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<TaxationTypeNcm> Create(List<TaxationTypeNcm> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<TaxationTypeNcm> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<TaxationTypeNcm> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<TaxationTypeNcm> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public TaxationTypeNcm FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public TaxationTypeNcm Update(TaxationTypeNcm entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<TaxationTypeNcm> Update(List<TaxationTypeNcm> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
