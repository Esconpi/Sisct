using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class OccurrenceService : IOccurrenceService
    {
        private readonly IRepository<Occurrence> _repository;

        public OccurrenceService(IRepository<Occurrence> repository)
        {
            _repository = repository;
        }

        public Occurrence Create(Occurrence entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Occurrence> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Occurrence> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Occurrence FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Occurrence Update(Occurrence entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}