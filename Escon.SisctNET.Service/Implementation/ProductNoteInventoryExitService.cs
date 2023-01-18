using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service.Implementation
{
    public class ProductNoteInventoryExitService : IProductNoteInventoryExitService
    {
        private readonly IProductNoteInventoryExitRepository _repository;

        public ProductNoteInventoryExitService(IProductNoteInventoryExitRepository repository)
        {
            _repository = repository;
        }

        public ProductNoteInventoryExit Create(ProductNoteInventoryExit entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<ProductNoteInventoryExit> Create(List<ProductNoteInventoryExit> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public async Task CreateRange(List<ProductNoteInventoryExit> products, Log log = null)
        {
            await _repository.CreateRange(products, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<ProductNoteInventoryExit> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<ProductNoteInventoryExit> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<ProductNoteInventoryExit> FindByCompany(long companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public ProductNoteInventoryExit FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<ProductNoteInventoryExit> FindByNote(string chave, Log log = null)
        {
            return _repository.FindByNote(chave, log);
        }

        public List<ProductNoteInventoryExit> FindByNotes(long companyId, string year, string month, Log log = null)
        {
            return _repository.FindByNotes(companyId, year, month, log);
        }

        public List<ProductNoteInventoryExit> FindByNotes(long companyId, string year, Log log = null)
        {
            return _repository.FindByNotes(companyId, year, log);
        }

        public List<ProductNoteInventoryExit> FindByPeriod(long companyId, DateTime inicio, DateTime fim, Log log = null)
        {
            return _repository.FindByPeriod(companyId, inicio, fim, log);
        }

        public ProductNoteInventoryExit Update(ProductNoteInventoryExit entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<ProductNoteInventoryExit> Update(List<ProductNoteInventoryExit> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
