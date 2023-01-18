using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CestRepository : Repository<Cest>, ICestRepository
    {
        private readonly ContextDataBase _context;

        public CestRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Cest FindByCode(string code, Log log = null)
        {
            var rst = _context.Cests.Where(_ => _.Code.Equals(code)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

    }
}
