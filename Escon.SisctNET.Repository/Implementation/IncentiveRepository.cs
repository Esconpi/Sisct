using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class IncentiveRepository : Repository<Model.Incentive>, IIncentiveRepository
    {
        private readonly ContextDataBase _context;

        public IncentiveRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<Incentive> FindByCompany(long company, Log log = null)
        {
            var rst = _context.Incentives.Where(_ => _.CompanyId.Equals(company)).ToList();
            AddLog(log);
            return rst;
        }

        public List<Incentive> FindByPeriod(int days, Log log = null)
        {
            var rst = _context.Incentives
                .Where(_ => (_.DateEnd.Subtract(DateTime.Now)).Days <= days && _.Active.Equals(true))
                .Include(c => c.Company)
                .ToList();
            AddLog(log);
            return rst;
        }
    }
}
