using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class TaxAnexoService : ITaxAnexoService
    {
        private readonly ITaxAnexoRepository _repository;

        public TaxAnexoService(ITaxAnexoRepository repository)
        {
            _repository = repository;
        }

        public TaxAnexo Create(TaxAnexo entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<TaxAnexo> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<TaxAnexo> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public TaxAnexo FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public TaxAnexo FindByMonth(int company, string mes, string ano, Log log = null)
        {
            return _repository.FindByMonth(company, mes, ano, log);
        }

        public TaxAnexo Update(TaxAnexo entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
