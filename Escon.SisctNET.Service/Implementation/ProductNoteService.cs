using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class ProductNoteService : IProductNoteService
    {
        private readonly IProductNoteRepository _repository;

        public ProductNoteService(IProductNoteRepository repository)
        {
            _repository = repository;
        }

        public ProductNote Create(ProductNote entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<ProductNote> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<ProductNote> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public ProductNote FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public ProductNote Update(ProductNote entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<ProductNote> FindByNotes(int noteId, Log log = null)
        {
            return _repository.FindByNotes(noteId, log);
        }

        public List<ProductNote> FindByProducts(List<Note> notes, Log log = null)
        {
            return _repository.FindByProducts(notes, log);
        }

        public List<ProductNote> FindByProductsType(List<Note> notes, int taxationType, Log log = null)
        {
            return _repository.FindByProductsType(notes, taxationType, log);
        }

        public List<ProductNote> FindByNcmUfAliq(List<Note> notes, string ncm, decimal aliq, Log log = null)
        {
            return _repository.FindByNcmUfAliq(notes, ncm, aliq, log);
        }

        public List<ProductNote> FindByCnpjCprod(List<Note> notes, string cnpj, string cprod, string ncm, string cest, Log log = null)
        {
            return _repository.FindByCnpjCprod(notes, cnpj, cprod, ncm, cest, log);
        }

        public decimal FindByTotal(List<string> notes, Log log = null)
        {
            return _repository.FindByTotal(notes, log);
        }

        public decimal FindBySubscription(List<Note> notes, int taxaid, Log log = null)
        {
            return _repository.FindBySubscription(notes, taxaid, log);
        }

        public List<ProductNote> FindByTaxation(int noteId, Log log = null)
        {
            return _repository.FindByTaxation(noteId, log);
        }

        public ProductNote FindByProduct(int noteId, string nItem, Log log = null)
        {
            return _repository.FindByProduct(noteId, nItem, log);
        }
    }
}
