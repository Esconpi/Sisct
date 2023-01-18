using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class ProfileService : IProfileService
    {
        IRepository<Profile> _repository;

        public ProfileService(IRepository<Profile> repository)
        {
            _repository = repository;
        }

        public Profile Create(Profile entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Profile> Create(List<Profile> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Profile> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Profile> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Profile> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Profile FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Profile Update(Profile entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Profile> Update(List<Profile> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
