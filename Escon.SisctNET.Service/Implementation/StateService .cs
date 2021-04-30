using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service.Implementation
{
    public class StateService : IStateService
    {
        private readonly IStateRepository _repository;

        public StateService(IStateRepository repository)
        {
            _repository = repository;
        }

        public State Create(State entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
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

        public State FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public State Update(State entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
