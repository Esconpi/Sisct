using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class BaseService : IBaseService
    {
        private readonly IBaseRepository _repository;

        public BaseService(IBaseRepository repository)
        {
            _repository = repository;
        }


        public Base Create(Base entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Base> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Base> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Base FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Base FindByName(string name, Log log = null)
        {
            return _repository.FindByName(name, log);
        }

        public Base Update(Base entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
