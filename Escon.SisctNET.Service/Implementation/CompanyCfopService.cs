﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service.Implementation
{
    public class CompanyCfopService : ICompanyCfopService
    {
        private readonly ICompanyCfopRepository _repository;

        public CompanyCfopService(ICompanyCfopRepository repository)
        {
            _repository = repository;
        }

        public CompanyCfop Create(CompanyCfop entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<CompanyCfop> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<CompanyCfop> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<CompanyCfop> FindByCompany(int companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public CompanyCfop FindByCompanyCfop(int companyId, int cfopId, Log log = null)
        {
            return _repository.FindByCompanyCfop(companyId, cfopId, log);
        }

        public CompanyCfop FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public CompanyCfop Update(CompanyCfop entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
