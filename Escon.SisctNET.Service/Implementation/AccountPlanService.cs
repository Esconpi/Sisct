using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class AccountPlanService : IAccountPlanService
    {
        private readonly IAccountPlanRepository _repository;

        public AccountPlanService(IAccountPlanRepository repository)
        {
            _repository = repository;
        }

        public AccountPlan Create(AccountPlan entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<AccountPlan> Create(List<AccountPlan> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<AccountPlan> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<AccountPlan> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<AccountPlan> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<AccountPlan> FindByAccountTypeId(long id, Log log = null)
        {
            return _repository.FindByAccountTypeId(id, log);
        }

        public List<AccountPlan> FindByCompanyActive(long companyId, Log log = null)
        {
            return _repository.FindByCompanyActive(companyId, log);
        }

        public List<AccountPlan> FindByCompanyActive(string code, Log log = null)
        {
            return _repository.FindByCompanyActive(code, log);
        }

        public List<AccountPlan> FindByCompanyId(long companyId, Log log = null)
        {
            return _repository.FindByCompanyId(companyId, log);
        }

        public AccountPlan FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public AccountPlan Update(AccountPlan entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<AccountPlan> Update(List<AccountPlan> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
