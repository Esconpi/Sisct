using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class CreditBalanceService : ICreditBalanceService
    {
        private readonly ICreditBalanceRepository _repository;

        public CreditBalanceService(ICreditBalanceRepository repository)
        {
            _repository = repository;
        }

        public CreditBalance Create(CreditBalance entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<CreditBalance> Create(List<CreditBalance> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<CreditBalance> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<CreditBalance> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public CreditBalance FindByCurrentMonth(long companyid, string month, string year, Log log = null)
        {
            return _repository.FindByCurrentMonth(companyid, month, year, log);
        }

        public CreditBalance FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public CreditBalance FindByLastMonth(long companyid, string month, string year, Log log = null)
        {
            return _repository.FindByLastMonth(companyid, month, year, log);
        }

        public CreditBalance Update(CreditBalance entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<CreditBalance> Update(List<CreditBalance> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
