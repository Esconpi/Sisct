using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class AliquotConfazService : IAliquotConfazService
    {
        private readonly IAliquotConfazRepository _repository;

        public AliquotConfazService(IAliquotConfazRepository repository)
        {
            _repository = repository;
        }

        public AliquotConfaz Create(AliquotConfaz entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<AliquotConfaz> Create(List<AliquotConfaz> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<AliquotConfaz> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<AliquotConfaz> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<AliquotConfaz> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public AliquotConfaz FindByAliquot(long stateOrigemId, long stateDestinoId, long annexId, Log log = null)
        {
            return _repository.FindByAliquot(stateOrigemId, stateDestinoId, annexId, log);
        }

        public List<AliquotConfaz> FindByAllState(Log log = null)
        {
            return _repository.FindByAllState(log);
        }

        public AliquotConfaz FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public AliquotConfaz Update(AliquotConfaz entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<AliquotConfaz> Update(List<AliquotConfaz> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
