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

        public void Delete(int id, Log log)
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

        public CompraAnexo FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public CompraAnexo Update(CompraAnexo entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
