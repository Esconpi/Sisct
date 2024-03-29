﻿using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class NcmService : INcmService
    {

        private readonly INcmRepository _repository;

        public NcmService(INcmRepository repository)
        {
            _repository = repository;
        }

        public Ncm Create(Ncm entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Ncm> Create(List<Ncm> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Ncm> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Ncm> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Ncm> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Ncm FindByCode(string code, Log log = null)
        {
            return _repository.FindByCode(code);
        }

        public Ncm FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Ncm Update(Ncm entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Ncm> Update(List<Ncm> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}

