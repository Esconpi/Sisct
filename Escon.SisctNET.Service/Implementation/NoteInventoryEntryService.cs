using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class NoteInventoryEntryService : INoteInventoryEntryService
    {
        private readonly INoteInventoryEntryRepository _repository;

        public NoteInventoryEntryService(INoteInventoryEntryRepository repository)
        {
            _repository = repository;
        }

        public NoteInventoryEntry Create(NoteInventoryEntry entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<NoteInventoryEntry> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<NoteInventoryEntry> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public NoteInventoryEntry FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public NoteInventoryEntry Update(NoteInventoryEntry entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
