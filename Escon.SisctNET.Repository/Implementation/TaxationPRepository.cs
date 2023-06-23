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

        public TaxationP FindByCode(List<TaxationP> taxations, string code, string cest, DateTime data, Log log = null)
        {
            var codes = taxations.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest)).ToList();

            var result = codes.Where(_ => (DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) ||
                                          (DateTime.Compare(_.DateStart, data) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) > 0))
                                .FirstOrDefault();

            AddLog(log);
            return result;
        }

        public List<TaxationP> FindByCompany(long companyId, Log log = null)
        {
            var rst = _context.TaxationsP
                            .Where(_ => _.CompanyId.Equals(companyId))
                            .Include(n => n.Ncm)
                            .ToList();
            AddLog(log);
            return rst;
        }

        public List<TaxationP> FindByCompanyActive(long companyId, Log log = null)
        {
            var rst = _context.TaxationsP
                              .Where(_ => _.CompanyId.Equals(companyId) && (Convert.ToDateTime(_.DateStart) < Convert.ToDateTime(_.DateEnd) || _.DateEnd.Equals(null)))
                              .Include(n => n.Ncm)
                              .ToList();
            AddLog(log);
            return rst;
        }

        public TaxationP FindByNcm(string code, string cest, Log log = null)
        {
            var codes = _context.TaxationsP.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest)).ToList();

            var rst = codes.Where(_ => _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
