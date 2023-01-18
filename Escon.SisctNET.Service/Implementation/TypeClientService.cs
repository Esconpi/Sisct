using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class TypeClientService : ITypeClientService
    {
        private readonly ITypeClientRepository _repository;

        public TypeClientService(ITypeClientRepository repository)
        {
            _repository = repository;
        }

        public TypeClient Create(TypeClient entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<TypeClient> Create(List<TypeClient> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<TypeClient> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<TypeClient> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<TypeClient> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public TypeClient FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public TypeClient Update(TypeClient entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<TypeClient> Update(List<TypeClient> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
