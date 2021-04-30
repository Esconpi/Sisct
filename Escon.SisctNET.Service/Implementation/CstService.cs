using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class CstService : ICstService
    {
        private readonly ICstRepository _repository;

        public CstService(ICstRepository repository)
        {
            _repository = repository;
        }

        public Cst Create(Cst entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Cst> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Cst> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Cst FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<Cst> FindByIdent(bool identicador, Log log = null)
        {
            return _repository.FindByIdent(identicador, log);
        }

        public Cst Update(Cst entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
