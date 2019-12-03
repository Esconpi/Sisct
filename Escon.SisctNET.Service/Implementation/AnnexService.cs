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

        public void Delete(int id, Log log)
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

        public Annex FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Annex Update(Annex entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
