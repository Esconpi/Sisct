﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class CfopTypeService : ICfopTypeService
    {
        private readonly IRepository<Model.CfopType> _repository;

        public CfopTypeService(IRepository<CfopType> repository)
        {
            _repository = repository;
        }

        public CfopType Create(CfopType entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<CfopType> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<CfopType> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public CfopType FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public CfopType Update(CfopType entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
