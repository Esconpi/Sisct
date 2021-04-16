using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

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

        public void Delete(int id, Log log)
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

        public ProductNoteInventoryExit FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public ProductNoteInventoryExit Update(ProductNoteInventoryExit entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
