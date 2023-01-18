﻿using System.Collections.Generic;
using System.Threading.Tasks;
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

        public List<ProductNote> Create(List<ProductNote> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public async Task CreateRange(List<ProductNote> products, Log log = null)
        {
            await _repository.CreateRange(products, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public async Task DeleteRange(List<ProductNote> products, Log log = null)
        {
            await _repository.DeleteRange(products, log);
        }

        public List<ProductNote> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<ProductNote> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public ProductNote FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public ProductNote Update(ProductNote entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<ProductNote> Update(List<ProductNote> entities, Log log)
        {
            return _repository.Update(entities, log);
        }

        public async Task UpdateRange(List<ProductNote> products, Log log = null)
        {
           await _repository.UpdateRange(products, log);
        }

        public List<ProductNote> FindByNote(long noteId, Log log = null)
        {
            return _repository.FindByNote(noteId, log);
        }

        public List<ProductNote> FindByProducts(List<Note> notes, Log log = null)
        {
            return _repository.FindByProducts(notes, log);
        }

        public List<ProductNote> FindByProductsType(List<Note> notes, Model.TypeTaxation taxationType, Log log = null)
        {
            return _repository.FindByProductsType(notes, taxationType, log);
        }

        public List<ProductNote> FindByNcmUfAliq(List<Note> notes, string ncm, decimal aliq,string cest, Log log = null)
        {
            return _repository.FindByNcmUfAliq(notes, ncm, aliq, cest, log);
        }

        public List<ProductNote> FindByCnpjCprod(List<Note> notes, string cnpj, string cprod, string ncm, string cest, Log log = null)
        {
            return _repository.FindByCnpjCprod(notes, cnpj, cprod, ncm, cest, log);
        }

        public decimal FindByTotal(List<long> notes, Log log = null)
        {
            return _repository.FindByTotal(notes, log);
        }

        public decimal FindBySubscription(List<Note> notes, Model.TypeTaxation taxationType, Log log = null)
        {
            return _repository.FindBySubscription(notes, taxationType, log);
        }

        public List<ProductNote> FindByTaxation(long noteId, Log log = null)
        {
            return _repository.FindByTaxation(noteId, log);
        }

        public ProductNote FindByProduct(long noteId, string nItem, Log log = null)
        {
            return _repository.FindByProduct(noteId, nItem, log);
        }

        public List<ProductNote> FindByCfopNotesIn(long companyId, List<Note> notes, Log log = null)
        {
            return _repository.FindByCfopNotesIn(companyId, notes, log);
        }

        public List<ProductNote> FindByCfopNotesOut(long companyId, List<Note> notes, Log log = null)
        {
            return _repository.FindByCfopNotesOut(companyId, notes, log);
        }

        public List<ProductNote> FindByIncentive(List<Note> notes, Log log = null)
        {
            return _repository.FindByIncentive(notes, log);
        }

        public List<ProductNote> FindByNormal(List<Note> notes, Log log = null)
        {
            return _repository.FindByNormal(notes, log);
        }

        public ProductNote FindByProduct(long id, Log log = null)
        {
            return _repository.FindByProduct(id, log);
        }

        public List<ProductNote> FindByProductsType(List<ProductNote> productNotes, TypeTaxation taxationType, Log log = null)
        {
            return _repository.FindByProductsType(productNotes, taxationType, log);
        }

        public List<ProductNote> FindByTaxation(List<ProductNote> productNotes, Log log = null)
        {
            return _repository.FindByTaxation(productNotes, log);
        }

        public List<ProductNote> FindByCompany(long companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }
    }
}
