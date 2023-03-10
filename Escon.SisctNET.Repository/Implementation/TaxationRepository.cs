using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxationRepository : Repository<Model.Taxation>, ITaxationRepository
    {
        private readonly ContextDataBase _context;
        

        public TaxationRepository(ContextDataBase context, IConfiguration configuration) 
            : base(context, configuration)
        {
            _context = context;
        }

        public Taxation FindByCode(string code, string cest, DateTime data, Log log = null)
        {
            var codes = _context.Taxations.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest)).ToList();

            var result = codes.Where(_ =>  (DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) ||
                                           (DateTime.Compare(_.DateStart, data) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) > 0))
                               .FirstOrDefault();

            AddLog(log);
            return result;
        }

        public Taxation FindByCode(List<Taxation> taxations, string code, string cest, DateTime data, Log log = null)
        {
            var codes = taxations.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest)).ToList();

            var result = codes.Where(_ => (DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) ||
                                          (DateTime.Compare(_.DateStart, data) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) > 0))
                                .FirstOrDefault();

            AddLog(log);
            return result;
        }

        public List<Taxation> FindByCompany(long companyId, Log log = null)
        {
            var rst = _context.Taxations
               .Where(_ => _.CompanyId.Equals(companyId))
               .Include(n => n.Ncm)
               .ToList();
            AddLog(log);
            return rst;
        }

        public List<Taxation> FindByCompanyActive(long companyId, Log log = null)
        {
            var rst = _context.Taxations
                .Where(_ => _.CompanyId.Equals(companyId) && (Convert.ToDateTime(_.DateStart) < Convert.ToDateTime(_.DateEnd) || _.DateEnd.Equals(null)))
                .Include(n => n.Ncm)
                .ToList();
            AddLog(log);
            return rst;
        }

        public Taxation FindByNcm(string code, string cest, Log log = null)
        {
            var rst = _context.Taxations.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
