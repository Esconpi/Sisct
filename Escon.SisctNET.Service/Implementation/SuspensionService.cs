using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class SuspensionService : ISuspensionService
    {
        private readonly ISuspensionRepository _repository;

        public SuspensionService(ISuspensionRepository repository)
        {
            _repository = repository;
        }

        public Suspension Create(Suspension entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Suspension> Create(List<Suspension> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Suspension> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Suspension> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Suspension> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Suspension> FindByCompany(long company, Log log = null)
        {
            return _repository.FindByCompany(company, log);
        }

        public Suspension FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Suspension Update(Suspension entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Suspension> Update(List<Suspension> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
