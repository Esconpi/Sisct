using Escon.SisctNET.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository
{
    public interface IDarDocumentRepository : IRepository<DarDocument>
    {
        Task<List<DarDocument>> ListFull();

        Task<List<DarDocument>> GetByCompanyIdAsync(int id);

        Task<DarDocument> GetByControlNumberAsync(int controlNumber);

        Task<List<DarDocument>> GetByControlNumberAsync(int[] controlNumber);

        Task<DarDocument> GetByCompanyAndPeriodReferenceAndDarAsync(int companyid, int period, int darId);

        Task<List<DarDocument>> GetByCompanyAndPeriodReferenceAsync(int companyid, int period, bool canceled);

        Task<List<int>> GetPeriodsReferenceAsync();

        Task<List<DarDocument>> SearchAsync(bool? canceled, bool? paidout, int? period, int? darid, int? companyid);

        Task<List<DarDocumentCompany>> FindByPeriodReferenceAsync(int periodReference, int? companyid);

        Task UpdateRangeAsync(List<DarDocument> documents);
    }
}