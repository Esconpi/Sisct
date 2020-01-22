﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service.Implementation
{
    public class CstService : ICstService
    {
        private readonly ICstRepository _repository;

        public CstService(ICstRepository repository)
        {
            _repository = repository;
        }


        public Cst Create(Cst entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Cst> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Cst> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Cst FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Cst Update(Cst entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
