using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class SectionService : ISectionService
    {
        private readonly ISectionRepository _repository;

        public SectionService(ISectionRepository repository)
        {
            _repository = repository;
        }

        public Section Create(Section entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Section> Create(List<Section> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Section> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Section> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Section> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Section FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Section Update(Section entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Section> Update(List<Section> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
