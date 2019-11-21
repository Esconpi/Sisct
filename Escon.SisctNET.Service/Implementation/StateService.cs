using System;
using System.Collections.Generic;
using Escon.SisctNET.Repository;
using Escon.SisctNET.Model;

namespace Escon.SisctNET.Service.Implementation
{
    public class StateService : IStateService
    {
        private readonly IStateRepository _repository;

        public StateService(IStateRepository repository)
        {
            _repository = repository;
        }

        public void Create(List<State> states, Log log = null)
        {
            _repository.Create(states);         
        }

        public State Create(State entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<State> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<State> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public State FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public State FindByUf(string uf, Log log = null)
        {
            return _repository.FindByUf(uf);
        }

        public State Update(State entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
