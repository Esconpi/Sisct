﻿using System;
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

        public List<ProductNote> FindByNote(int noteId, Log log = null)
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

        public decimal FindByTotal(List<int> notes, Log log = null)
        {
            return _repository.FindByTotal(notes, log);
        }

        public decimal FindBySubscription(List<Note> notes, Model.TypeTaxation taxationType, Log log = null)
        {
            return _repository.FindBySubscription(notes, taxationType, log);
        }

        public List<ProductNote> FindByTaxation(int noteId, Log log = null)
        {
            return _repository.FindByTaxation(noteId, log);
        }

        public ProductNote FindByProduct(int noteId, string nItem, Log log = null)
        {
            return _repository.FindByProduct(noteId, nItem, log);
        }

        public List<ProductNote> FindByCfopNotesIn(int companyId, List<Note> notes, Log log = null)
        {
            return _repository.FindByCfopNotesIn(companyId, notes, log);
        }

        public List<ProductNote> FindByCfopNotesOut(int companyId, List<Note> notes, Log log = null)
        {
            return _repository.FindByCfopNotesOut(companyId, notes, log);
        }

        public bool FindByNcmAnnex(int Annex, string ncm, Log log = null)
        {
            return _repository.FindByNcmAnnex(Annex, ncm, log);
        }

        public List<ProductNote> FindByIncentive(List<Note> notes, Log log = null)
        {
            return _repository.FindByIncentive(notes, log);
        }

        public List<ProductNote> FindByNormal(List<Note> notes, Log log = null)
        {
            return _repository.FindByNormal(notes, log);
        }
        
        public void Create(List<ProductNote> products, Log log = null)
        {
            _repository.Create(products, log);
        }

        public void Delete(List<ProductNote> products, Log log = null)
        {
            _repository.Delete(products, log);
        }

        public void Update(List<ProductNote> products, Log log = null)
        {
            _repository.Update(products, log);
        }

        public ProductNote FindByProduct(int id, Log log = null)
        {
            return _repository.FindByProduct(id, log);
        }
    }
}
