using System;
using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class TaxationService : ITaxationService
    {
        private readonly ITaxationRepository _repository;

        public TaxationService(ITaxationRepository repository)
        {
            _repository = repository;
        }

        public Taxation Create(Taxation entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Taxation> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Taxation> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Taxation FindByCode(string code, string cest, DateTime data, Log log = null)
        {
            return _repository.FindByCode(code, cest, data, log);
        }

        public Taxation FindByCode(List<Taxation> taxations, string code, string cest, DateTime data, Log log = null)
        {
            return _repository.FindByCode(taxations, code, cest, data, log);
        }

        public List<Taxation> FindByCompany(long companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public List<Taxation> FindByCompanyActive(long companyId, Log log = null)
        {
            return _repository.FindByCompanyActive(companyId, log);
        }

        public Taxation FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Taxation FindByNcm(string code, string cest, Log log = null)
        {
            return _repository.FindByNcm(code, cest, log);
        }

        public Taxation Update(Taxation entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
