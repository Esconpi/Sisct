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

        public List<Provider> Create(List<Provider> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
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

        public List<Provider> FindByCompany(long companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public List<Provider> FindByCompany(long companyId, string year, string month, Log log = null)
        {
            return _repository.FindByCompany(companyId, year, month, log);
        }

        public List<string> FindByContribuinte(long companyId, string type, Log log = null)
        {
            return _repository.FindByContribuinte(companyId, type, log);
        }

        public Provider FindByDocument(string document, Log log = null)
        {
            return _repository.FindByDocument(document, log);
        }

        public Provider FindByDocumentCompany(long companyId, string document, Log log = null)
        {
            return _repository.FindByDocumentCompany(companyId, document, log);
        }

        public Provider FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Provider FindByName(string name, Log log = null)
        {
            return _repository.FindByName(name, log);
        }

        public Provider FindByRaiz(string raiz, Log log = null)
        {
            return _repository.FindByRaiz(raiz, log);
        }

        public Provider Update(Provider entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Provider> Update(List<Provider> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
