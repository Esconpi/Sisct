using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;
using Escon.SisctNET.Repository;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service.Implementation
{
    public class TaxationNcmService : ITaxationNcmService
    {
        private readonly ITaxationNcmRepository _repository;

        public TaxationNcmService(ITaxationNcmRepository repository)
        {
            _repository = repository;
        }

        public TaxationNcm Create(TaxationNcm entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<TaxationNcm> Create(List<TaxationNcm> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public async Task CreateRange(List<TaxationNcm> taxationNcms, Log log = null)
        {
            await _repository.CreateRange(taxationNcms, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<TaxationNcm> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<TaxationNcm> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<TaxationNcm> FindAllInDate(DateTime dateProd, Log log = null)
        {
            return _repository.FindAllInDate(dateProd, log);
        }

        public List<TaxationNcm> FindAllInDate(List<TaxationNcm> ncms, DateTime dateProd, Log log = null)
        {
            return _repository.FindAllInDate(ncms, dateProd, log);
        }

        public List<TaxationNcm> FindByCompany(string company, Log log = null)
        {
            return _repository.FindByCompany(company, log);
        }

        public List<TaxationNcm> FindByCompany(long company, Log log = null)
        {
            return _repository.FindByCompany(company, log);
        }

        public List<TaxationNcm> FindByCompany(long company, string year, string month, Log log = null)
        {
            return _repository.FindByCompany(company, year, month, log);
        }

        public List<TaxationNcm> FindByGeneral(Log log = null)
        {
            return _repository.FindByGeneral(log);
        }

        public TaxationNcm FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<TaxationNcm> FindByNcms(List<TaxationNcm> ncms, string ncmRaiz, Log log = null)
        {
            return _repository.FindByNcms(ncms, ncmRaiz, log);
        }

        public List<TaxationNcm> FindByPeriod(List<TaxationNcm> taxationNcms, DateTime inicio, DateTime fim, Log log = null)
        {
            return _repository.FindByPeriod(taxationNcms, inicio, fim, log);
        }

        public List<TaxationNcm> FindMono(long typeCompany, Log log = null)
        {
            return _repository.FindMono(typeCompany, log);
        }

        public TaxationNcm Update(TaxationNcm entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<TaxationNcm> Update(List<TaxationNcm> entities, Log log)
        {
            return _repository.Update(entities, log);
        }

        public async Task UpdateRange(List<TaxationNcm> taxationNcms, Log log = null)
        {
            await _repository.UpdateRange(taxationNcms, log);
        }
    }
}
