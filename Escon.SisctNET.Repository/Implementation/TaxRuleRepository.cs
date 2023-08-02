using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxRuleRepository : Repository<Model.TaxRule>, ITaxRuleRepository
    {
        private readonly ContextDataBase _context;

        public TaxRuleRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
