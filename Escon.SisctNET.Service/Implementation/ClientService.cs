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

        public void Create(List<Client> clients, Log log = null)
        {
            _repository.Create(clients, log);
        }

        public void Delete(long id, Log log)
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

        public List<Client> FindByCompany(long companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public List<Client> FindByCompany(long companyId, string year, string month, Log log = null)
        {
            return _repository.FindByCompany(companyId, year, month, log);
        }

        public List<string> FindByContribuinte(long companyId, string type, Log log = null)
        {
            return _repository.FindByContribuinte(companyId, type, log);
        }

        public Client FindByDocument(string document, Log log = null)
        {
            return _repository.FindByDocument(document, log);
        }

        public Client FindByDocumentCompany(long companyId, string document, Log log = null)
        {
            return _repository.FindByDocumentCompany(companyId, document, log);
        }

        public Client FindById(long id, Log log)
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

        public void Update(List<Client> clients, Log log = null)
        {
            _repository.Update(clients, log);
        }
    }
}
