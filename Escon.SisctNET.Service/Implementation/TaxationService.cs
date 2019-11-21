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

        public void Delete(int id, Log log)
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

        public Taxation FindByCode(string code, Log log = null)
        {
            return _repository.FindByCode(code, log);
        }

        public Taxation FindByCode2(string code2, Log log = null)
        {
            return _repository.FindByCode2(code2, log);
        }

        public Taxation FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Taxation Update(Taxation entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
