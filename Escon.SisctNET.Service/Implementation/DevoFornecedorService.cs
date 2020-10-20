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

        public void Create(List<DevoFornecedor> devoFornecedors, Log log = null)
        {
            _repository.Create(devoFornecedors, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<DevoFornecedor> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<DevoFornecedor> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<DevoFornecedor> FindByDevoTax(int taxAnexo, Log log = null)
        {
            return _repository.FindByDevoTax(taxAnexo, log);
        }

        public DevoFornecedor FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public DevoFornecedor Update(DevoFornecedor entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public void Update(List<DevoFornecedor> devoFornecedors, Log log = null)
        {
            _repository.Update(devoFornecedors, log);
        }
    }
}
