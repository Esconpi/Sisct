using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class AccountPlanRepository : Repository<Model.AccountPlan>, IAccountPlanRepository
    {
        private readonly ContextDataBase _context;

        public AccountPlanRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<AccountPlan> FindByAccountTypeId(long id, Log log = null)
        {
            var result = _context.AccountPlans.Where(a => a.AccountPlanTypeId.Equals(id)).ToList();
            AddLog(log);
            return result;
        }

        public List<AccountPlan> FindByCompanyActive(long companyId, Log log = null)
        {
            List<AccountPlan> result = _context.AccountPlans.Include(t => t.AccountPlanType).Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true)).ToList();
            AddLog(log);
            return result;
        }

        public List<AccountPlan> FindByCompanyActive(string code, Log log = null)
        {
            List<AccountPlan> result = _context.AccountPlans.Include(t => t.AccountPlanType).Where(_ => _.Company.Code.Equals(code) && _.Active.Equals(true)).ToList();
            AddLog(log);
            return result;
        }

        public List<AccountPlan> FindByCompanyId(long companyId, Model.Log log = null)
        {
            List<AccountPlan> result = _context.AccountPlans.Include(t => t.AccountPlanType).Where(_ => _.CompanyId.Equals(companyId)).ToList();
            AddLog(log);
            return result;
        }

    }
}
