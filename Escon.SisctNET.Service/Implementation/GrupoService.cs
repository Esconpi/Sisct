using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class GrupoService : IGrupoService
    {
        private readonly IGrupoRepository _repository;

        public GrupoService(IGrupoRepository repository)
        {
            _repository = repository;
        }

        public Grupo Create(Grupo entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Grupo> Create(List<Grupo> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Grupo> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Grupo> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Grupo> FindByGrupos(long taxid, Log log = null)
        {
            return _repository.FindByGrupos(taxid, log);
        }

        public Grupo FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Grupo Update(Grupo entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Grupo> Update(List<Grupo> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
