using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class IncentiveService : IIncentiveService
    {
        private readonly IIncentiveRepository _repository;

        public IncentiveService(IIncentiveRepository repository)
        {
            _repository = repository;
        }

        public Incentive Create(Incentive entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Incentive> Create(List<Incentive> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Incentive> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Incentive> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Incentive> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Incentive> FindByCompany(long company, Log log = null)
        {
            return _repository.FindByCompany(company, log);
        }

        public Incentive FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<Incentive> FindByPeriod(int days, Log log = null)
        {
            return _repository.FindByPeriod(days, log);
        }

        public Incentive Update(Incentive entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Incentive> Update(List<Incentive> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
