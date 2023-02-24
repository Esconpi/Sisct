using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class Product1Service : IProduct1Service
    {
        private readonly IProduct1Repository _repository;

        public Product1Service(IProduct1Repository repository)
        {
            _repository = repository;
        }

        public Product1 Create(Product1 entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Product1> Create(List<Product1> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Product1> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Product1> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Product1> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Product1 FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Product1 FindByProduct(string code, long grupoId, Log log = null)
        {
            return _repository.FindByProduct(code, grupoId, log);
        }

        public Product1 Update(Product1 entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Product1> Update(List<Product1> entities, Log log)
        {
            return _repository.Update(entities, log);
        }

        public List<Product1> FindAllInDate(DateTime dateProd, Log log = null)
        {
            return _repository.FindAllInDate(dateProd, log);
        }

        public Product1 FindByProduct(long id, Log log = null)
        {
            return _repository.FindByProduct(id, log);
        }

        public List<Product1> FindByGroup(long groupid, Log log = null)
        {
            return _repository.FindByGroup(groupid, log);
        }

        public List<Product1> FindByAllGroup(Log log = null)
        {
            return _repository.FindByAllGroup(log);
        }
    }
}
