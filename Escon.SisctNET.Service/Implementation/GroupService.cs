using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _repository;

        public GroupService(IGroupRepository repository)
        {
            _repository = repository;
        }

        public void Create(List<Group> groups, Log log = null)
        {
            _repository.Create(groups);
        }

        public Group Create(Group entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Group> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Group> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Group FindByDescription(string description, Log log = null)
        {
            return _repository.FindByDescription(description);
        }

        public Group FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Group Update(Group entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
