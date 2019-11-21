﻿using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class FunctionalityService : IFunctionalityService
    {
        Repository.IFunctionalityRepository _repository;

        public FunctionalityService(Repository.IFunctionalityRepository repository)
        {
            _repository = repository;
        }

        public Functionality Create(Functionality entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Functionality> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Functionality> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Functionality FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Functionality FindByName(string name, Log log)
        {
            return _repository.FindByName(name, log);
        }

        public Functionality Update(Functionality entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}