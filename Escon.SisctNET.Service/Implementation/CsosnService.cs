using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class CsosnService : ICsosnService
    {
        private readonly ICsosnRepository _repository;

        public CsosnService(ICsosnRepository repository)
        {
            _repository = repository;
        }

        public Csosn Create(Csosn entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Csosn> Create(List<Csosn> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Csosn> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Csosn> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Csosn FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Csosn Update(Csosn entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Csosn> Update(List<Csosn> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
