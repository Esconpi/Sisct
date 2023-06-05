using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public Product Create(Product entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Product> Create(List<Product> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Product> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Product> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Product> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Product FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Product Update(Product entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Product> Update(List<Product> entities, Log log)
        {
            return _repository.Update(entities, log);
        }

        public List<Product> FindAllInDate(DateTime data, Log log = null)
        {
            return _repository.FindAllInDate(data, log);
        }

        public Product FindByProduct(long id, Log log = null)
        {
            return _repository.FindByProduct(id, log);
        }

        public Product FindByProduct(string code, long grupoId, Log log = null)
        {
            return _repository.FindByProduct(code, grupoId, log);
        }

        public List<Product> FindByGroup(long groupid, Log log = null)
        {
            return _repository.FindByGroup(groupid, log);
        }

        public List<Product> FindAllByGroup(Log log = null)
        {
            return _repository.FindAllByGroup(log);
        }

        public List<Product> FindAllByGroup(long groupid, Log log = null)
        {
            return _repository.FindAllByGroup(groupid, log);   
        }
    }
}
