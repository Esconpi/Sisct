using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class CountyService : ICountyService
    {
        private readonly ICountyRepository _repository;

        public CountyService(ICountyRepository repository)
        {
            _repository = repository;
        }

        public County Create(County entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<County> Create(List<County> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<County> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<County> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public County FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public County Update(County entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<County> Update(List<County> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
