using Escon.SisctNET.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository
{
    public interface IDarDocumentRepository : IRepository<DarDocument>
    {
        Task<List<DarDocument>> GetByCompanyIdAsync(int id);

        Task<DarDocument> GetByCompanyAndPeriodReferenceAndDarAsync(int companyid, int period, int darId);

        Task<List<DarDocument>> GetByCompanyAndPeriodReferenceAsync(int companyid, int period, bool canceled);
    }
}