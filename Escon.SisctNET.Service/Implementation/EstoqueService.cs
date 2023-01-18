using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class EstoqueService : IEstoqueService
    {
        private readonly IEstoqueRepository _repository;

        public EstoqueService(IEstoqueRepository repository)
        {
            _repository = repository;
        }

        public Estoque Create(Estoque entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Estoque> Create(List<Estoque> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Estoque> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Estoque> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Estoque> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Estoque> FindByCompany(long company, Log log = null)
        {
            return _repository.FindByCompany(company, log);
        }

        public Estoque FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Estoque Update(Estoque entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Estoque> Update(List<Estoque> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
