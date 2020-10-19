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

        public void Delete(int id, Log log)
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

        public DevoCliente FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public DevoCliente Update(DevoCliente entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
