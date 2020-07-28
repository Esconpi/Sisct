using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CreditBalanceRepository : Repository<CreditBalance>, ICreditBalanceRepository
    {
        private readonly ContextDataBase _context;

        public CreditBalanceRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public CreditBalance FindByLastMonth(int companyid, string month, string year, Log log = null)
        {
            AddLog(log);
            return _context.CreditBalances.Where(_ => _.CompanyId.Equals(companyid) && _.MesRef.Equals(month) && _.AnoRef.Equals(year)).FirstOrDefault();
        }
    }
}
