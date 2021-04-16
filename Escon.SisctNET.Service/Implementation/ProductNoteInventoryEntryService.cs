using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class ProductNoteInventoryEntryService : IProductNoteInventoryEntryService
    {
        private readonly IProductNoteInventoryEntryRepository _repository;

        public ProductNoteInventoryEntryService(IProductNoteInventoryEntryRepository repository)
        {
            _repository = repository;
        }

        public ProductNoteInventoryEntry Create(ProductNoteInventoryEntry entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<ProductNoteInventoryEntry> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<ProductNoteInventoryEntry> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public ProductNoteInventoryEntry FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public ProductNoteInventoryEntry Update(ProductNoteInventoryEntry entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
