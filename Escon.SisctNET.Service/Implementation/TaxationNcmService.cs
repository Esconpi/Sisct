using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;
using Escon.SisctNET.Repository;

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

        public void Create(List<TaxationNcm> taxationNcms, Log log = null)
        {
            _repository.Create(taxationNcms, log);
        }

        public void Delete(int id, Log log)
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

        public TaxationNcm FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<TaxationNcm> FindMono(int typeCompany, Log log = null)
        {
            return _repository.FindMono(typeCompany, log);
        }

        public TaxationNcm Update(TaxationNcm entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public void Update(List<TaxationNcm> taxationNcms, Log log = null)
        {
            _repository.Update(taxationNcms, log);
        }
    }
}
