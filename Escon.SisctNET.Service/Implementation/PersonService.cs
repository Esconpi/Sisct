using Escon.SisctNET.Model;
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

        public List<Person> Create(List<Person> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Person> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Person> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Person> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Person FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<Person> FindByProfileId(long profileId, Log log = null)
        {
            return _repository.FindByProfileId(profileId, log);
        }

        public Person Update(Person entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Person> Update(List<Person> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}