﻿using Escon.SisctNET.Model;
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

        public List<Product> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Product> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Product FindByDescription(string description, Log log = null)
        {
            return _repository.FindByDescription(description);
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

        public decimal FindByPrice(long id, Log log)
        {
            return _repository.FindByPrice(id, log);
        }

        public Product FindByProduct(string code, long grupoId, string description, Log log = null)
        {
            return _repository.FindByProduct(code, grupoId, description);
        }

        public List<Product> FindAllInDate(DateTime dateProd, Log log = null)
        {
            return _repository.FindAllInDate(dateProd, log);
        }

        public Product FindByProduct(long id, Log log = null)
        {
            return _repository.FindByProduct(id, log);
        }
    }
}
