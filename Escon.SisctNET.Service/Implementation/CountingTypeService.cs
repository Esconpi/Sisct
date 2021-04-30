using Escon.SisctNET.Model;
using System.Collections.Generic;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class CountingTypeService : ICountingTypeService
    {
        private readonly ICountingTypeRepository _repository;

        public CountingTypeService(ICountingTypeRepository repository)
        {
            _repository = repository;
        }

        public CountingType Create(CountingType entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<CountingType> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<CountingType> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public CountingType FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public CountingType Update(CountingType entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
