using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service.Implementation
{
    public class SuspensionService : ISuspensionService
    {
        private readonly ISuspensionRepository _repository;

        public SuspensionService(ISuspensionRepository repository)
        {
            _repository = repository;
        }

        public Suspension Create(Suspension entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Suspension> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Suspension> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Suspension> FindByCompany(int company, Log log = null)
        {
            return _repository.FindByCompany(company, log);
        }

        public Suspension FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Suspension Update(Suspension entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
