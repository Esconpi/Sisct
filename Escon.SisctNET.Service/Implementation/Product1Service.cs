using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service.Implementation
{
    public class Product1Service : IProduct1Service
    {
        private readonly IProduct1Repository _repository;

        public Product1Service(IProduct1Repository repository)
        {
            _repository = repository;
        }

        public void Create(List<Product1> products, Log log = null)
        {
            _repository.Create(products,log);
        }

        public Product1 Create(Product1 entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Product1> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Product1> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Product1 FindByDescription(string description, Log log = null)
        {
            return _repository.FindByDescription(description, log);
        }

        public Product1 FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public decimal FindByPrice(int id, Log log = null)
        {
            return _repository.FindByPrice(id, log);
        }

        public Product1 FindByProduct(string code, int grupoId, Log log = null)
        {
            return _repository.FindByProduct(code, grupoId, log);
        }

        public Product1 Update(Product1 entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
