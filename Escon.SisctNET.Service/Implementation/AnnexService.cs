using System;
using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class AnnexService : IAnnexService
    {

        private readonly IAnnexRepository _repository;

        public AnnexService(IAnnexRepository repository)
        {
            _repository = repository;
        }

        public Annex Create(Annex entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Annex> Create(List<Annex> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Annex> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Annex> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Annex FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Annex Update(Annex entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Annex> Update(List<Annex> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
