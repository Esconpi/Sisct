﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service.Implementation
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _repository;

        public CompanyService(ICompanyRepository repository)
        {
            _repository = repository;
        }

        public Company Create(Company entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Company> Create(List<Company> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Company> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Company> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Company> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Company FindByCode(string code, Log log = null)
        {
            return _repository.FindByCode(code);
        }

        public Company FindByDocument(string document, Log log = null)
        {
            return _repository.FindByDocument(document);
        }

        public Company FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Company Update(Company entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Company> Update(List<Company> entities, Log log)
        {
            return _repository.Update(entities, log);
        }

        public List<Model.Company> FindByCompanies(Log log = null)
        {
            return _repository.FindByCompanies(log);
        }

        public async Task<List<Company>> ListAllActiveAsync(Log log) => await _repository.ListAllActiveAsync(log);

        public List<Company> FindByCompanies(string company, Log log = null)
        {
            return _repository.FindByCompanies(company, log);
        }
    }
}
