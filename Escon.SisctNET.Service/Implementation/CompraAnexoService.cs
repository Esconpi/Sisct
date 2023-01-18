using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class CompraAnexoService : ICompraAnexoService
    {
        private readonly ICompraAnexoRepository _repository;

        public CompraAnexoService(ICompraAnexoRepository repository)
        {
            _repository = repository;
        }

        public CompraAnexo Create(CompraAnexo entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<CompraAnexo> Create(List<CompraAnexo> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<CompraAnexo> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<CompraAnexo> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public CompraAnexo FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<CompraAnexo> FindByComprasTax(long taxAnexo, Log log = null)
        {
            return _repository.FindByComprasTax(taxAnexo, log);
        }

        public CompraAnexo Update(CompraAnexo entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<CompraAnexo> Update(List<CompraAnexo> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
