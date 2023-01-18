using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

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

        public List<TypeNcm> Create(List<TypeNcm> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
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

        public TypeNcm FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public TypeNcm Update(TypeNcm entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<TypeNcm> Update(List<TypeNcm> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
