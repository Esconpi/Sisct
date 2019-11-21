﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _repository;

        public PersonService(IPersonRepository repository)
        {
            _repository = repository;
        }

        public Person Create(Person entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Person> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Person> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Person FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<Person> FindByProfileId(int profileId, Log log = null)
        {
            return _repository.FindByProfileId(profileId, log);
        }

        public Person Update(Person entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}