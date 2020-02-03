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

        public List<Client> FindByCompanyId(int companyId, Log log = null)
        {
            var result = _context.Clients.Where(_ => _.CompanyId.Equals(companyId)).ToList();
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
    }
}
