using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;
using System.Text;

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

        public void Create(List<ProductIncentivo> productIncentivos, Log log = null)
        {
            _repository.Create(productIncentivos, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<ProductIncentivo> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<ProductIncentivo> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<ProductIncentivo> FindByAllProducts(int company, Log log = null)
        {
            return _repository.FindByAllProducts(company, log);
        }

        public List<ProductIncentivo> FindByAllProducts(string company, Log log = null)
        {
            return _repository.FindByAllProducts(company, log);
        }

        public List<ProductIncentivo> FindByDate(int company, DateTime date, Log log = null)
        {
            return _repository.FindByDate(company, date, log);
        }

        public List<ProductIncentivo> FindByDate(List<ProductIncentivo> productIncentivos, DateTime date, Log log = null)
        {
            return _repository.FindByDate(productIncentivos, date, log);
        }

        public ProductIncentivo FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public ProductIncentivo FindByProduct(int company, string code, string ncm,string cest, Log log = null)
        {
            return _repository.FindByProduct(company, code, ncm, cest, log);
        }

        public List<ProductIncentivo> FindByProducts(int id, string year, string month, Log log = null)
        {
            return _repository.FindByProducts(id, year, month, log);
        }

        public ProductIncentivo Update(ProductIncentivo entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public void Update(List<ProductIncentivo> productIncentivos, Log log = null)
        {
            _repository.Update(productIncentivos, log);
        }
    }
}
