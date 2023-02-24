using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class Product3Service : IProduct3Service
    {
        private readonly IProduct3Repository _repository;

        public Product3Service(IProduct3Repository repository)
        {
            _repository = repository;
        }

        public Product3 Create(Product3 entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Product3> Create(List<Product3> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Product3> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Product3> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Product3> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Product3> FindByGroup(long groupid, Log log = null)
        {
            return _repository.FindByGroup(groupid, log);
        }

        public Product3 FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Product3 FindByProduct(string code, long grupoId, Log log = null)
        {
            return _repository.FindByProduct(code, grupoId, log);
        }

        public Product3 Update(Product3 entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Product3> Update(List<Product3> entities, Log log)
        {
            return _repository.Update(entities, log);
        }

        public List<Product3> FindAllInDate(DateTime dateProd, Log log = null)
        {
            return _repository.FindAllInDate(dateProd, log);
        }

        public List<Product3> FindByAllGroup(Log log = null)
        {
            return _repository.FindByAllGroup(log);
        }

        public Product3 FindByProduct(long id, Log log = null)
        {
            return _repository.FindByProduct(id, log);
        }
    }
}
