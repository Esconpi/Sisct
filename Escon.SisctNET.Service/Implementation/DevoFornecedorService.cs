using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class DevoFornecedorService : IDevoFornecedorService
    {
        private readonly IDevoFornecedorRepository _repository;

        public DevoFornecedorService(IDevoFornecedorRepository repository)
        {
            _repository = repository;
        }

        public DevoFornecedor Create(DevoFornecedor entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<DevoFornecedor> Create(List<DevoFornecedor> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<DevoFornecedor> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<DevoFornecedor> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<DevoFornecedor> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<DevoFornecedor> FindByDevoTax(long taxAnexo, Log log = null)
        {
            return _repository.FindByDevoTax(taxAnexo, log);
        }

        public DevoFornecedor FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public DevoFornecedor Update(DevoFornecedor entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<DevoFornecedor> Update(List<DevoFornecedor> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
