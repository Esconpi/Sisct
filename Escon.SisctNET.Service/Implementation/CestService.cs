using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class CestService : ICestService
    {
        private readonly ICestRepository _repository;

        public CestService(ICestRepository repository)
        {
            _repository = repository;
        }

        public Cest Create(Cest entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Cest> Create(List<Cest> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Cest> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Cest> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Cest FindByCode(string code, Log log = null)
        {
            return _repository.FindByCode(code);
        }

        public Cest FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Cest Update(Cest entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Cest> Update(List<Cest> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
