using System;
using System.Collections.Generic;
using Escon.SisctNET.Repository;
using Escon.SisctNET.Model;

namespace Escon.SisctNET.Service.Implementation
{
    public class AliquotService : IAliquotService
    {
        private readonly IAliquotRepository _repository;

        public AliquotService(IAliquotRepository repository)
        {
            _repository = repository;
        }

        public Aliquot Create(Aliquot entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Aliquot> Create(List<Aliquot> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Aliquot> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Aliquot> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Aliquot> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Aliquot FindByAliquot(long stateOrigemId, long stateDestinoId, Log log = null)
        {
            return _repository.FindByAliquot(stateOrigemId, stateDestinoId, log);
        }

        public List<Aliquot> FindByAllState(Log log = null)
        {
            return _repository.FindByAllState(log);
        }

        public Aliquot FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Aliquot FindByUf(string uf, Log log = null)
        {
            return _repository.FindByUf(uf);
        }

        public Aliquot FindByUf(List<Aliquot> aliquots, DateTime data, string ufOrigem, string ufDestino, Log log = null)
        {
            return _repository.FindByUf(aliquots, data, ufOrigem, ufDestino, log);
        }

        public Aliquot FindByUf(string ufOrigem, string ufDestino, DateTime data, Log log = null)
        {
            return _repository.FindByUf(ufOrigem, ufDestino, data, log);
        }

        public Aliquot Update(Aliquot entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Aliquot> Update(List<Aliquot> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
