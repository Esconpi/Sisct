using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class SuspensionRepository : Repository<Model.Suspension>, ISuspensionRepository
    {
        private readonly ContextDataBase _context;

        public SuspensionRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<Suspension> FindByCompany(int company, Log log = null)
        {
            var rst = _context.Suspensions.Where(_ => _.CompanyId.Equals(company)).ToList();
            AddLog(log);
            return rst;
        }
    }
}
