using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class TaxService : ITaxService
    {
        private readonly ITaxRepository _repository;

        public TaxService(ITaxRepository repository)
        {
            _repository = repository;
        }

        public Tax Create(Tax entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Tax> Create(List<Tax> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Tax> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Tax> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Tax> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Tax FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Tax FindByMonth(long company, string mes, string ano, Log log = null)
        {
            return _repository.FindByMonth(company, mes, ano, log);
        }

        public Tax FindByMonth(long company, string mes, string ano, string type, Log log = null)
        {
            return _repository.FindByMonth(company, mes, ano, type, log);
        }

        public Tax Update(Tax entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Tax> Update(List<Tax> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
