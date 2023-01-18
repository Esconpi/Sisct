using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxationTypeRepository : Repository<TaxationType>, ITaxationTypeRepository
    {
        private readonly ContextDataBase _context;

        public TaxationTypeRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public TaxationType FindByDescription(string description, Log log = null)
        {
            var rst = _context.Taxationtypes.Where(_ => _.Description.Equals(description)).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
