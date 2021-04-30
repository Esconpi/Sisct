using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service.Implementation
{
    public class DarDocumentService : IDarDocumentService
    {
        private readonly IDarDocumentRepository _repository;

        public DarDocumentService(IDarDocumentRepository repository)
        {
            _repository = repository;
        }

        public DarDocument Create(DarDocument entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<DarDocument> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<DarDocument> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public DarDocument FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public async Task<List<DarDocumentCompany>> FindByPeriodReferenceAsync(int periodReference, long? companyId) => await _repository.FindByPeriodReferenceAsync(periodReference, companyId);

        public async Task<DarDocument> GetByCompanyAndPeriodReferenceAndDarAsync(long companyid, int period, long darId) =>
            await _repository.GetByCompanyAndPeriodReferenceAndDarAsync(companyid, period, darId);

        public async Task<List<DarDocument>> GetByCompanyAndPeriodReferenceAsync(long companyid, int period, bool canceled) =>
            await _repository.GetByCompanyAndPeriodReferenceAsync(companyid, period, canceled);

        public async Task<List<DarDocument>> GetByCompanyIdAsync(long id) => await _repository.GetByCompanyIdAsync(id);

        public async Task<DarDocument> GetByControlNumberAsync(int controlNumber) => await _repository.GetByControlNumberAsync(controlNumber);

        public async Task<List<DarDocument>> GetByControlNumberAsync(int[] controlNumber) => await _repository.GetByControlNumberAsync(controlNumber);

        public async Task<List<int>> GetPeriodsReferenceAsync() => await _repository.GetPeriodsReferenceAsync();

        public async Task<List<DarDocument>> ListFull() => await _repository.ListFull();

        public async Task<List<DarDocument>> SearchAsync(bool? canceled, bool? paidout, int? period, long? darid, long? companyid) =>
            await _repository.SearchAsync(canceled, paidout, period, darid, companyid);

        public DarDocument Update(DarDocument entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public async Task UpdateRangeAsync(List<DarDocument> documents) => await _repository.UpdateRangeAsync(documents);
    }
}