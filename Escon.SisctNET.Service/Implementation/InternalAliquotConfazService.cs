using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class InternalAliquotConfazService : IInternalAliquotConfazService
    {
        private readonly IInternalAliquotConfazRepository _repository;

        public InternalAliquotConfazService(IInternalAliquotConfazRepository repository)
        {
            _repository = repository;
        }

        public InternalAliquotConfaz Create(InternalAliquotConfaz entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<InternalAliquotConfaz> Create(List<InternalAliquotConfaz> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<InternalAliquotConfaz> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<InternalAliquotConfaz> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<InternalAliquotConfaz> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<InternalAliquotConfaz> FindByAllState(Log log = null)
        {
            return _repository.FindByAllState(log);
        }

        public InternalAliquotConfaz FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public InternalAliquotConfaz Update(InternalAliquotConfaz entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<InternalAliquotConfaz> Update(List<InternalAliquotConfaz> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
