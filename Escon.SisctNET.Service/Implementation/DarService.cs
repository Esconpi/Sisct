using System;
using System.Collections.Generic;
using System.Text;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class DarService : IDarService
    {
        private readonly IDarRepository _repository;

        public DarService(IDarRepository repository)
        {
            _repository = repository;
        }

        public Dar Create(Dar entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Dar> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Dar> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Dar FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Dar Update(Dar entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
