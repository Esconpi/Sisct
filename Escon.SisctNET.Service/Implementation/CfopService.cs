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
     
        public void Create(List<Cfop> cfops, Log log = null)
        {
            _repository.Create(cfops);
        }

        public Cfop Create(Cfop entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
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

        public Cfop FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Cfop Update(Cfop entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
