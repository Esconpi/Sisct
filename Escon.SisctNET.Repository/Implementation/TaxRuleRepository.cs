using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxRuleRepository : Repository<Model.TaxRule>, ITaxRuleRepository
    {
        private readonly ContextDataBase _context;

        public TaxRuleRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<TaxRule> FindByCompany(long companyId, Log log = null)
        {
            var rst = _context.TaxRules
                          .Where(_ => _.CompanyId.Equals(companyId))
                          .Include(t => t.TaxationTypeNcm)
                          .ToList();
            AddLog(log);
            return rst;
        }
    }
}
