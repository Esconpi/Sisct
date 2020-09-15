﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class Product2Service : IProduct2Service
    {
        private readonly IProduct2Repository _repository;

        public Product2Service(IProduct2Repository repository)
        {
            _repository = repository;
        }

        public void Create(List<Product2> products, Log log = null)
        {
            _repository.Create(products, log);
        }

        public Product2 Create(Product2 entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Product2> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Product2> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Product2 FindByDescription(string description, Log log = null)
        {
            return _repository.FindByDescription(description, log);
        }

        public Product2 FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public decimal FindByPrice(int id, Log log = null)
        {
            return _repository.FindByPrice(id, log);
        }

        public Product2 FindByProduct(string code, int grupoId, Log log = null)
        {
            return _repository.FindByProduct(code, grupoId, log);
        }

        public Product2 Update(Product2 entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public void Update(List<Product2> products, Log log = null)
        {
            _repository.Update(products, log);
        }
    }
}
