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

        public List<NoteInventoryEntry> FindByCompany(int companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public NoteInventoryEntry FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public NoteInventoryEntry FindByNote(string chave, Log log = null)
        {
            return _repository.FindByNote(chave, log);
        }

        public NoteInventoryEntry FindByNote(int id, Log log = null)
        {
            return _repository.FindByNote(id, log);
        }

        public List<NoteInventoryEntry> FindByNotes(int id, string year, string month, Log log = null)
        {
            return _repository.FindByNotes(id, year, month);
        }

        public NoteInventoryEntry Update(NoteInventoryEntry entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
