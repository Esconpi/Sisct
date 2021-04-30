using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class AccessService : IAccessService
    {
        private readonly IAccessRepository _repository;

        public AccessService(IAccessRepository repository)
        {
            _repository = repository;
        }

        public Access Create(Access entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Access> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Access> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Access> FindByFunctionalityId(long functionalityId, Log log = null)
        {
            return _repository.FindByFunctionalityId(functionalityId, log);
        }

        public List<Access> FindByProfileId(long profileId, Log log = null)
        {
            return _repository.FindByProfileId(profileId, log);
        }

        public List<Access> FindByActive(long profileId, Log log = null)
        {
            return _repository.FindByActive(profileId, log);
        }

        public Access FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Access Update(Access entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
