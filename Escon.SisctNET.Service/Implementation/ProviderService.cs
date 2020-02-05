using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _repository;

        public ProviderService(IProviderRepository repository)
        {
            _repository = repository;
        }

        public Provider Create(Provider entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Provider> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Provider> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Provider> FindByCompanyId(int companyId, Log log = null)
        {
            return _repository.FindByCompanyId(companyId, log);
        }

        public Provider FindByDocument(int document, Log log = null)
        {
            return _repository.FindByDocument(document, log);
        }

        public Provider FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Provider FindByName(string name, Log log = null)
        {
            return _repository.FindByName(name, log);
        }

        public Provider Update(Provider entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
