using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository.Implementation
{
    public class DarDocumentRepository : Repository<DarDocument>, IDarDocumentRepository
    {
        private readonly ContextDataBase _context;

        public DarDocumentRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public async Task<List<DarDocumentCompany>> FindByPeriodReferenceAsync(int periodReference, int? companyid)
        {
            List<object> parameters = new List<object>();
            parameters.Add(periodReference);

            var query = "select DISTINCT company.id,company.document, company.socialname, dardocument.periodreference, dar.code darcode, dar.description dardescription"
                    + "  from company "
                    + "  left join dardocument on dardocument.companyid = company.id "
                    + "  left join dar on dar.id = dardocument.darid "
                    + " WHERE dardocument.periodreference = {0} OR dardocument.periodreference IS NULL "
                    + "  and company.active = 1 ";

            if (companyid != null)
            {
                query += " and company.id = {1}";
                parameters.Add(companyid.Value);
            }

            query += " ORDER BY company.socialname";

            var result = await _context.DarDocumentCompanies.FromSql(query, parameters.ToArray()).ToListAsync();
            return result;
        }

        public async Task<DarDocument> GetByCompanyAndPeriodReferenceAndDarAsync(int companyid, int period, int darId) =>
            await _context.DarDocuments
            .Include(x => x.Company)
            .Include(x => x.Dar)
            .LastOrDefaultAsync(x => x.CompanyId.Equals(companyid) && x.PeriodReference.Equals(period) && x.DarId.Equals(darId));

        public async Task<List<DarDocument>> GetByCompanyAndPeriodReferenceAsync(int companyid, int period, bool canceled) =>
            await _context.DarDocuments
            .Include(x => x.Company)
            .Include(x => x.Dar)
            .Where(x => x.CompanyId.Equals(companyid) && x.PeriodReference.Equals(period) && x.Canceled.Equals(canceled))
            .ToListAsync();

        public async Task<List<DarDocument>> GetByCompanyIdAsync(int id) =>
            await _context.DarDocuments
            .Include(x => x.Company)
            .Include(x => x.Dar)
            .Where(x => x.CompanyId.Equals(id)).ToListAsync();

        public async Task<List<int>> GetPeriodsReferenceAsync() =>
            await _context.DarDocuments.GroupBy(x => x.PeriodReference).Select(x => x.Key).ToListAsync();

        public async Task<List<DarDocument>> ListFull() =>
            await _context.DarDocuments
                .Include(x => x.Company)
                .Include(x => x.Dar)
                .OrderByDescending(x => x.Id)
                .Take(100)
                .ToListAsync();

        public async Task<List<DarDocument>> SearchAsync(bool? canceled, bool? paidout, int? period, int? darid, int? companyid)
        {
            IQueryable<DarDocument> result = _context.DarDocuments
                 .Include(x => x.Company)
                 .Include(x => x.Dar);

            if (canceled != null)
            {
                var cancel = Convert.ToBoolean(canceled.Value);
                result = result.Where(x => x.Canceled.Equals(cancel));
            }

            if (paidout != null)
            {
                var paid = Convert.ToBoolean(paidout.Value);
                result = result.Where(x => x.PaidOut.Equals(paid));
            }

            if (period != null)
                result = result.Where(x => x.PeriodReference.Equals(period.Value));

            if (darid != null)
                result = result.Where(x => x.DarId.Equals(darid.Value));

            if (companyid != null)
                result = result.Where(x => x.CompanyId.Equals(companyid.Value));


            var resultReturn = await result.ToListAsync();
            return resultReturn;
        }
    }
}
