using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

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

        public void Delete(int id, Log log)
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

        public DarDocument FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public DarDocument Update(DarDocument entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
