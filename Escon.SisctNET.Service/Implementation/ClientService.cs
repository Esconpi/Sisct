using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _repository;

        public ClientService(IClientRepository repository)
        {
            _repository = repository;
        }

        public Client Create(Client entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Client> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Client> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Client> FindByCompanyId(int companyId, Log log = null)
        {
            return _repository.FindByCompanyId(companyId, log);
        }

        public List<string> FindByContribuinte(int companyId, string type, Log log = null)
        {
            return _repository.FindByContribuinte(companyId, type, log);
        }

        public Client FindByDocument(int document, Log log = null)
        {
            return _repository.FindByDocument(document, log);
        }

        public Client FindByDocumentCompany(int companyId, string document, Log log = null)
        {
            return _repository.FindByDocumentCompany(companyId, document, log);
        }

        public Client FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Client FindByName(string name, Log log = null)
        {
            return _repository.FindByName(name, log);
        }

        public Client FindByRaiz(string raiz, Log log = null)
        {
            return _repository.FindByRaiz(raiz, log);
        }

        public Client Update(Client entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
