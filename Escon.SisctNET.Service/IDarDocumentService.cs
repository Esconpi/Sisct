using Escon.SisctNET.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service
{
    public interface IDarDocumentService : IServiceBase<Model.DarDocument>
    {
        Task<List<DarDocument>> ListFull();

        Task<List<DarDocument>> GetByCompanyIdAsync(int id);

        Task<DarDocument> GetByCompanyAndPeriodReferenceAndDarAsync(int companyid, int period, int darId );

        Task<List<DarDocument>> GetByCompanyAndPeriodReferenceAsync(int companyid, int period, bool canceled);

        Task<List<int>> GetPeriodsReferenceAsync();

        Task<List<DarDocument>> SearchAsync(bool? canceled, bool? paidout, int? period, int? darid, int? companyid);

        Task<List<DarDocumentCompany>> FindByPeriodReferenceAsync(int periodReference, int? companyId);

        Task<DarDocument> GetByControlNumberAsync(int controlNumber);

        Task<List<DarDocument>> GetByControlNumberAsync(int[] controlNumber);

        Task UpdateRangeAsync(List<DarDocument> documents);
    }
}