using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class NatReceitaService : INatReceitaService
    {
        private readonly INatReceitaRepository _repository;

        public NatReceitaService(INatReceitaRepository repository)
        {
            _repository = repository;

        }
        public NatReceita Create(NatReceita entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<NatReceita> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<NatReceita> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public NatReceita FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public NatReceita Update(NatReceita entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
