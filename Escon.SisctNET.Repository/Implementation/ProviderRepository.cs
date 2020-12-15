using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProviderRepository : Repository<Model.Provider>, IProviderRepository
    {
        private readonly ContextDataBase _context;

        public ProviderRepository(
            ContextDataBase context, IConfiguration configuration) 
            : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<Provider> providers, Log log = null)
        {
            foreach (var p in providers)
            {
                _context.Providers.Add(p);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<Provider> FindByCompany(int companyId, Log log = null)
        {
            var result = _context.Providers.Where(_ => _.CompanyId.Equals(companyId)).ToList();
            AddLog(log);
            return result;
        }

        public List<Provider> FindByCompany(int companyId, string year, string month, Log log = null)
        {
            var result = _context.Providers.Where(_ => _.CompanyId.Equals(companyId) && _.AnoRef.Equals(year) && _.MesRef.Equals(month)).ToList();
            AddLog(log);
            return result;
        }

        public List<string> FindByContribuinte(int companyId, string type, Log log = null)
        {
            List<string> result = null;
            if (type == "all")
            {
                result = _context.Providers.Where(_ => _.CompanyId.Equals(companyId) && _.TypeClientId.Equals(1)).Select(_ => _.Document).ToList();
            }
            else if (type == "raiz")
            {
                result = _context.Providers.Where(_ => _.CompanyId.Equals(companyId) && _.TypeClientId.Equals(1)).Select(_ => _.CnpjRaiz).Distinct().ToList();
            }

            AddLog(log);
            return result;
        }

        public Provider FindByDocument(int document, Log log = null)
        {
            var result = _context.Providers.Where(_ => _.Document.Equals(document)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public Provider FindByDocumentCompany(int companyId, string document, Log log = null)
        {
            var result = _context.Providers.Where(_ => _.CompanyId.Equals(companyId) && _.Document.Equals(document)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public Provider FindByName(string name, Log log = null)
        {
            var result = _context.Providers.Where(_ => _.Name.Equals(name)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public Provider FindByRaiz(string raiz, Log log = null)
        {
            var result = _context.Providers.Where(_ => _.CnpjRaiz.Equals(raiz)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public void Update(List<Provider> providers, Log log = null)
        {
            foreach (var p in providers)
            {
                _context.Providers.Update(p);
            }

            AddLog(log);
            _context.SaveChanges();
        }
    }
}
