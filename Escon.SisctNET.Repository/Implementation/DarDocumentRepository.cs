using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public async Task<List<DarDocument>> ListFull() =>
            await _context.DarDocuments
                .Include(x => x.Company)
                .Include(x => x.Dar)
                .OrderByDescending(x => x.Id)
                .Take(100)
                .ToListAsync();
    }
}
