using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class AccountPlanTypeGroupService : IAccountPlanTypeGroupService
    {
        private readonly IRepository<Model.AccountPlanTypeGroup> _repository;

        public AccountPlanTypeGroupService(IRepository<Model.AccountPlanTypeGroup> repository)
        {
            _repository = repository;
        }

        public AccountPlanTypeGroup Create(AccountPlanTypeGroup entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<AccountPlanTypeGroup> Create(List<AccountPlanTypeGroup> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<AccountPlanTypeGroup> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<AccountPlanTypeGroup> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public AccountPlanTypeGroup FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public AccountPlanTypeGroup Update(AccountPlanTypeGroup entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<AccountPlanTypeGroup> Update(List<AccountPlanTypeGroup> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
