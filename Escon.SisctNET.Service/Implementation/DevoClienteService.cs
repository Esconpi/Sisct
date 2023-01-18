using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class DevoClienteService : IDevoClienteService
    {
        private readonly IDevoClienteRepository _repository;

        public DevoClienteService(IDevoClienteRepository repository)
        {
            _repository = repository;
        }

        public DevoCliente Create(DevoCliente entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<DevoCliente> Create(List<DevoCliente> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<DevoCliente> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<DevoCliente> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<DevoCliente> FindByDevoTax(long taxAnexo, Log log = null)
        {
            return _repository.FindByDevoTax(taxAnexo, log);
        }

        public DevoCliente FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public DevoCliente Update(DevoCliente entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<DevoCliente> Update(List<DevoCliente> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
