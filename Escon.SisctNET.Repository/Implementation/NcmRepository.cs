using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class NcmRepository : Repository<Ncm>, INcmRepository
    {
        private readonly ContextDataBase _context;

        public NcmRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Ncm FindByCode(string code, Log log = null)
        {
            var rst = _context.Ncms.Where(_ => _.Code.Trim().Equals(code)).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
