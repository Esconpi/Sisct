﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;


namespace Escon.SisctNET.Service.Implementation
{
    public class InternalAliquotService : IInternalAliquotService
    {
        private readonly IInternalAliquotRepository _repository;

        public InternalAliquotService(IInternalAliquotRepository repository)
        {
            _repository = repository;
        }

        public InternalAliquot Create(InternalAliquot entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<InternalAliquot> Create(List<InternalAliquot> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<InternalAliquot> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<InternalAliquot> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<InternalAliquot> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public InternalAliquot FindByAliquot(long stateId, Log log = null)
        {
            return _repository.FindByAliquot(stateId, log);
        }

        public List<InternalAliquot> FindByAllState(Log log = null)
        {
            return _repository.FindByAllState(log);
        }

        public InternalAliquot FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public InternalAliquot FindByUf(List<InternalAliquot> aliquots, DateTime data, string uf, Log log = null)
        {
            return _repository.FindByUf(aliquots, data, uf, log);
        }

        public InternalAliquot Update(InternalAliquot entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<InternalAliquot> Update(List<InternalAliquot> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
