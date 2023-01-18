using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class CfopService : ICfopService
    {
        private readonly ICfopRepository _repository;

        public CfopService(ICfopRepository repository)
        {
            _repository = repository;
        }
     
        public Cfop Create(Cfop entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Cfop> Create(List<Cfop> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Cfop> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Cfop> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Cfop> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Cfop FindByCode(string code, Log log = null)
        {
            return _repository.FindByCode(code);
        }

        public Cfop FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Cfop Update(Cfop entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Cfop> Update(List<Cfop> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
