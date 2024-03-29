﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class TaxationTypeService : ITaxationTypeService
    {
        private readonly ITaxationTypeRepository _repository;

        public TaxationTypeService(ITaxationTypeRepository repository)
        {
            _repository = repository;
        }

        public TaxationType Create(TaxationType entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<TaxationType> Create(List<TaxationType> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<TaxationType> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<TaxationType> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<TaxationType> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public TaxationType FindByDescription(string description, Log log = null)
        {
            return _repository.FindByDescription(description);
        }

        public TaxationType FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public TaxationType Update(TaxationType entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<TaxationType> Update(List<TaxationType> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
