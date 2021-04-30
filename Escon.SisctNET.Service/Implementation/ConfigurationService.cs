using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _repository;

        public ConfigurationService(IConfigurationRepository repository)
        {
            _repository = repository;
        }

        public Configuration Create(Configuration entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Configuration> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Configuration> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Configuration FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Configuration FindByName(string name, Log log = null)
        {
            return _repository.FindByName(name, log);
        }

        public Configuration Update(Configuration entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}