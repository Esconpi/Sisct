using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class AccountPlanTypeService : IAccountPlanTypeService
    {
        private readonly IRepository<Model.AccountPlanType> _repository;

        public AccountPlanTypeService(IRepository<Model.AccountPlanType> repository)
        {
            _repository = repository;
        }

        public Model.AccountPlanType Create(Model.AccountPlanType entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }


        public List<Model.AccountPlanType> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Model.AccountPlanType> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Model.AccountPlanType FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Model.AccountPlanType Update(Model.AccountPlanType entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
