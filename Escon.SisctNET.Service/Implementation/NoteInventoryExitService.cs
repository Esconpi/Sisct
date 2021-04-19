using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class NoteInventoryExitService : INoteInventoryExitService
    {
        private readonly INoteInventoryExitRepository _repository;

        public NoteInventoryExitService(INoteInventoryExitRepository repository)
        {
            _repository = repository;
        }

        public NoteInventoryExit Create(NoteInventoryExit entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<NoteInventoryExit> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<NoteInventoryExit> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<NoteInventoryExit> FindByCompany(int companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public NoteInventoryExit FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public NoteInventoryExit FindByNote(string chave, Log log = null)
        {
            return _repository.FindByNote(chave, log);
        }

        public NoteInventoryExit FindByNote(int id, Log log = null)
        {
            return _repository.FindByNote(id, log);
        }

        public List<NoteInventoryExit> FindByNotes(int id, string year, string month, Log log = null)
        {
            return _repository.FindByNotes(id, year, month, log);
        }

        public NoteInventoryExit Update(NoteInventoryExit entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
