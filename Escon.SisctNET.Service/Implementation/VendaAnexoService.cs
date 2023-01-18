using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class VendaAnexoService : IVendaAnexoService
    {
        private readonly IVendaAnexoRepository _repository;

        public VendaAnexoService(IVendaAnexoRepository repository)
        {
            _repository = repository;
        }

        public VendaAnexo Create(VendaAnexo entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<VendaAnexo> Create(List<VendaAnexo> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<VendaAnexo> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<VendaAnexo> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<VendaAnexo> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public VendaAnexo FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<VendaAnexo> FindByVendasTax(long taxAnexo, Log log = null)
        {
            return _repository.FindByVendasTax(taxAnexo, log);
        }

        public VendaAnexo Update(VendaAnexo entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<VendaAnexo> Update(List<VendaAnexo> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
