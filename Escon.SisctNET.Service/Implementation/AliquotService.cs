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

        public void Create(List<Aliquot> states, Log log = null)
        {
            _repository.Create(states);         
        }

        public Aliquot Create(Aliquot entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Aliquot> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Aliquot> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
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

        public Aliquot FindByUf(List<Aliquot> states, DateTime data, string ufOrigem, string ufDestino, Log log = null)
        {
            return _repository.FindByUf(states, data, ufOrigem, ufDestino, log);
        }

        public Aliquot FindByUf(string ufOrigem, string ufDestino, DateTime data, Log log = null)
        {
            return _repository.FindByUf(ufOrigem, ufDestino, data, log);
        }

        public Aliquot Update(Aliquot entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
