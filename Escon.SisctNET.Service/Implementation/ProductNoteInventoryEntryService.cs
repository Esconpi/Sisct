﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class ProductNoteInventoryEntryService : IProductNoteInventoryEntryService
    {
        private readonly IProductNoteInventoryEntryRepository _repository;

        public ProductNoteInventoryEntryService(IProductNoteInventoryEntryRepository repository)
        {
            _repository = repository;
        }

        public ProductNoteInventoryEntry Create(ProductNoteInventoryEntry entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Create(List<ProductNoteInventoryEntry> products, Log log = null)
        {
            _repository.Create(products, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<ProductNoteInventoryEntry> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<ProductNoteInventoryEntry> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<ProductNoteInventoryEntry> FindByCompany(int companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public ProductNoteInventoryEntry FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<ProductNoteInventoryEntry> FindByNote(string chave, Log log = null)
        {
            return _repository.FindByNote(chave, log);
        }

        public List<ProductNoteInventoryEntry> FindByNotes(int companyId, string year, string month, Log log = null)
        {
            return _repository.FindByNotes(companyId, year, month, log);
        }

        public List<ProductNoteInventoryEntry> FindByPeriod(int companyId, DateTime inicio, DateTime fim, Log log = null)
        {
            return _repository.FindByPeriod(companyId, inicio, fim, log);
        }

        public ProductNoteInventoryEntry Update(ProductNoteInventoryEntry entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
