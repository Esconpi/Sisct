using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxationPRepository : Repository<Model.TaxationP>, ITaxationPRepository
    {
        private readonly ContextDataBase _context;

        public TaxationPRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<TaxationP> FindByCompany(long companyId, Log log = null)
        {
            var rst = _context.TaxationPs
                            .Where(_ => _.CompanyId.Equals(companyId))
                            .Include(n => n.Ncm)
                            .ToList();
            AddLog(log);
            return rst;
        }

        public List<TaxationP> FindByCompanyActive(long companyId, Log log = null)
        {
            var rst = _context.TaxationPs
                              .Where(_ => _.CompanyId.Equals(companyId) && (Convert.ToDateTime(_.DateStart) < Convert.ToDateTime(_.DateEnd) || _.DateEnd.Equals(null)))
                              .Include(n => n.Ncm)
                              .ToList();
            AddLog(log);
            return rst;
        }
    }
}
