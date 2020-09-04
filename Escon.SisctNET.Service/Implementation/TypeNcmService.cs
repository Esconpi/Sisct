using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service.Implementation
{
    public class TypeNcmService : ITypeNcmService
    {
        private readonly ITypeNcmRepository _repository;

        public TypeNcmService(ITypeNcmRepository repository)
        {
            _repository = repository;
        }

        public TypeNcm Create(TypeNcm entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<TypeNcm> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<TypeNcm> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public TypeNcm FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public TypeNcm Update(TypeNcm entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
