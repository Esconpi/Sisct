using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CfopRepository : Repository<Cfop>, ICfopRepository
    {
        private readonly ContextDataBase _context;

        public CfopRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<Cfop> cfops, Log log = null)
        {
            foreach (var c in cfops)
            {
                _context.Cfops.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public Cfop FindByCode(string code, Log log = null)
        {
            var rst = _context.Cfops.Where(_ => _.Code.Equals(code)).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
