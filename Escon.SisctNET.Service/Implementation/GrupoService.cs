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

        public void Create(List<Grupo> grupos, Log log = null)
        {
            _repository.Create(grupos, log);
        }

        public void Delete(int id, Log log)
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

        public List<Grupo> FindByGrupos(int taxid, Log log = null)
        {
            return _repository.FindByGrupos(taxid, log);
        }

        public Grupo FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Grupo Update(Grupo entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public void Update(List<Grupo> grupos, Log log = null)
        {
            _repository.Update(grupos, log);
        }
    }
}
