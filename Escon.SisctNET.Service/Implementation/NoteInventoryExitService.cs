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

        public NoteInventoryExit FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public NoteInventoryExit Update(NoteInventoryExit entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
