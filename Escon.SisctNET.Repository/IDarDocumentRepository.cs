using Escon.SisctNET.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository
{
    public interface IDarDocumentRepository : IRepository<DarDocument>
    {
        Task<List<DarDocument>> ListFull();

        Task<List<DarDocument>> GetByCompanyIdAsync(long id);

        Task<DarDocument> GetByControlNumberAsync(int controlNumber);

        Task<List<DarDocument>> GetByControlNumberAsync(int[] controlNumber);

        Task<DarDocument> GetByCompanyAndPeriodReferenceAndDarAsync(long companyid, int period, long darId);

        Task<List<DarDocument>> GetByCompanyAndPeriodReferenceAsync(long companyid, int period, bool canceled);

        Task<List<int>> GetPeriodsReferenceAsync();

        Task<List<DarDocument>> SearchAsync(bool? canceled, bool? paidout, int? period, long? darid, long? companyid);

        Task<List<DarDocumentCompany>> FindByPeriodReferenceAsync(int periodReference, long? companyid);

        Task UpdateRangeAsync(List<DarDocument> documents);
    }
}