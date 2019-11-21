using System.Collections.Generic;
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

        public void Create(List<Cest> cests, Log log = null)
        {
            foreach (var c in cests)
            {
                _context.Cests.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public Cest FindByCode(string code, Log log = null)
        {
            var rst = _context.Cests.Where(_ => _.Code.Equals(code)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

    }
}
