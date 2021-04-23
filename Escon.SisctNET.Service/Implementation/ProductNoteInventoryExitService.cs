using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
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

        public void Create(List<ProductNoteInventoryExit> products, Log log = null)
        {
            _repository.Create(products, log);
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

        public List<ProductNoteInventoryExit> FindByCompany(int companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public ProductNoteInventoryExit FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<ProductNoteInventoryExit> FindByNote(string chave, Log log = null)
        {
            return _repository.FindByNote(chave, log);
        }

        public List<ProductNoteInventoryExit> FindByNotes(int companyId, string year, string month, Log log = null)
        {
            return _repository.FindByNotes(companyId, year, month, log);
        }

        public List<ProductNoteInventoryExit> FindByPeriod(int companyId, DateTime inicio, DateTime fim, Log log = null)
        {
            return _repository.FindByPeriod(companyId, inicio, fim, log);
        }

        public ProductNoteInventoryExit Update(ProductNoteInventoryExit entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
