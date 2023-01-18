using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
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

        public List<Client> FindByCompany(long companyId, Log log = null)
        {
            var result = _context.Clients.Where(_ => _.CompanyId.Equals(companyId)).ToList();
            AddLog(log);
            return result;
        }

        public List<Client> FindByCompany(long companyId, string year, string month, Log log = null)
        {
            var result = _context.Clients.Where(_ => _.CompanyId.Equals(companyId) && _.AnoRef.Equals(year) && _.MesRef.Equals(month)).ToList();
            AddLog(log);
            return result;
        }

        public List<string> FindByContribuinte(long companyId, string type, Log log = null)
        {
            List<string> result = null;
            if (type == "all")
            {
                result = _context.Clients.Where(_ => _.CompanyId.Equals(companyId) && _.TypeClientId.Equals((long)1)).Select(_ => _.Document).ToList();
            }
            else if (type == "raiz")
            {
                result = _context.Clients.Where(_ => _.CompanyId.Equals(companyId) && _.TypeClientId.Equals((long)1)).Select(_ => _.CnpjRaiz).Distinct().ToList();
            }
            
            AddLog(log);
            return result;
        }

        public Client FindByDocument(string document, Log log = null)
        {
            var result = _context.Clients.Where(_ => _.Document.Equals(document)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public Client FindByDocumentCompany(long companyId, string document, Log log = null)
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
    }
}
