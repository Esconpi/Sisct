using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class InvoicingService : IInvoicingService
    {
        private readonly IInvoicingRepository _repository;

        public InvoicingService(IInvoicingRepository repository)
        {
            _repository = repository;

        }
        public Invoicing Create(Invoicing entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Invoicing> Create(List<Invoicing> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Invoicing> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Invoicing> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Invoicing> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Invoicing FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Invoicing Update(Invoicing entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Invoicing> Update(List<Invoicing> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
