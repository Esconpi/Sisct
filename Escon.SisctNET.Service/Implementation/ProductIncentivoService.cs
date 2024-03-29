﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class ProductIncentivoService : IProductIncentivoService
    {

        private readonly IProductIncentivoRepository _repository;

        public ProductIncentivoService(IProductIncentivoRepository repository)
        {
            _repository = repository;
        }

        public ProductIncentivo Create(ProductIncentivo entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<ProductIncentivo> Create(List<ProductIncentivo> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<ProductIncentivo> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<ProductIncentivo> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<ProductIncentivo> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<ProductIncentivo> FindByAllProducts(long company, Log log = null)
        {
            return _repository.FindByAllProducts(company, log);
        }

        public List<ProductIncentivo> FindByAllProducts(string company, Log log = null)
        {
            return _repository.FindByAllProducts(company, log);
        }

        public List<ProductIncentivo> FindByDate(long company, DateTime date, Log log = null)
        {
            return _repository.FindByDate(company, date, log);
        }

        public List<ProductIncentivo> FindByDate(List<ProductIncentivo> productIncentivos, DateTime date, Log log = null)
        {
            return _repository.FindByDate(productIncentivos, date, log);
        }

        public ProductIncentivo FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public ProductIncentivo FindByProduct(long company, string code, string ncm,string cest, Log log = null)
        {
            return _repository.FindByProduct(company, code, ncm, cest, log);
        }

        public List<ProductIncentivo> FindByProducts(long id, string year, string month, Log log = null)
        {
            return _repository.FindByProducts(id, year, month, log);
        }

        public List<ProductIncentivo> FindByProducts(List<ProductIncentivo> productIncentivos, string ncmRaiz, Log log = null)
        {
            return _repository.FindByProducts(productIncentivos, ncmRaiz, log);
        }

        public ProductIncentivo Update(ProductIncentivo entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<ProductIncentivo> Update(List<ProductIncentivo> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
