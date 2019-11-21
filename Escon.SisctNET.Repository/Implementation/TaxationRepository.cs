using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxationRepository : Repository<Model.Taxation>, ITaxationRepository
    {
        private readonly ContextDataBase _context;

        public TaxationRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Taxation FindByCode(string code, Log log = null)
        {
            var rst = _context.Taxations.Where(_ => _.Code.Equals(code)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public Taxation FindByCode2(string code2, Log log = null)
        {
            var rst = _context.Taxations.Where(_ => _.Code2.Equals(code2)).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
