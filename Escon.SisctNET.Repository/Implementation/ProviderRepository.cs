using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProviderRepository : Repository<Provider>, IProviderRepository
    {
        private readonly ContextDataBase _context;

        public ProviderRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<Provider> FindByCompanyId(int companyId, Log log = null)
        {
            var result = _context.Providers.Where(_ => _.CompanyId.Equals(companyId)).ToList();
            AddLog(log);
            return result;
        }

        public Provider FindByDocument(int document, Log log = null)
        {
            var result = _context.Providers.Where(_ => _.Document.Equals(document)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public Provider FindByName(string name, Log log = null)
        {
            var result = _context.Providers.Where(_ => _.Name.Equals(name)).FirstOrDefault();
            AddLog(log);
            return result;
        }
    }
}
