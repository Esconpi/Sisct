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

        public void Create(List<VendaAnexo> vendaAnexos, Log log = null)
        {
            _repository.Create(vendaAnexos, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
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

        public void Update(List<VendaAnexo> vendaAnexos, Log log = null)
        {
            _repository.Update(vendaAnexos, log);
        }
    }
}
