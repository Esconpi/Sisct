using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        private readonly ContextDataBase _context;

        public ClientRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<Client> clients, Log log = null)
        {
            foreach(var c in clients)
            {
                _context.Clients.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<Client> FindByCompanyId(int companyId, Log log = null)
        {
            var result = _context.Clients.Where(_ => _.CompanyId.Equals(companyId)).ToList();
            AddLog(log);
            return result;
        }

        public List<string> FindByContribuinte(int companyId, string type, Log log = null)
        {
            List<string> result = null;
            if (type == "all")
            {
                result = _context.Clients.Where(_ => _.CompanyId.Equals(companyId) && _.TypeClientId.Equals(1)).Select(_ => _.Document).ToList();
            }
            else if (type == "raiz")
            {
                result = _context.Clients.Where(_ => _.CompanyId.Equals(companyId) && _.TypeClientId.Equals(1)).Select(_ => _.CnpjRaiz).Distinct().ToList();
            }
            
            AddLog(log);
            return result;
        }

        public Client FindByDocument(int document, Log log = null)
        {
            var result = _context.Clients.Where(_ => _.Document.Equals(document)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public Client FindByDocumentCompany(int companyId, string document, Log log = null)
        {
            var result = _context.Clients.Where(_ => _.CompanyId.Equals(companyId) && _.Document.Equals(document)).FirstOrDefault();
            AddLog(log);
            return result;
        }


        public Client FindByName(string name, Log log = null)
        {
            var result = _context.Clients.Where(_ => _.Name.Equals(name)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public Client FindByRaiz(string raiz, Log log = null)
        {
            var result = _context.Clients.Where(_ => _.CnpjRaiz.Equals(raiz)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public void Update(List<Client> clients, Log log = null)
        {
            foreach (var c in clients)
            {
                _context.Clients.Update(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }
    }
}
